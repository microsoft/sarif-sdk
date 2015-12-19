// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Configuration
{
    /// <summary>Base class for configuration objects, which read user-supplied configuration from the command line, response files, and XML configuration files.</summary>
    /// <seealso cref="System.Xml.Serialization.IXmlSerializable"/>
    public abstract class ConfigurationBase : IXmlSerializable
    {
        /// <summary>A regular expression which determines which field names are valid in the XML and on the CLI.</summary>
        private static readonly Regex s_validName = new Regex(@"^\w[\w-]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>List of valid primitive types.</summary>
        private static readonly Type[] s_validTypes = new Type[] { typeof(int), typeof(uint), typeof(double), typeof(bool), typeof(string), typeof(long), typeof(ulong), typeof(Version) };

        /// <summary>User readable names corresponding to the valid primitive types.</summary>
        private static readonly string[] s_typeNames = new string[] { "int", "uint", "double", "bool", "string", "long", "ulong", "version" };

        /// <summary>The usage text.</summary>
        private readonly List<string> _usage;

        /// <summary>The fields which are configured for assignment when parsing arguments or configuration files.</summary>
        private readonly Dictionary<PropertyInfo, FieldAttribute> _fields;

        /// <summary>The names of the configured fields.</summary>
        private readonly Dictionary<string, PropertyInfo> _names;

        /// <summary>The short forms of the names of the configured fields.</summary>
        private readonly Dictionary<string, PropertyInfo> _shortNames;

        /// <summary>The set of fields which are required.</summary>
        private readonly List<PropertyInfo> _required;

        /// <summary>The default field.</summary>
        private readonly PropertyInfo _defaultField;

        /// <summary>Initializes a new instance of the <see cref="ConfigurationBase"/> class.</summary>
        /// <param name="banner">The banner text, if any. This field may be null.</param>
        protected ConfigurationBase(string banner)
            : this()
        {
            this.Banner = banner;
        }

        private List<KeyValuePair<PropertyInfo, FieldAttribute>> GetTargetProperties()
        {
            var result = new List<KeyValuePair<PropertyInfo, FieldAttribute>>();
            foreach (PropertyInfo rawProperty in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty))
            {
                FieldAttribute propertySettings = rawProperty.GetCustomAttribute<FieldAttribute>();
                if (propertySettings == null)
                {
                    continue;
                }

                // set the field name to the name of the member itself if it wasn't specified
                if (propertySettings.Name == null)
                {
                    propertySettings.Name = rawProperty.Name;
                }

                result.Add(new KeyValuePair<PropertyInfo, FieldAttribute>(rawProperty, propertySettings));
            }

            result.Sort((x, y) => StringComparer.CurrentCultureIgnoreCase.Compare(x.Value.Name, y.Value.Name));
            return result;
        }

        /// <summary>Initializes a new instance of the <see cref="ConfigurationBase"/> class.</summary>
        /// <exception cref="ArgumentException">Thrown when a class derived from
        /// <see cref="ConfigurationBase"/> contains <see cref="FieldAttribute"/> settings which the
        /// parser is incapable of handling.</exception>
        protected ConfigurationBase()
        {
            _fields = new Dictionary<PropertyInfo, FieldAttribute>();
            _names = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            _shortNames = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            _required = new List<PropertyInfo>();
            _usage = new List<string>();

            foreach (KeyValuePair<PropertyInfo, FieldAttribute> fieldPair in this.GetTargetProperties())
            {
                PropertyInfo field = fieldPair.Key;
                FieldAttribute fieldAttr = fieldPair.Value;

                // validation
                if (!s_validName.IsMatch(fieldAttr.Name))
                {
                    throw new ArgumentException("Field name " + fieldAttr.Name + " contains invalid characters");
                }

                if (fieldAttr.ShortName != null && !s_validName.IsMatch(fieldAttr.ShortName))
                {
                    throw new ArgumentException("Field short name " + fieldAttr.ShortName + " contains invalid characters");
                }

                if (_names.ContainsKey(fieldAttr.Name))
                {
                    throw new ArgumentException("Field name " + fieldAttr.Name + " cannot be specified on multiple fields");
                }

                if (_shortNames.ContainsKey(fieldAttr.Name))
                {
                    throw new ArgumentException("Field name " + fieldAttr.Name + " conflicts with short name specified on another field");
                }

                if (fieldAttr.ShortName != null && _shortNames.ContainsKey(fieldAttr.ShortName))
                {
                    throw new ArgumentException("Field short name " + fieldAttr.ShortName + " cannot be specified on multiple fields");
                }

                if (fieldAttr.ShortName != null && _names.ContainsKey(fieldAttr.ShortName))
                {
                    throw new ArgumentException("Field short name " + fieldAttr.ShortName + " conflicts with full name specified on another field");
                }

                if (!IsValidType(field.PropertyType))
                {
                    throw new ArgumentException("Field type " + field.PropertyType.Name + " cannot be parsed");
                }

                if ((fieldAttr.Type & FieldTypes.Default) != FieldTypes.None && _defaultField != null)
                {
                    throw new ArgumentException("Cannot have multiple default fields");
                }

                // add to dict
                _fields.Add(field, fieldAttr);
                if (fieldAttr.ShortName != null)
                {
                    _shortNames.Add(fieldAttr.ShortName, field);
                }

                _names.Add(fieldAttr.Name, field);
                this.AddToUsage(field, fieldAttr);

                if ((fieldAttr.Type & FieldTypes.Default) != FieldTypes.None)
                {
                    if (GetNormalizedType(field.PropertyType) == typeof(bool))
                    {
                        throw new ArgumentException("Default argument cannot be a boolean");
                    }

                    _defaultField = field;
                }

                if ((fieldAttr.Type & FieldTypes.Required) != FieldTypes.None)
                {
                    _required.Add(field);
                }
            }
        }

        /// <summary>Gets the banner text.</summary>
        /// <value>The banner text.</value>
        public string Banner { get; private set; }

        /// <summary>Gets the usage text.</summary>
        /// <value>The usage text.</value>
        public string Usage
        {
            get
            {
                int windowWidth = 80;
                try
                {
                    windowWidth = Console.WindowWidth;
                }
                catch (System.IO.IOException)
                {
                }

                return CreateUsage(windowWidth);
            }
        }

        /// <summary>
        /// Creates usage text assuming a console <paramref name="windowWidth"/> wide.
        /// </summary>
        /// <param name="windowWidth">How wide a console to generate usage text for.</param>
        /// <returns>Usage text for this <see cref="ConfigurationBase"/> instance.</returns>
        public string CreateUsage(int windowWidth)
        {
            int maxLeftSide = 0; // maximum length of the left side information
            for (int i = 0; i < _usage.Count; i += 2)
            {
                if (_usage[i].Length > maxLeftSide)
                {
                    maxLeftSide = _usage[i].Length;
                }
            }

            bool sameLine = maxLeftSide + 9 < windowWidth - 20; // put args and usage on same line

            int leftSideWidth = maxLeftSide + 9;
            StringBuilder usageBuilder = new StringBuilder();
            if (this.Banner != null)
            {
                usageBuilder.AppendLine(this.Banner);
            }

            usageBuilder.AppendLine("Usage:");
            if (sameLine)
            {
                for (int i = 0; i < _usage.Count / 2; i++)
                {
                    usageBuilder.AppendLine(FormatUsage(leftSideWidth, windowWidth, _usage[i * 2], _usage[(i * 2) + 1]));
                }
            }
            else
            {
                for (int i = 0; i < _usage.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        usageBuilder.AppendLine();
                        usageBuilder.Append(_usage[i]);
                        usageBuilder.AppendLine();
                    }
                    else
                    {
                        usageBuilder.Append(_usage[i]);
                    }
                }
            }

            return usageBuilder.ToString();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <seealso cref="System.Object.ToString()"/>
        public override string ToString()
        {
            return this.ToString(false);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <param name="showHidden">true to show hidden configuration options, false to hide them</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(bool showHidden)
        {
            List<string> args = new List<string>();
            foreach (string name in _names.Keys)
            {
                PropertyInfo fi = _names[name];

                // if it's not hidden
                if ((_fields[fi].Type & FieldTypes.Hidden) == FieldTypes.None || showHidden)
                {
                    object v = fi.GetValue(this, null);
                    if (v != null)
                    {
                        args.Add(ArgToString(fi.PropertyType, name, v));
                    }
                }
            }

            return string.Join(" ", args.ToArray());
        }

        /// <summary>Writes the configuration currently stored in this instance to file as an XML configuration.</summary>
        /// <param name="filePath">Full path where the XML configuration shall be written.</param>
        public void WriteConfigurationToFile(string filePath)
        {
            using (XmlWriter xmlWriter = XmlWriter.Create(filePath))
            {
                this.WriteXml(xmlWriter);
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        /// <seealso cref="System.Xml.Serialization.IXmlSerializable.WriteXml(XmlWriter)"/>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(this.GetClassName());
            writer.WriteAttributeString("version", this.GetType().Assembly.GetName().Version.ToString());

            foreach (string name in _names.Keys)
            {
                PropertyInfo fi = _names[name];

                // if it's not CLI Only
                if ((_fields[fi].Type & FieldTypes.CliOnly) == FieldTypes.None)
                {
                    object v = fi.GetValue(this, null);
                    if (v != null)
                    {
                        if (IsArray(fi.PropertyType))
                        {
                            foreach (object o in (Array)v)
                            {
                                writer.WriteStartElement(name);
                                this.WriteType(fi.PropertyType.GetElementType(), o, writer);
                                writer.WriteEndElement();
                            }
                        }
                        else
                        {
                            writer.WriteStartElement(name);
                            this.WriteType(fi.PropertyType, v, writer);
                            writer.WriteEndElement();
                        }
                    }
                }
            }

            writer.WriteEndElement(); // </classname>
            writer.Flush();
        }

        /// <summary>Print usage string to Console.Out.</summary>
        public virtual void PrintUsage()
        {
            Console.WriteLine(this.Usage);
        }

        /// <summary>Parses an argument list.</summary>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string args)
        {
            string error;
            if (this.ParseArgs(args, out error))
            {
                return true;
            }
            else
            {
                Console.Error.WriteLine(error);
                return false;
            }
        }

        /// <summary>Parses an argument list.</summary>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="errors">[out] Returns a dictionary into which parse errors will be placed, where
        /// the key is the name of the argument the user provided, and the value is an error message
        /// describing that argument.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(IList<string> args, out Dictionary<string, string> errors)
        {
            return this.ParseArgs(null, args, out errors);
        }

        /// <summary>Parses an argument string.</summary>
        /// <param name="args">A command line. This command line will be split into individual arguments
        /// using the system function <c>CommandLineToArgVW</c>
        /// (http://msdn.microsoft.com/en-us/library/windows/desktop/bb776391.aspx)</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string args, out string error)
        {
            return this.ParseArgs(args, true, out error);
        }

        /// <summary>Parses the argument string.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="args">A command line. This command line will be split into individual arguments
        /// using the system function <c>CommandLineToArgVW</c>
        /// (http://msdn.microsoft.com/en-us/library/windows/desktop/bb776391.aspx)</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <param name="allowConfigFileLoad">Allow or disallow file load with @filename.</param>
        /// <param name="error">[out] The human readable error string generated.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string args, bool allowConfigFileLoad, out string error)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            return this.ParseArgs(ArgumentSplitter.CommandLineToArgvW(args), allowConfigFileLoad, out error);
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(IList<string> args)
        {
            string error;
            if (this.ParseArgs(args, out error))
            {
                return true;
            }
            else
            {
                Console.Error.WriteLine(error);
                return false;
            }
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(IList<string> args, out string error)
        {
            return this.ParseArgs(null, args, out error);
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="allowConfigFileLoad">Allow file load with @filename.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(IList<string> args, bool allowConfigFileLoad, out string error)
        {
            return this.ParseArgs(null, args, allowConfigFileLoad, out error);
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="defaultConfig">The default configuration file (this value can be null)</param>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string defaultConfig, IList<string> args, out string error)
        {
            return this.ParseArgs(defaultConfig, args, true, out error);
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="defaultConfig">The default configuration file (this value can be null)</param>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="allowConfigFileLoad">If <c>true</c>, allows the parser to load data from XML
        /// configuration files and console response files; otherwise disallows these actions.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string defaultConfig, IList<string> args, bool allowConfigFileLoad, out string error)
        {
            error = null;
            Dictionary<string, string> errors;
            bool status = this.ParseArgs(defaultConfig, args, allowConfigFileLoad, out errors);
            if (status)
            {
                return true;
            }

            error = GenerateErrorString(errors);
            return false;
        }

        /// <summary>Parses the argument list.</summary>
        /// <param name="defaultConfig">The default configuration file (this value can be null)</param>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="errors">[out] Returns a dictionary into which parse errors will be placed, where
        /// the key is the name of the argument the user provided, and the value is an error message
        /// describing that argument.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string defaultConfig, IList<string> args, out Dictionary<string, string> errors)
        {
            return this.ParseArgs(defaultConfig, args, true, out errors);
        }

        /// <summary>Parses the argument list.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="TargetInvocationException">Thrown when a target invocation error condition
        /// occurs.</exception>
        /// <param name="defaultConfig">The default configuration file (this value can be null)</param>
        /// <param name="args">The array of arguments, similar to <c>argv</c>.</param>
        /// <param name="allowConfigFileLoad">If <c>true</c>, allows the parser to load data from XML
        /// configuration files and console response files; otherwise disallows these actions.</param>
        /// <param name="errors">[out] Returns a dictionary into which parse errors will be placed, where
        /// the key is the name of the argument the user provided, and the value is an error message
        /// describing that argument.</param>
        /// <returns>
        /// <c>true</c> if a program should continue running after this method returns, or <c>false</c>
        /// if the program should terminate. (For example, returns <c>true</c> when the parse is
        /// successful, <c>false</c> if an erroneous argument is provided, and <c>false</c> if the user
        /// requested usage with /?)
        /// </returns>
        public bool ParseArgs(string defaultConfig, IList<string> args, bool allowConfigFileLoad, out Dictionary<string, string> errors)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            ConfigurationParseContext parseContext = new ConfigurationParseContext();
            bool status = ParseArgsImpl(defaultConfig, args, allowConfigFileLoad, parseContext);

            // Set arrays to their parsed values
            foreach (KeyValuePair<PropertyInfo, List<object>> arrayArgumentValue in parseContext.MultipleUseArgumentValues)
            {
                List<object> o = arrayArgumentValue.Value;
                Array a = Array.CreateInstance(arrayArgumentValue.Key.PropertyType.GetElementType(), o.Count);
                for (int i = 0; i < o.Count; i++)
                {
                    a.SetValue(o[i], i);
                }

                try
                {
                    arrayArgumentValue.Key.SetValue(this, a, null);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is ArgumentException)
                    {
                        parseContext.AddCommandLineError(arrayArgumentValue.Key.Name, ex.InnerException.Message);
                        status = false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            errors = parseContext.GenerateErrorDictionary();
            if (errors.Count != 0)
            {
                status = false;
            }

            // Validate is only run if we parsed OK
            if (status)
            {
                return this.Validate(errors);
            }

            return status;
        }

        private bool ParseArgsImpl(string defaultConfig, IList<string> args, bool allowConfigFileLoad, ConfigurationParseContext parseContext)
        {
            PropertyInfo lastArg = null;
            bool status = true;

            if (defaultConfig != null)
            {
                if (!ParseXmlConfigFileSafe(parseContext, defaultConfig))
                {
                    status = false;
                }
            }

            bool lastArgWasUnknown = false;
            for (int idx = 0; idx < args.Count;)
            {
                string arg = args[idx];

                if (arg.StartsWith("@", StringComparison.Ordinal) && allowConfigFileLoad)
                {
                    string configFileName = arg.Substring(1);
                    if (configFileName.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
                    {
                        status = status && ParseResponseFile(parseContext, configFileName);
                    }
                    else
                    {
                        status = status && ParseXmlConfigFileSafe(parseContext, configFileName);
                    }

                    idx++;
                    lastArg = null;
                    lastArgWasUnknown = false;
                }
                else if (arg.Equals("/?", StringComparison.Ordinal) || arg.Equals("-?", StringComparison.Ordinal) || arg.Equals("--?", StringComparison.Ordinal))
                {
                    this.PrintUsage();
                    if (allowConfigFileLoad)
                    {
                        Console.Out.WriteLine("Invoke with @file.rsp (with .rsp extension) to load console response file.");
                        Console.Out.WriteLine("Invoke with @file.xml (with anything but .rsp extension) to load XML config.");
                    }

                    parseContext.RemoveAllErrors();
                    return false;
                }
                else if (arg.StartsWith("/", StringComparison.Ordinal) || arg.StartsWith("-", StringComparison.Ordinal))
                {
                    // parse arg value
                    string name = arg.Substring(1);
                    if (_names.TryGetValue(name, out lastArg))
                    {
                    }
                    else if (_shortNames.TryGetValue(name, out lastArg))
                    {
                        name = _fields[lastArg].Name;
                    }
                    else if (name.EndsWith("-", StringComparison.Ordinal))
                    {
                        name = name.Substring(0, name.Length - 1);
                        if (this.TryFindArgumentByName(name, out lastArg))
                        {
                            lastArgWasUnknown = false;
                            Type normalizedType = GetNormalizedType(lastArg.PropertyType);
                            if (normalizedType == typeof(bool) && HasNoMinusAttribute(lastArg))
                            {
                                parseContext.AddCommandLineError(name, "unknown argument (the negated version of this flag is explicitly disallowed)");
                                status = false;
                                lastArg = null;
                                idx++;
                            }
                            else if (normalizedType != typeof(bool))
                            {
                                parseContext.AddCommandLineError(name, "boolean switch used with non-boolean argument");
                                status = false;
                                lastArg = null;
                                idx++;
                            }
                        }
                        else
                        {
                            parseContext.AddCommandLineError(name, "unknown argument");
                            status = false;
                            lastArgWasUnknown = true;
                            idx++;
                        }
                    }
                    else
                    {
                        // error
                        parseContext.AddCommandLineError(name, "unknown argument");
                        status = false;
                        lastArgWasUnknown = true;
                        idx++;
                    }

                    if (lastArg != null && parseContext.FlagsSetOnCommandLine.Contains(lastArg) && (!IsArray(lastArg.PropertyType)))
                    {
                        parseContext.AddCommandLineError(name, "specified multiple times");
                        status = false;
                    }

                    if (lastArg != null && GetNormalizedType(lastArg.PropertyType) != typeof(bool))
                    {
                        lastArgWasUnknown = false;
                        idx++;
                    }
                }
                else if (_defaultField != null)
                {
                    lastArg = _defaultField;
                }
                else
                {
                    if (!lastArgWasUnknown)
                    {   // if the last arg was a typo, this probably isn't a separate error
                        parseContext.AddCommandLineError(string.Empty, "default argument given but there is no default argument");
                        status = false;
                    }

                    idx++; // try to keep parsing
                }

                if (lastArg != null)
                {
                    lastArgWasUnknown = false;
                    bool parseOK = true;

                    // We've seen the arg and set lastArg, so parse a value
                    if (parseContext.FlagsSetOnCommandLine.Contains(lastArg) && (!IsArray(lastArg.PropertyType)))
                    {
                        parseContext.AddCommandLineError(_fields[lastArg].Name, "specified multiple times");
                        parseOK = false;
                        status = false;
                    }

                    object value;
                    string parse_error;
                    int endIdx;
                    if (!this.ParseArg(lastArg.PropertyType, args, idx, out endIdx, out parse_error, out value))
                    {
                        parseContext.AddCommandLineError(_fields[lastArg].Name, parse_error);
                        parseOK = false;
                        status = false;
                    }

                    if (parseOK)
                    {
                        if (IsArray(lastArg.PropertyType))
                        {
                            // parse array
                            List<object> vals;

                            // add it to the list of arrays
                            if (parseContext.MultipleUseArgumentValues.TryGetValue(lastArg, out vals))
                            { // update the list of arrays
                                parseContext.MultipleUseArgumentValues[lastArg].Add(value);
                            }
                            else
                            {
                                vals = new List<object>();
                                vals.Add(value);
                                parseContext.MultipleUseArgumentValues.Add(lastArg, vals);
                                parseContext.FlagsSetOnCommandLine.Add(lastArg);
                            }
                        }
                        else
                        {
                            try
                            {
                                lastArg.SetValue(this, value, null);

                                // Remove any errors about this field from the config XML
                                parseContext.RemoveConfigurationError(_fields[lastArg].Name);
                                parseContext.FlagsSetOnCommandLine.Add(lastArg);
                            }
                            catch (TargetInvocationException ex)
                            {
                                if (ex.InnerException is ArgumentException)
                                {
                                    parseContext.AddCommandLineError(_fields[lastArg].Name, ex.InnerException.Message);
                                    status = false;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    idx = endIdx;
                    lastArg = null;
                }
            }

            List<PropertyInfo> missing = _required.FindAll(x => !parseContext.FlagsSetOnCommandLine.Contains(x) && !parseContext.FlagsSetInConfigurationFile.Contains(x));
            if (missing.Count > 0)
            {
                foreach (PropertyInfo f in missing)
                {
                    string n = f.Name;

                    // get the name of the arg, if it differs from the property name
                    foreach (KeyValuePair<string, PropertyInfo> v in _names)
                    {
                        if (v.Value.Name == f.Name)
                        {
                            n = v.Key;
                        }
                    }

                    parseContext.AddCommandLineError(n, "missing required argument");
                }

                status = false;
            }

            return status;
        }

        private bool ParseResponseFile(ConfigurationParseContext parseContext, string responseFileName)
        {
            string commandText;
            try
            {
                commandText = File.ReadAllText(responseFileName);
            }
            catch (IOException ex)
            {
                parseContext.AddCommandLineError(string.Empty, "While loading command line response file " + responseFileName + ": " + ex.Message);
                return false;
            }

            return this.ParseArgsImpl(null, ArgumentSplitter.CommandLineToArgvW(commandText), false, parseContext);
        }

        // TODO: This needs to be fixed.

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable
        /// interface, you should return null (Nothing in Visual Basic) from this method, and instead, if
        /// specifying a custom schema is required, apply the
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the
        /// object that is produced by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        /// method and consumed by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        /// method.
        /// </returns>
        /// <seealso cref="System.Xml.Serialization.IXmlSerializable.GetSchema()"/>
        public XmlSchema GetSchema()
        {
            XmlSchema schema = new XmlSchema();
            XmlSchemaComplexType rootType = new XmlSchemaComplexType();
            XmlSchemaAttribute verAttr = new XmlSchemaAttribute();
            verAttr.SchemaTypeName = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName; // work around idiotic .NET bug
            verAttr.Use = XmlSchemaUse.Required;
            verAttr.Name = "version";
            rootType.Attributes.Add(verAttr);
            XmlSchemaSequence rootContents = new XmlSchemaSequence();

            foreach (PropertyInfo f in _fields.Keys)
            {
                // if it's not CLI only
                if ((_fields[f].Type & FieldTypes.CliOnly) == FieldTypes.None)
                {
                    string name = _fields[f].Name;
                    XmlSchemaElement e = new XmlSchemaElement();
                    e.Name = name;
                    if (IsArray(f.PropertyType))
                    {
                        e.MaxOccurs = decimal.MaxValue;
                    }
                    else
                    {
                        e.MaxOccurs = 1;
                    }

                    if (_required.Contains(f))
                    {
                        e.MinOccurs = 1;
                    }
                    else
                    {
                        e.MinOccurs = 0;
                    }

                    // Work around idiotic .NET bug
                    XmlSchemaType t = this.GetSchemaType(f.PropertyType);
                    Type ft = f.PropertyType;
                    if (IsArray(ft))
                    {
                        ft = ft.GetElementType();
                    }

                    if (IsStruct(ft))
                    {
                        e.SchemaType = t;
                    }
                    else
                    {
                        e.SchemaTypeName = t.QualifiedName;
                    }

                    rootContents.Items.Add(e);
                }
            }

            rootType.Particle = rootContents;
            XmlSchemaElement rootElement = new XmlSchemaElement();
            rootElement.Name = this.GetClassName();
            rootElement.SchemaType = rootType;
            schema.Items.Add(rootElement);
            return schema;
        }

        /// <summary>Reads a configuration from file.</summary>
        /// <param name="filePath">Full pathname of the file.</param>
        public void ReadConfigurationFromFile(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                this.ReadXml(reader);
            }
        }

        /// <summary>Parse an XML configuration file matching this type.</summary>
        /// <exception cref="XmlException">Thrown when invalid configuration data exists in the source
        /// XML.</exception>
        /// <param name="reader">An <see cref="XmlReader"/> which does the actual XML reading.</param>
        /// <seealso cref="System.Xml.Serialization.IXmlSerializable.ReadXml(XmlReader)"/>
        public void ReadXml(XmlReader reader)
        {
            string error;
            if (!this.ReadXml(reader, out error))
            {
                throw new XmlException(error);
            }
        }

        /// <summary>Parse an XML configuration file matching this type.</summary>
        /// <param name="reader">An <see cref="XmlReader"/> which does the actual XML reading.</param>
        /// <param name="errors">[out] Returns a dictionary into which parse errors will be placed, where
        /// the key is the name of the argument the user provided, and the value is an error message
        /// describing that argument.</param>
        /// <returns>True if no errors were encountered, false otherwise.</returns>
        public bool ReadXml(XmlReader reader, out Dictionary<string, string> errors)
        {
            ConfigurationParseContext parseContext = new ConfigurationParseContext();
            if (this.ParseXmlConfigFileRaw(reader, parseContext))
            {
                // Check for all required
                List<PropertyInfo> missing = _required.FindAll(x => !parseContext.FlagsSetInConfigurationFile.Contains(x) && ((_fields[x].Type & FieldTypes.CliOnly) == FieldTypes.None)); // it's only missing if it's required *and* it's not CLI only
                foreach (PropertyInfo f in missing)
                {
                    parseContext.AddConfigurationError(f.Name, "missing required element");
                }
            }

            errors = parseContext.GenerateErrorDictionary();
            return errors.Count == 0;
        }

        /// <summary>Parse an XML configuration file matching this type.</summary>
        /// <param name="reader">An <see cref="XmlReader"/> which does the actual XML reading.</param>
        /// <param name="error">[out] A human readable list of errors in the arguments provided.</param>
        /// <returns>True if no errors were encountered, false otherwise.</returns>
        public bool ReadXml(XmlReader reader, out string error)
        {
            error = null;
            Dictionary<string, string> errors;
            bool status = this.ReadXml(reader, out errors);
            if (status)
            {
                return true;
            }

            error = GenerateErrorString(errors);
            return false;
        }

        private static string GenerateErrorString(Dictionary<string, string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return null;
            }

            return String.Join(Environment.NewLine, errors.Select(x =>
                    x.Key.Length > 0 ? x.Key + ": " + x.Value : x.Value)) + Environment.NewLine;
        }

        /// <summary>
        /// Validate the parsed configuration and set an error message if it is invalid.
        /// </summary>
        /// <param name="errors">A dictionary into which parse errors will be placed, where
        /// the key is the name of the argument the user provided, and the value is an error message
        /// describing that argument.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool Validate(Dictionary<string, string> errors)
        {
            return true;
        }

        /// <summary>
        /// Valid field types: Arrays of structs, primitives, or nullables of primitives; structs of
        /// primitives or nullables of primitives; nullables of primitives; primatives.
        /// </summary>
        /// <param name="t">The type to check for validity.</param>
        /// <returns>true if valid type, false if not.</returns>
        private static bool IsValidType(Type t)
        {
            if (IsArray(t))
            {
                return IsValidType(t.GetElementType()) && !IsArray(t.GetElementType()); // arrays can be anything valid that's not a nested array
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsValidType(GetTargetOfNullable(t)); // I think any valid type here is fine--arrays can't be put here anyway, nor can strings, but so what?
            }
            else if (IsEnum(t))
            {
                return Array.FindAll<FieldInfo>(t.GetFields(), x => x.IsPublic && x.IsStatic).Length > 0;
            }
            else if (IsStruct(t))
            {
                foreach (FieldInfo f in GetApplicableStructureFields(t))
                {
                    if (!IsValidType(f.FieldType) || IsArray(f.FieldType) || IsStruct(f.FieldType) || IsNullable(f.FieldType))
                    {
                        return false;
                    }
                }

                return GetApplicableStructureFields(t).Length > 0;
            }
            else
            {
                return Array.Exists<Type>(s_validTypes, x => x == t);
            }
        }

        /// <summary>Turns an argument given its type, name and value into its corresponding representation on the command line.</summary>
        /// <param name="type">The type being written to the CLI string.</param>
        /// <param name="name">The name of the type being written.</param>
        /// <param name="value">The value contained in the switch to write.</param>
        /// <returns>A command line string representing the given type, with the given switch name and value contents.</returns>
        private static string ArgToString(Type type, string name, object value)
        {
            if (IsArray(type))
            {
                string[] eles = new string[((Array)value).Length];
                int i = 0;
                foreach (var e in (Array)value)
                {
                    eles[i] = ArgToString(type.GetElementType(), name, e);
                    i++;
                }

                return string.Join(" ", eles);
            }
            else if (IsNullable(type))
            {
                if (value != null)
                {
                    return ArgToString(GetTargetOfNullable(type), name, value);
                }

                return null;
            }
            else if (IsEnum(type))
            {
                return string.Format(CultureInfo.InvariantCulture, "/{0} {1}", name, value);
            }
            else if (IsStruct(type))
            {
                string argument = string.Join(
                    " ",
                    Array.ConvertAll(
                    GetApplicableStructureFields(type),
                    x =>
                    {
                        if (x.FieldType == typeof(string))
                        {
                            string v = x.GetValue(value).ToString();
                            if (v.EndsWith("\\", StringComparison.Ordinal))
                            {
                                v += "\\";  // add an escape backslash to escape the trailing quote
                            }

                            return "\"" + v + "\"";
                        }
                        else
                        {
                            return x.GetValue(value).ToString();
                        }
                    }));
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "/{0} {1}",
                    name,
                    argument);
            }
            else if (type == typeof(bool))
            {
                return string.Format(CultureInfo.InvariantCulture, "/{0}{1}", name, (bool)value ? string.Empty : "-");
            }
            else if (type == typeof(string))
            {
                return string.Format(CultureInfo.InvariantCulture, "/{0} \"{1}\"", name, value.ToString().EndsWith("\\", StringComparison.Ordinal) ? value.ToString() + "\\" : value.ToString()); // add an escape backslash to escape the trailing quote
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "/{0} {1}", name, value);
            }
        }

        /// <summary>Queries if a type is an array.</summary>
        /// <param name="t">The type to query.</param>
        /// <returns>true if <paramref name="t"/> is an array, false if not, or if <paramref name="t"/> is not.</returns>
        private static bool IsArray(Type t)
        {
            if (t == null)
            {
                return false;
            }

            return t.IsArray;
        }

        /// <summary>Queries if a type is nullable.</summary>
        /// <param name="t">The type to check.</param>
        /// <returns>true if <paramref name="t"/> is nullable, false if not, or if <paramref name="t"/> is not.</returns>
        private static bool IsNullable(Type t)
        {
            if (t == null)
            {
                return false;
            }

            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>Queries if a type is an enum.</summary>
        /// <param name="t">The type to query.</param>
        /// <returns>true if <paramref name="t"/> is an enum type, false if not, or if <paramref name="t"/> is not.</returns>
        private static bool IsEnum(Type t)
        {
            if (t == null)
            {
                return false;
            }

            return t.IsEnum;
        }

        /// <summary>Gets a target of nullable type.</summary>
        /// <param name="t">The type from where the target is read.</param>
        /// <returns>The target of nullable type <paramref name="t"/>, or null if <paramref name="t"/> is not a nullable type.</returns>
        private static Type GetTargetOfNullable(Type t)
        {
            if (IsNullable(t))
            {
                return t.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>Queries if a type is a structure.</summary>
        /// <param name="t">The type to query.</param>
        /// <returns>true if <paramref name="t"/> is a structure, false if not, or if <paramref name="t"/> is not.</returns>
        private static bool IsStruct(Type t)
        {
            if (t == null)
            {
                return false;
            }

            Type nullableTarget = GetTargetOfNullable(t);
            if (nullableTarget != null)
            {
                return !nullableTarget.IsPrimitive && nullableTarget.IsValueType && !IsEnum(nullableTarget);
            }

            return !t.IsPrimitive && t.IsValueType && !IsArray(t) && !IsEnum(t); // if it's not primitive or nullable and a value type, it must be a struct
        }

        /// <summary>Gets the name of the type given after normalization.</summary>
        /// <param name="t">The type whose name shall be retrieved.</param>
        /// <returns>The type name, after normalization.</returns>
        private static string GetTypeName(Type t)
        {
            Type normalizedT = GetNormalizedType(t);
            if (IsStruct(normalizedT))
            {
                var names = new List<string>();
                foreach (FieldInfo f in GetApplicableStructureFields(normalizedT))
                {
                    names.Add(GetTypeName(f.FieldType));
                }

                return string.Join(" ", names.ToArray());
            }
            else if (IsEnum(normalizedT))
            {
                return "<enum>";
            }
            else
            {
                int idx = Array.FindIndex<Type>(s_validTypes, tp => tp.Equals(normalizedT));
                if (idx > -1)
                {
                    return "<" + s_typeNames[idx] + ">";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a normalized type. Normalizing turns nullable types into their contained type, and
        /// arrays into their containing type.
        /// </summary>
        /// <param name="t">The type to normalize.</param>
        /// <returns>The normalized type.</returns>
        private static Type GetNormalizedType(Type t)
        {
            Type nullableTarget = GetTargetOfNullable(t);
            if (nullableTarget != null)
            {
                return nullableTarget;
            }
            else if (IsArray(t))
            {
                return t.GetElementType();
            }
            else
            {
                return t;
            }
        }

        /// <summary>Tries to parse xml.</summary>
        /// <param name="t">The type to parse.</param>
        /// <param name="element">Element to parse from.</param>
        /// <param name="value">The value generated, if any.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private static bool TryParseXml(Type t, XmlElement element, out object value)
        {
            string innerText = String.Concat(element.ChildNodes.OfType<XmlText>().Select(x => x.Value));
            if (innerText.Length == 0)
            {
                // if there was no text, it's null? or is it empty string?
                if (GetNormalizedType(t) == typeof(string))
                {
                    value = string.Empty;
                }
                else
                {
                    // if it's not a string, just set it to null--though technically this isn't valid, we'll be tolerant
                    value = null;
                }

                return true;
            }
            else
            {
                return TryParse(t, innerText.ToString(), out value);
            }
        }

        /// <summary>Tries to parse the given type from the given string.</summary>
        /// <param name="t">The type to parse.</param>
        /// <param name="source">The data being parsed.</param>
        /// <param name="value">[out] The value parsed, if any.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private static bool TryParse(Type t, string source, out object value)
        {
            Type normalizedT = GetNormalizedType(t);
            if (normalizedT == typeof(int))
            {
                int r;
                bool s = int.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(uint))
            {
                uint r;
                bool s = uint.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(decimal))
            {
                decimal r;
                bool s = decimal.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(double))
            {
                double r;
                bool s = double.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(bool))
            {
                bool r;
                bool s = bool.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(long))
            {
                long r;
                bool s = long.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(ulong))
            {
                ulong r;
                bool s = ulong.TryParse(source, out r);
                value = r;
                return s;
            }

            if (normalizedT == typeof(string))
            {
                value = source;
                return true;
            }

            if (normalizedT == typeof(Version))
            {
                Version parsedVersion;
                if (Version.TryParse(source, out parsedVersion))
                {
                    // Force all version fields to be defined.
                    // (This way users don't see differing behavior if they enter 1.0 vs. entering 1.0.0)
                    value = new Version(
                        parsedVersion.Major,
                        parsedVersion.Minor,
                        parsedVersion.Build == -1 ? 0 : parsedVersion.Build,
                        parsedVersion.Revision == -1 ? 0 : parsedVersion.Revision
                        );
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            if (IsEnum(t))
            {
                try
                {
                    value = Enum.Parse(t, source, true); // ignore case
                    return true;
                }
                catch (ArgumentException)
                {
                    value = null;
                    return false;
                }
            }
            else if (IsEnum(GetTargetOfNullable(t)))
            {
                try
                {
                    value = Enum.Parse(GetTargetOfNullable(t), source, true); // ignore case
                    return true;
                }
                catch (ArgumentException)
                {
                    value = null;
                    return false;
                }
            }

            value = null;
            return false;
        }

        /// <summary>Formats the usage text.</summary>
        /// <param name="leftSideWidth">Width of the left side of the help text; where the arguments themselves are written. (as opposed to the right side, where help text is written)</param>
        /// <param name="windowWidth">Width of the window for which usage shall be generated.</param>
        /// <param name="arg">The argument name to write.</param>
        /// <param name="usage">The usage text to write.</param>
        /// <returns>The formatted usage text.</returns>
        private static string FormatUsage(int leftSideWidth, int windowWidth, string arg, string usage)
        {
            int indent = 3;
            windowWidth -= 1;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                result.Append(' ');
            }

            result.Append(arg);
            for (int i = result.Length; i < leftSideWidth; i++)
            {
                result.Append(' ');
            }

            // Now format usage
            int width = windowWidth - leftSideWidth;
            while (usage.Length > 0)
            {
                if (usage.Length <= width)
                {
                    result.Append(usage);
                    usage = string.Empty;
                    break;
                }
                else
                {
                    // find a split point
                    int split = width;
                    while (split >= 0 && usage[split] != ' ')
                    {
                        split--;
                    }

                    // we found one
                    if (split > -1)
                    {
                        string l = usage.Substring(0, split);

                        // don't want to do an empty line
                        if (!string.IsNullOrEmpty(l.Trim()))
                        {
                            result.AppendLine(l);
                        }

                        usage = usage.Substring(split).Trim();
                        if (usage.Length == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // we're going to have to cut the line
                        // unfortunately, I have to do one off so it doesn't insert a newline AND line wrap (i.e., insert a blank line)
                        string l = usage.Substring(0, width - 2);
                        result.AppendLine(l + "-");
                        usage = usage.Substring(width - 2).Trim();
                        if (usage.Length == 0)
                        {
                            break;
                        }
                    }
                }

                for (int i = 0; i < leftSideWidth; i++)
                {
                    result.Append(' ');
                }
            }

            return result.ToString();
        }

        /// <summary>Gets the fields which are applicable on a structure.</summary>
        /// <param name="t">The type where fields should be read.</param>
        /// <returns>The applicable structure fields.</returns>
        private static FieldInfo[] GetApplicableStructureFields(Type t)
        {
            if (!IsStruct(t))
            {
                throw new ArgumentException("The type to GetApplicableStructureFields must be a structure.");
            }

            return t.GetFields(BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>Queries a property for the existence of the no minus attribute.</summary>
        /// <param name="prop">The property to check for the NoMinusAttribute.</param>
        /// <returns>true if the supplied property has the no minus attribute, false if not.</returns>
        private static bool HasNoMinusAttribute(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(typeof(NoMinusAttribute), true).Length != 0;
        }

        /// <summary>Tries to find an argument by name.</summary>
        /// <param name="name">The name of the argument to find.</param>
        /// <param name="arg">Where the found argument is stored, in the event that it is found.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private bool TryFindArgumentByName(string name, out PropertyInfo arg)
        {
            return _names.TryGetValue(name, out arg) || _shortNames.TryGetValue(name, out arg);
        }

        /// <summary>Gets the schema type.</summary>
        /// <param name="t">The type for which a schema type shall be generated.</param>
        /// <returns>The schema type generated.</returns>
        private XmlSchemaType GetSchemaType(Type t)
        {
            if (IsArray(t))
            {
                t = t.GetElementType();
            }

            if (IsStruct(t))
            {
                XmlSchemaComplexType ct = new XmlSchemaComplexType();
                XmlSchemaSequence ctContents = new XmlSchemaSequence();
                foreach (FieldInfo f in GetApplicableStructureFields(t))
                {
                    XmlSchemaElement field = new XmlSchemaElement();
                    field.Name = f.Name;

                    // Work around idiotic .NET bug
                    XmlSchemaType st = this.GetSchemaType(f.FieldType);
                    if (IsStruct(f.FieldType))
                    {
                        field.SchemaType = st;
                    }
                    else
                    {
                        field.SchemaTypeName = st.QualifiedName;
                    }

                    field.MinOccurs = 1;
                    field.MaxOccurs = 1;
                    ctContents.Items.Add(field);
                }

                ctContents.MinOccurs = 1;
                ctContents.MaxOccurs = 1;
                ct.Particle = ctContents;
                return ct;
            }
            else
            {
                t = GetNormalizedType(t);
                if (t == typeof(int))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int);
                }
                else if (t == typeof(uint))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedInt);
                }
                else if (t == typeof(bool))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean);
                }
                else if (t == typeof(double))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Double);
                }
                else if (t == typeof(decimal))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal);
                }
                else if (t == typeof(long))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Long);
                }
                else if (t == typeof(ulong))
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedLong);
                }
                else
                {
                    return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
                }
            }
        }

        /// <summary>Attempts to read an XML Element as the given type.</summary>
        /// <param name="t">The type to read.</param>
        /// <param name="element">The element to process.</param>
        /// <param name="value">The value generated.</param>
        /// <param name="error">The error string, if any.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private bool ReadType(Type t, XmlElement element, out object value, out string error)
        {
            StringBuilder errors = new StringBuilder();
            bool status = true;
            if (IsStruct(t))
            {
                value = Activator.CreateInstance(t);
                List<FieldInfo> setFields = new List<FieldInfo>();
                XmlElement child;
                foreach (XmlNode node in element.ChildNodes)
                {
                    child = node as XmlElement;
                    if (child != null)
                    {
                        FieldInfo field = Array.Find<FieldInfo>(GetApplicableStructureFields(t), x => x.Name == child.Name);
                        if (field == null)
                        {
                            errors.AppendLine("unrecognized element " + child.Name);
                            value = null;
                            status = false;
                        }
                        else if (setFields.Contains(field))
                        {
                            errors.AppendLine(field.Name + " specified multiple times");
                            value = null;
                            status = false;
                        }

                        object e;
                        string er;
                        if (!this.ReadType(field.FieldType, child, out e, out er))
                        {
                            value = null;
                            errors.AppendLine(er);
                            status = false;
                        }

                        if (status)
                        {
                            field.SetValue(value, e);
                            setFields.Add(field);
                        }
                    }
                }

                if (status)
                {
                    error = null;
                }
                else
                {
                    error = errors.ToString();
                }

                return status;
            }
            else
            {
                // read the element type
                if (IsArray(t))
                {
                    return this.ReadType(t.GetElementType(), element, out value, out error);
                }
                else if (!TryParseXml(t, element, out value))
                {
                    error = "Could not parse type " + t.Name + " at element " + element.Name;
                    value = null;
                    return false;
                }

                error = null;
                return true;
            }
        }

        /// <summary>Writes a value as an XML Element, given its type.</summary>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="t">The type to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is
        /// serialized.</param>
        private void WriteType(Type t, object value, XmlWriter writer)
        {
            if (IsStruct(t) || IsStruct(GetTargetOfNullable(t)))
            {
                foreach (FieldInfo f in GetApplicableStructureFields(t))
                {
                    writer.WriteStartElement(f.Name);
                    this.WriteType(f.FieldType, f.GetValue(value), writer);
                    writer.WriteEndElement();
                }
            }
            else if (IsArray(t))
            {
                throw new ArgumentException("Cannot write arrays from WriteType");
            }
            else if (IsEnum(t) || IsEnum(GetTargetOfNullable(t)) || typeof(Version) == t)
            {
                writer.WriteValue(value.ToString());
            }
            else if (t != typeof(string) || value != null)
            {
                writer.WriteValue(value);
            }
        }

        /// <summary>Parses an argument.</summary>
        /// <param name="argType">Type of the argument to parse.</param>
        /// <param name="args">Argument array to parse.</param>
        /// <param name="startIdx">The start index in the array to parse.</param>
        /// <param name="endIdx">[out] The end index in the array being parsed.</param>
        /// <param name="error">Generated error string, if any.</param>
        /// <param name="value">The value generated, if any.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private bool ParseArg(Type argType, IList<string> args, int startIdx, out int endIdx, out string error, out object value)
        {
            if (IsStruct(argType))
            {
                if (IsNullable(argType))
                {
                    argType = argType.GetGenericArguments()[0];
                }

                FieldInfo[] structureFields = GetApplicableStructureFields(argType);
                if (structureFields.Length + startIdx > args.Count)
                {
                    error = "Too few values specified";
                    endIdx = startIdx;
                    value = null;
                    return false;
                }

                value = Activator.CreateInstance(argType);
                for (int i = 0; i < structureFields.Length; i++)
                {
                    object v;
                    if (!TryParse(structureFields[i].FieldType, args[i + startIdx], out v))
                    {
                        string t;
                        Type nT = GetNormalizedType(structureFields[i].FieldType);
                        if (Array.Exists(s_validTypes, x => x.Equals(nT)))
                        {
                            t = s_typeNames[Array.IndexOf(s_validTypes, nT)];
                        }
                        else
                        {
                            t = nT.Name;
                        }

                        error = "Unable to parse value " + args[i + startIdx] + " as type " + t;
                        endIdx = startIdx + i;
                        return false;
                    }

                    structureFields[i].SetValue(value, v);
                }

                error = null;
                endIdx = startIdx + structureFields.Length;
                return true;
            }
            else if (GetNormalizedType(argType) == typeof(bool))
            {
                if (args[startIdx].EndsWith("-", StringComparison.Ordinal))
                {
                    value = false;
                }
                else
                {
                    value = true;
                }

                error = null;
                endIdx = startIdx + 1;
                return true;
            }
            else if (IsArray(argType))
            {
                return this.ParseArg(argType.GetElementType(), args, startIdx, out endIdx, out error, out value);
            }
            else
            {
                if (args.Count <= startIdx)
                {
                    error = "Too few values specified";
                    endIdx = startIdx;
                    value = null;
                    return false;
                }

                if (!TryParse(argType, args[startIdx], out value))
                {
                    value = null;
                    string t;
                    Type nT = GetNormalizedType(argType);
                    if (Array.Exists(s_validTypes, x => x.Equals(nT)))
                    {
                        t = s_typeNames[Array.IndexOf(s_validTypes, nT)];
                    }
                    else
                    {
                        t = nT.Name;
                    }

                    error = "Unable to parse value " + args[startIdx] + " as type " + t;
                    endIdx = startIdx + 1;
                    return false;
                }

                error = null;
                endIdx = startIdx + 1;
                return true;
            }
        }

        /// <summary>Adds a field to the usage text.</summary>
        /// <param name="field">The field to add.</param>
        /// <param name="fieldAttr">The field attribute attached to <paramref name="field"/>.</param>
        private void AddToUsage(PropertyInfo field, FieldAttribute fieldAttr)
        {
            if ((fieldAttr.Type & FieldTypes.Hidden) != FieldTypes.None)
            {
                return;
            }

            if (GetNormalizedType(field.PropertyType) == typeof(bool))
            {
                if (fieldAttr.ShortName != null)
                {
                    _usage.Add(string.Format(CultureInfo.InvariantCulture, "/{0} (short: /{1})", fieldAttr.Name, fieldAttr.ShortName));
                }
                else
                {
                    _usage.Add(string.Format(CultureInfo.InvariantCulture, "/{0}", fieldAttr.Name));
                }
            }
            else
            {
                string typeName = GetTypeName(field.PropertyType);
                if (fieldAttr.ShortName != null)
                {
                    _usage.Add(string.Format(CultureInfo.InvariantCulture, "/{0} {1} (short: /{2})", fieldAttr.Name, typeName, fieldAttr.ShortName));
                }
                else
                {
                    _usage.Add(string.Format(CultureInfo.InvariantCulture, "/{0} {1}", fieldAttr.Name, typeName));
                }
            }

            string options = String.Empty; // notes about usage
            if ((fieldAttr.Type & FieldTypes.Required) != FieldTypes.None)
            {
                if (IsArray(field.PropertyType))
                {
                    options = "required, multiple";
                }
                else
                {
                    options = "required";
                }
            }
            else if (IsArray(field.PropertyType))
            {
                options = "multiple";
            }

            if (IsEnum(GetNormalizedType(field.PropertyType)))
            {
                StringBuilder sb = new StringBuilder("values: ");
                foreach (FieldInfo f in Array.FindAll<FieldInfo>(GetNormalizedType(field.PropertyType).GetFields(), x => x.IsPublic && x.IsStatic))
                {
                    sb.Append("\"" + f.Name + "\"");
                    sb.Append(", ");
                }

                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 2, 2);
                }

                if (String.IsNullOrEmpty(options))
                {
                    options = sb.ToString();
                }
                else
                {
                    options += ", " + sb.ToString();
                }
            }

            if (!String.IsNullOrEmpty(options))
            {
                options = "(" + options + ")";
            }

            if (!String.IsNullOrWhiteSpace(fieldAttr.Usage))
            {
                if (options.Length != 0)
                {
                    options = fieldAttr.Usage + " " + options;
                }
                else
                {
                    options = fieldAttr.Usage;
                }
            }

            _usage.Add(options);
        }

        private bool ParseXmlConfigFileSafe(ConfigurationParseContext parseContext, string configFileName)
        {
            try
            {
                using (XmlTextReader r = new XmlTextReader(configFileName) { DtdProcessing = DtdProcessing.Ignore })
                {
                    if (!this.ParseXmlConfigFileRaw(r, parseContext))
                    {
                        return false;
                    }
                }
            }
            catch (IOException ex)
            {
                parseContext.AddCommandLineError(string.Empty, "While loading XML configuration file " + configFileName + ": " + ex.Message);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                parseContext.AddCommandLineError(string.Empty, "While loading XML configuration file " + configFileName + ": " + ex.Message);
                return false;
            }
            catch (UriFormatException ex)
            {
                parseContext.AddCommandLineError(string.Empty, "While loading XML configuration file " + configFileName + ": " + ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse an XML configuration file matching this type. Does NOT check for missing required
        /// arguments (that is done by ReadXml(XmlReader, out string)) or call Validate() method. It
        /// *only* throws setter errors.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="TargetInvocationException">Thrown when a target invocation error condition
        /// occurs.</exception>
        /// <param name="reader">An <see cref="XmlReader"/> which does the actual XML reading.</param>
        /// <param name="parseContext">Context for the configuration parse currently running.</param>
        /// <returns>True if no errors were encountered, false otherwise.</returns>
        private bool ParseXmlConfigFileRaw(XmlReader reader, ConfigurationParseContext parseContext)
        {
            List<PropertyInfo> mySetArgs = new List<PropertyInfo>();
            bool status = true;
            StringBuilder general_errors = new StringBuilder();
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(reader);
            }
            catch (XmlException ex)
            {
                parseContext.AddConfigurationError(string.Empty, ex.Message);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                parseContext.AddConfigurationError(string.Empty, ex.Message);
                return false;
            }

            XmlElement root = null;
            foreach (XmlNode n in document.ChildNodes)
            {
                XmlElement asElement = n as XmlElement;
                if (asElement != null)
                {
                    if (n.Name == this.GetClassName())
                    {
                        root = asElement;
                    }
                    else
                    {
                        general_errors.AppendLine("unknown element " + n.Name);
                        status = false;
                    }
                }
            }

            if (root == null)
            {
                general_errors.AppendLine("missing root element " + this.GetClassName());
                status = false;
            }
            else
            {
                // TODO: should we check version here?
                XmlElement el;
                foreach (XmlNode node in root.ChildNodes)
                {
                    el = node as XmlElement;
                    if (el != null)
                    {
                        string name = el.Name;
                        PropertyInfo field;
                        if (_names.TryGetValue(name, out field))
                        {
                        }
                        else if (_shortNames.TryGetValue(name, out field))
                        {
                        }
                        else
                        {
                            parseContext.AddConfigurationError(name, "unrecognized element");
                            status = false;
                            continue;
                        }

                        if ((_fields[field].Type & FieldTypes.CliOnly) != FieldTypes.None)
                        {
                            parseContext.AddConfigurationError(name, "parameter can only be specified at the command line");
                            status = false;
                        }

                        if (mySetArgs.Contains(field) && !IsArray(field.PropertyType))
                        {
                            parseContext.AddConfigurationError(name, "specified multiple times");
                            status = false;
                        }

                        object value;
                        string e;
                        if (!this.ReadType(field.PropertyType, el, out value, out e))
                        {
                            parseContext.AddConfigurationError(field.Name, e);
                            status = false;
                        }

                        // don't set it if it was set by the CLI, for non-arrays
                        if (IsArray(field.PropertyType))
                        {
                            List<object> l;
                            if (parseContext.MultipleUseArgumentValues.TryGetValue(field, out l))
                            {
                                l.Add(value);
                            }
                            else
                            {
                                parseContext.MultipleUseArgumentValues.Add(field, new List<object>(new object[] { value }));
                            }
                        }
                        else
                        {
                            if (!parseContext.FlagsSetOnCommandLine.Contains(field))
                            {
                                try
                                {
                                    field.SetValue(this, value, null); // is this cast OK?
                                }
                                catch (TargetInvocationException ex)
                                {
                                    if (ex.InnerException is ArgumentException)
                                    {
                                        parseContext.AddConfigurationError(field.Name, ex.InnerException.Message);
                                        status = false;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }

                            mySetArgs.Add(field);
                        }
                    }
                }
            }

            // now set any arrays
            foreach (PropertyInfo key in parseContext.MultipleUseArgumentValues.Keys)
            {
                Array v = Array.CreateInstance(key.PropertyType.GetElementType(), parseContext.MultipleUseArgumentValues[key].Count);
                int i = 0;
                foreach (object o in parseContext.MultipleUseArgumentValues[key])
                {
                    v.SetValue(o, i);
                    i++;
                }

                try
                {
                    key.SetValue(this, v, null);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is ArgumentException)
                    {
                        parseContext.AddConfigurationError(key.Name, ex.InnerException.Message);
                        status = false;
                    }
                    else
                    {
                        throw;
                    }
                }

                mySetArgs.Add(key);
            }

            foreach (PropertyInfo arg in mySetArgs)
            {
                parseContext.FlagsSetInConfigurationFile.Add(arg);
            }

            if (general_errors.Length > 0)
            {
                parseContext.AddConfigurationError(string.Empty, general_errors.ToString());
            }

            return status;
        }

        /// <summary>Gets the class name.</summary>
        /// <returns>The class name.</returns>
        private string GetClassName()
        {
            return this.GetType().Name;
        }
    }
}
