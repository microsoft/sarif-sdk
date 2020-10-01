using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BSOA.Demo
{
    public class ConsoleColumn
    {
        public string Heading { get; set; }
        public bool RightAligned { get; set; }
        public bool Highlighted { get; set; }
        public int Width { get; set; }

        public ConsoleColumn(string heading, bool rightAligned = false, bool highlighted = false)
        {
            Heading = heading;
            RightAligned = rightAligned;
            Highlighted = highlighted;
            Width = heading?.Length ?? 0;
        }
    }

    public class ConsoleTable
    {
        private IReadOnlyList<ConsoleColumn> Columns { get; set; }
        private List<string[]> Rows { get; set; }

        private ConsoleColor DefaultColor { get; set; }
        private ConsoleColor HighlightColor { get; set; }

        private Point Start { get; set; }

        public ConsoleTable(params ConsoleColumn[] columns)
        {
            DefaultColor = Console.ForegroundColor;
            HighlightColor = ConsoleColor.Green;

            Columns = columns;
            Rows = new List<string[]>();

            Start = new Point(Console.CursorLeft, Console.CursorTop);
        }

        public ConsoleTable(params string[] columnNames) :
            this(columnNames.Select((name) => new ConsoleColumn(name)).ToArray())
        { }

        public void AppendRow(params string[] values)
        {
            Rows.Add(values);

            bool redrawRequired = false;
            for (int i = 0; i < values.Length; ++i)
            {
                int valueLength = values[i]?.Length ?? 0;
                if (Columns[i].Width < valueLength)
                {
                    redrawRequired = true;
                    Columns[i].Width = valueLength;
                }
            }

            if (redrawRequired)
            {
                WriteTable();
            }
            else
            {
                if (Rows.Count == 1) { WriteHeader(); }
                WriteRow(values);
            }
        }

        private void WriteHeader()
        {
            // Write column headings
            WriteRow(Columns.Select((c) => c.Heading));

            // Write separator row
            WriteRow(Columns.Select((c) => new string('-', c.Width)));
        }

        private void WriteRow(IEnumerable<string> values)
        {
            Console.Write("| ");

            int i = 0;
            foreach (string value in values)
            {
                WriteCell(value, Columns[i]);
                i++;
            }

            Console.WriteLine();
        }

        private void WriteTable()
        {
            Console.CursorTop = Start.Y;
            Console.CursorLeft = Start.X;

            WriteHeader();

            foreach (string[] row in Rows)
            {
                WriteRow(row);
            }
        }

        private void WriteCell(string value, ConsoleColumn column)
        {
            if (value == null) { value = "<null>"; }

            Console.ForegroundColor = (column.Highlighted ? HighlightColor : DefaultColor);

            int padLength = Math.Max(0, column.Width - value.Length);
            if (padLength > 0 && column.RightAligned)
            {
                Console.Write(new string(' ', padLength));
            }

            Console.Write(value);

            if (padLength > 0 && !column.RightAligned)
            {
                Console.Write(new string(' ', padLength));
            }

            Console.ForegroundColor = DefaultColor;
            Console.Write(" | ");
        }
    }
}
