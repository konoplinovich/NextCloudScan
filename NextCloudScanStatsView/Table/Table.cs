using System;
using System.Collections.Generic;
using System.Text;

namespace NextCloudScanStatsView.Interface
{
    public class Table
    {
        private int _columnsCount;

        public List<Column> Columns { get; private set; }
        public List<string> Headers { get; private set; } = new List<string>();
        public static Borders Borders { get; private set; }
        public int Width { get; private set; }

        public Table(List<Column> columns)
        {
            Columns = columns;
            _columnsCount = columns.Count;

            foreach (Column column in Columns)
            {
                Headers.Add(column.Header);
                Width += column.Width;
            }

            Width += _columnsCount + 1;

            Borders = new Borders()
            {
                UpperLeft = "┌",
                UpperRight = "┐",
                UpperCenter = "┬",
                IntersectLeft = "├",
                IntersectRight = "┤",
                IntersectCenter = "┼",
                BottomLeft = "└",
                BottomRight = "┘",
                BottomCenter = "┴",
                VerticalLine = "│",
                HorizontalLine = "─",
                WhiteSpacePlaceholder = ' ',
                LongLinePlaceholder = "..."
            };
        }

        public void DrawHeader()
        {
            Console.WriteLine(DrawLine(Borders.UpperLeft, Borders.UpperRight, Borders.UpperCenter));
            Console.WriteLine(DrawRow(Headers));
            Console.WriteLine(DrawLine(Borders.IntersectLeft, Borders.IntersectRight, Borders.IntersectCenter));
        }

        public void AddRow(List<string> values)
        {
            Console.WriteLine(DrawRow(values));
        }

        public void AddRow(List<string> values, int lastColumnsSpan)
        {
            Console.WriteLine(DrawRowWithSpan(values, lastColumnsSpan));
        }

        public void StartRowsWithSpan(int lastColumnsSpan)
        {
            Console.WriteLine(DrawLineWithSpan(lastColumnsSpan, Borders.IntersectLeft, Borders.IntersectRight, Borders.IntersectCenter, Borders.BottomCenter));
        }

        public void EndRowsWithSpan(int lastColumnsSpan)
        {
            Console.WriteLine(DrawLineWithSpan(lastColumnsSpan, Borders.IntersectLeft, Borders.IntersectRight, Borders.IntersectCenter, Borders.UpperCenter));
        }

        public void LastRowWithSpan(int lastColumnsSpan)
        {
            Console.WriteLine(DrawLineWithSpan(lastColumnsSpan, Borders.BottomLeft, Borders.BottomRight, Borders.BottomCenter, Borders.HorizontalLine));
        }

        public void Close()
        {
            Console.WriteLine(DrawLine(Borders.BottomLeft, Borders.BottomRight, Borders.BottomCenter));
        }

        private string DrawLineWithSpan(int lastColumnsSpan, string left, string right, string center, string bottomCenter)
        {
            StringBuilder row = new StringBuilder();

            for (int column = 0; column < _columnsCount; column++)
            {
                int width = Columns[column].Width;
                string start = column >= 1 ? string.Empty : left;

                row.Append(start);

                for (int index = 0; index < width; index++)
                {
                    row.Append(Borders.HorizontalLine);
                }

                if (column < _columnsCount - lastColumnsSpan) row.Append(center);
                else if (column == _columnsCount - 1) row.Append(right);
                else row.Append(bottomCenter);
            }

            return row.ToString();
        }

        private string DrawRowWithSpan(List<string> values, int lastColumnsSpan)
        {
            StringBuilder row = new StringBuilder();

            for (int column = 0; column < _columnsCount - lastColumnsSpan; column++)
            {
                int width = Columns[column].Width;
                string start = column >= 1 ? string.Empty : Borders.VerticalLine;
                row.Append(start);

                string content = values[column];
                content = NormalizeCell(content, width, Columns[column].Alignment);

                row.Append(content);
                row.Append(Borders.VerticalLine);
            }

            row.Append(values[values.Count - 1]);
            return row.ToString().PadRight(Width - 1, Borders.WhiteSpacePlaceholder) + Borders.VerticalLine;
        }

        private string DrawRow(List<string> values)
        {
            StringBuilder row = new StringBuilder();

            for (int column = 0; column < _columnsCount; column++)
            {
                int width = Columns[column].Width;
                string start = column >= 1 ? string.Empty : Borders.VerticalLine;
                row.Append(start);

                string content = values[column];
                content = NormalizeCell(content, width, Columns[column].Alignment);

                row.Append(content);
                row.Append(Borders.VerticalLine);
            }

            return row.ToString(); ;
        }

        private string DrawLine(string left, string right, string center)
        {
            StringBuilder row = new StringBuilder();

            for (int column = 0; column < _columnsCount; column++)
            {
                int width = Columns[column].Width;
                string start = column >= 1 ? string.Empty : left;

                row.Append(start);

                for (int index = 0; index < width; index++)
                {
                    row.Append(Borders.HorizontalLine);
                }

                string end = column != _columnsCount - 1 ? center : right;
                row.Append(end);
            }

            return row.ToString();
        }

        private static string NormalizeCell(string content, int width, Alignment alignment)
        {
            if (content.Length < width)
            {
                switch (alignment)
                {
                    case Alignment.Left:
                        content = content.PadRight(width, Borders.WhiteSpacePlaceholder);
                        break;
                    case Alignment.Right:
                        content = content.PadLeft(width, Borders.WhiteSpacePlaceholder);
                        break;
                    default:
                        break;
                }
            }
            else if (content.Length > width && width > Borders.LongLinePlaceholder.Length)
            {
                content = content.Substring(0, width - Borders.LongLinePlaceholder.Length);
                content += Borders.LongLinePlaceholder;
            }
            else
            {
                switch (alignment)
                {
                    case Alignment.Left:
                        content.PadRight(width);
                        break;
                    case Alignment.Right:
                        content.PadLeft(width);
                        break;
                    default:
                        break;
                }
            }

            content = content.Length > width ? string.Empty.PadLeft(width) : content;

            return content;
        }
    }
}