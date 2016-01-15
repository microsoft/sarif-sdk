// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// Writes code/text with indent formatting to make prettier generate code.
    /// </summary>
    internal sealed class CodeWriter
    {
        /// <summary>The maximum indent level supported. Indents greater than this will throw
        /// <see cref="InvalidOperationException"/> (because they're almost certainly bugs).</summary>
        public const int MaximumIndent = 15;

        /// <summary>The default indent size, in characters.</summary>
        public const int DefaultIndentSize = 4;

        /// <summary>The default indent character.</summary>
        public const char DefaultIndentCharacter = ' ';

        private readonly StringBuilder _builder = new StringBuilder();
        private readonly int _indentSize;
        private readonly char _indentCharacter;
        private int _indentLevel;

        /// <summary>Initializes a new instance of the <see cref="CodeWriter"/> class.</summary>
        public CodeWriter() : this(DefaultIndentSize, DefaultIndentCharacter)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeWriter"/> class, using the specified
        /// indent size and character.</summary> 
        /// <param name="indentSize">The number of indentation characters added for each level of indentation.</param>
        /// <param name="indentCharacter">The character to use to indent.</param>
        public CodeWriter(int indentSize, char indentCharacter)
        {
            _indentSize = indentSize;
            _indentCharacter = indentCharacter;
        }

        /// <summary>Increases the indent level.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the current indentation level is
        /// already <see cref="MaximumIndent"/>.</exception>
        public void IncrementIndentLevel()
        {
            if (_indentLevel == MaximumIndent)
            {
                throw new InvalidOperationException(Strings.IndentedTooDeep);
            }

            _indentLevel++;
        }

        /// <summary>Decreases the indent level.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the indent level is already zero.</exception>
        public void DecrementIndentLevel()
        {
            int oldLevel = _indentLevel;
            if (oldLevel == 0)
            {
                throw new InvalidOperationException(Strings.IndentMismatch);
            }

            _indentLevel = oldLevel - 1;
        }

        /// <summary>Appends a newline to the output.</summary>
        public void WriteLine()
        {
            _builder.AppendLine();
        }

        /// <summary>Writes a line of text to the output, with indent and newline applied.</summary>
        /// <param name="value">The text to put on the line.</param>
        public void WriteLine(string value)
        {
            string newLine = Environment.NewLine;
            this.AppendIndent(value.Length + newLine.Length);
            _builder.Append(value);
            _builder.Append(newLine);
        }

        /// <summary>
        /// Writes a formatted line of text to the output, with indent and newline applied.
        /// </summary>
        /// <param name="formatString">The format string to use.</param>
        /// <param name="args">
        /// A variable-length parameters list containing arguments to format into
        /// <paramref name="formatString"/>.
        /// </param>
        public void WriteLine(string formatString, params string[] args)
        {
            this.WriteLine(String.Format(CultureInfo.InvariantCulture, formatString, args));
        }

        /// <summary>Consumes a <see cref="StringBuilder"/> as a line.</summary>
        /// <param name="sb">The string builder to write as a line.</param>
        public void WriteLineConsume(StringBuilder sb)
        {
            this.WriteLine(sb.ToString());
            sb.Clear();
        }

        /// <summary>Writes text and a leading indent block.</summary>
        /// <param name="value">The text to write.</param>
        public void Write(string value)
        {
            this.AppendIndent(value.Length);
            _builder.Append(value);
        }

        /// <summary>Appends a line without any indentation.</summary>
        public void WriteLineRaw()
        {
            _builder.AppendLine();
        }

        /// <summary>Appends a line without any indentation.</summary>
        /// <param name="value">The text to put on the line.</param>
        public void WriteLineRaw(string value)
        {
            _builder.AppendLine(value);
        }

        /// <summary>Appends characters without any indenting, newlines, or other processing.</summary>
        /// <param name="value">The text to write.</param>
        public void WriteRaw(string value)
        {
            _builder.Append(value);
        }

        /// <summary>Opens a block in the generated code.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the current indentation level is
        /// already <see cref="MaximumIndent"/>.</exception>
        public void OpenBrace()
        {
            this.WriteLine("{");
            this.IncrementIndentLevel();
        }

        /// <summary>Writes a declaration and opens an associated block in the code.</summary>
        /// <param name="declaration">The declaration to write.</param>
        /// <exception cref="InvalidOperationException">Thrown when the current indentation level is
        /// already <see cref="MaximumIndent"/>.</exception>
        public void OpenBrace(string declaration)
        {
            this.WriteLine(declaration);
            this.OpenBrace();
        }

        /// <summary>Writes a formatted declaration and opens an associated block in the code.</summary>
        /// <param name="declFormat">The format string for the declaration to write.</param>`
        /// <param name="args">
        /// Arguments formatted into <paramref name="declFormat"/> to generate the associated declaration.
        /// </param>
        public void OpenBrace(string declFormat, params string[] args)
        {
            this.WriteLine(declFormat, args);
            this.OpenBrace();
        }

        /// <summary>
        /// Consumes a declaration from a <see cref="StringBuilder"/> and opens an associated block in the code.
        /// </summary>
        /// <param name="declaration">The <see cref="StringBuilder"/> containing the declaration to use.</param>
        /// <exception cref="InvalidOperationException">Thrown when the current indentation level is
        /// already <see cref="MaximumIndent"/>.</exception>
        public void OpenBraceConsume(StringBuilder declaration)
        {
            this.WriteLineConsume(declaration);
            this.OpenBrace();
        }

        /// <summary>Writes close block to the generated code.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the indent level is already zero.</exception>
        public void CloseBrace(string suffixText = null)
        {
            this.DecrementIndentLevel();
            this.WriteLine("}" + (suffixText ?? ""));
        }

        /// <summary>Convert this object into a string representation.</summary>
        /// <returns>A string that represents this object.</returns>
        public override string ToString()
        {
            return _builder.ToString();
        }

        private void AppendIndent(int additionalCharacters)
        {
            int indentCharacters;
            checked
            {
                indentCharacters = _indentLevel * _indentSize;
                int charactersToAdd = indentCharacters + additionalCharacters;
                _builder.EnsureCapacity(_builder.Capacity + charactersToAdd);
            }

            _builder.Append(_indentCharacter, indentCharacters);
        }
    }
}