// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Readers.SampleModel
{
    // These classes are a sample object model with JSON representation for DeferredCollectionsTests.

    public enum Level
    {
        Error = 0,
        Warn = 1,
        Info = 2,
        Debug = 3,
        Detail = 4
    }

    public class LogMessage
    {
        public Level Level { get; set; }
        public DateTime WhenUtc { get; set; }
        public string Text { get; set; }
        public string CodeContextID { get; set; }

        public override int GetHashCode()
        {
            return this.Level.GetHashCode() ^ this.WhenUtc.GetHashCode() ^ this.Text.GetHashCode() ^ this.CodeContextID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LogMessage other = obj as LogMessage;
            if (other == null) return false;

            return this.Level == other.Level
                && this.WhenUtc == other.WhenUtc
                && this.Text == other.Text
                && this.CodeContextID == other.CodeContextID;
        }
    }

    public enum CodeContextType
    {
        Application,
        Binary,
        Namespace,
        Class,
        Method,
        Property,
        Field,
        Line
    }

    public class CodeContext
    {
        public string ParentContextID { get; set; }
        public string Name { get; set; }
        public CodeContextType Type { get; set; }

        public override int GetHashCode()
        {
            return this.ParentContextID.GetHashCode() ^ this.Name.GetHashCode() ^ this.Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            CodeContext other = obj as CodeContext;
            if (other == null) return false;

            return this.ParentContextID == other.ParentContextID
                && this.Name == other.Name
                && this.Type == other.Type;
        }
    }

    public class Log
    {
        public Guid ID { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public string ApplicationContext { get; set; }

        public IList<LogMessage> Messages { get; set; }

        public IDictionary<string, CodeContext> CodeContexts { get; set; }
    }

    /// <summary>
    ///  ContractResolver using DeferredList and DeferredDictionary for collections
    /// </summary>
    internal class LogModelDeferredContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof(IList<LogMessage>))
            {
                contract.Converter = new DeferredListConverter<LogMessage>();
            }
            else if (objectType == typeof(IDictionary<string, CodeContext>))
            {
                contract.Converter = new DeferredDictionaryConverter<CodeContext>();
            }

            return contract;
        }
    }

    internal class LogModelSampleBuilder
    {
        public const string SampleLogPath = @"CodeCrawler.log.json";
        public const string SampleOneLinePath = "CodeCrawler.OneLine.log.json";
        public const string SampleNoCodeContextsPath = "CodeCrawler.NoCodeContexts.log.json";
        public const string SampleEmptyPath = "CodeCrawler.Empty.log.json";

        private static readonly string[] MessageTexts = { "File Scan starting", "File Scan complete", "Rules \u00A9 reloaded", "File Scan \u16A0 timed out \U00010908" };
        private static readonly object _locker = new object();

        public static Log Build()
        {
            // This test construction doesn't make sense. If the test content should be non-deterministic,
            // we shouldn't provide a hard-coded seed. If the test content should be deterministic, we
            // shouldn't emit the current DateTime (which itself may have different lengths). The differing
            // length of emitted DateTime has provoked some bug in a boundary condition that has caused
            // non-deterministic test failures. This is good in the sense that we've captured signal on
            // some buried bug that we need to chase down. It's bad because the failure pops up 
            // unpredictably. A common way to resolve this situation is to emit sufficient details
            // during test execution to reliably recreate the failure (e.g., by logging the Random seed
            // and by consistently using the Random instance to produce the generated content).
            //
            // When someone fixes the broken test case, we should also make a call on how to handle 
            // the test content generation. For now, we will make this output deterministic by 
            // deriving the emitted Date from the Random instance with a hard-coded seed of 5.
            // https://github.com/Microsoft/sarif-sdk/issues/1126
            //
            // Using a hard-coded Random seed to produce generated content is inadvisable because
            // it would be preferable to simply check in the deterministic test content rather
            // than to take the costs to produce it each time. If we stick with the randomly 
            // generated content approach, the next line of code should replace the hard-coded
            // seed with one that derives from current time. We also need to emit the seed in
            // test output. This logging is straightforward for within-IDE testing as well as
            // console testing. We currently do not have sufficient logging in AppVeyor testing
            // to see this output, however. That gap needs to be closed.
            //

            var random = new Random(7);

            // Uncomment this code to provoke a failure for debugging
            //return Build(random, new DateTime(random.Next(1, 9999), random.Next(1, 12), random.Next(1, 30)), 7);

            // Generating 5 rows of content does not fail
            return Build(random, new DateTime(random.Next(1, 9999), random.Next(1, 12), random.Next(1, 30)), 5);
        }

        public static Log Build(Random r, DateTime whenUtc, int messageCount)
        {
            Log log = new Log();
            log.ID = Guid.NewGuid();
            log.StartTimeUtc = whenUtc;
            log.ApplicationContext = "CodeCrawler.exe";

            Dictionary<string, CodeContext> contexts = new Dictionary<string, CodeContext>();
            contexts["app"] = new CodeContext() { Name = "CodeCrawler.exe", Type = CodeContextType.Binary };
            contexts["scan"] = new CodeContext() { Name = "CodeCrawler.Scanners", Type = CodeContextType.Namespace, ParentContextID = "app" };
            contexts["file"] = new CodeContext() { Name = "FileScanner", Type = CodeContextType.Class, ParentContextID = "scan" };
            contexts["run"] = new CodeContext() { Name = "Run()", Type = CodeContextType.Method, ParentContextID = "file" };
            contexts["load"] = new CodeContext() { Name = "LoadRules()", Type = CodeContextType.Method, ParentContextID = "run" };
            log.CodeContexts = contexts;

            List<string> codeContextKeys = new List<string>(contexts.Keys);

            log.Messages = new List<LogMessage>();
            for (int i = 0; i < messageCount; ++i)
            {
                whenUtc = whenUtc.AddMilliseconds(r.Next(10));

                LogMessage m = new LogMessage()
                {
                    Level = (Level)r.Next(5),
                    WhenUtc = whenUtc,
                    Text = MessageTexts[r.Next(MessageTexts.Length)],
                    CodeContextID = codeContextKeys[r.Next(codeContextKeys.Count)]
                };

                log.Messages.Add(m);
            }

            return log;
        }

        public static void EnsureSamplesBuilt()
        {
            lock (_locker)
            {
                Log log = null;
                JsonSerializer serializer = new JsonSerializer();

                if (!File.Exists(SampleLogPath))
                {
                    if (log == null) log = Build();

                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(File.OpenWrite(SampleLogPath))))
                    {
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(writer, log);
                    }
                }

                if (!File.Exists(SampleOneLinePath))
                {
                    if (log == null) log = Build();

                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(File.OpenWrite(SampleOneLinePath))))
                    {
                        serializer.Formatting = Formatting.None;
                        serializer.Serialize(writer, log);
                    }
                }

                if (!File.Exists(SampleNoCodeContextsPath))
                {
                    if (log == null) log = Build();

                    log.CodeContexts.Clear();
                    foreach (LogMessage m in log.Messages)
                    {
                        m.CodeContextID = null;
                    }

                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(File.OpenWrite(SampleNoCodeContextsPath))))
                    {
                        serializer.Formatting = Formatting.None;
                        serializer.Serialize(writer, log);
                    }
                }

                if (!File.Exists(SampleEmptyPath))
                {
                    if (log == null) log = Build();

                    log.CodeContexts.Clear();
                    log.Messages.Clear();

                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(File.OpenWrite(SampleEmptyPath))))
                    {
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(writer, log);
                    }
                }
            }
        }
    }
}
