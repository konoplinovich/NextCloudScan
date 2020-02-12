using System;
using System.Collections.Generic;
using System.Text;

namespace NextCloudScanStatsView.Interface
{
    public class Table
    {
        public List<Column> Columns { get; private set; }
        public List<string> Headers { get; private set; } = new List<string>();
        public static Borders Borders { get; private set; }
        public int Width { get; private set; }

        public Table(List<Column> columns)
        {
            Columns = columns;

            foreach (Column column in Columns)
            {
                Headers.Add(column.Header);
                Width += column.Width;
            }

            Width += Columns.Count + 1;

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
                Placeholder = ' '
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

            row = new StringBuilder();

            for (int index = 0; index < Columns.Count; index++)
            {
                int width = Columns[index].Width;
                string start = index >= 1 ? string.Empty : left;

                row.Append(start);

                for (int i = 0; i < width; i++)
                {
                    row.Append(Borders.HorizontalLine);
                }

                if (index < Columns.Count - lastColumnsSpan) row.Append(center);
                else if (index == Columns.Count - 1) row.Append(right);
                else row.Append(bottomCenter);
            }

            return row.ToString();
        }

        private string DrawRowWithSpan(List<string> values, int lastColumnsSpan)
        {
            StringBuilder row = new StringBuilder();

            for (int index = 0; index < Columns.Count - lastColumnsSpan; index++)
            {
                int width = Columns[index].Width;
                string start = index >= 1 ? string.Empty : Borders.VerticalLine;
                row.Append(start);

                string cell = values[index];
                cell = NormalizeCell(cell, width, Columns[index].Alignment);

                row.Append(cell);
                row.Append(Borders.VerticalLine);
            }

            row.Append(values[values.Count - 1]);
            return row.ToString().PadRight(Width - 1, Borders.Placeholder) + Borders.VerticalLine;
        }

        private string DrawRow(List<string> values)
        {
            StringBuilder row = new StringBuilder();

            for (int index = 0; index < Columns.Count; index++)
            {
                int width = Columns[index].Width;
                string start = index >= 1 ? string.Empty : Borders.VerticalLine;
                row.Append(start);

                string cell = values[index];
                cell = NormalizeCell(cell, width, Columns[index].Alignment);

                row.Append(cell);
                row.Append(Borders.VerticalLine);
            }

            return row.ToString(); ;
        }

        private string DrawLine(string left, string right, string center)
        {
            StringBuilder row = new StringBuilder();

            row = new StringBuilder();

            for (int index = 0; index < Columns.Count; index++)
            {
                int column = Columns[index].Width;
                string start = index >= 1 ? string.Empty : left;

                row.Append(start);

                for (int i = 0; i < column; i++)
                {
                    row.Append(Borders.HorizontalLine);
                }

                string end = index == Columns.Count - 1 ? right : center;
                row.Append(end);
            }

            return row.ToString();
        }

        private static string NormalizeCell(string cell, int column, Alignment alignment)
        {
            if (cell.Length < column)
            {
                switch (alignment)
                {
                    case Alignment.Left:
                        cell = cell.PadRight(column, Borders.Placeholder);
                        break;
                    case Alignment.Right:
                        cell = cell.PadLeft(column, Borders.Placeholder);
                        break;
                    default:
                        break;
                }
            }
            else if (cell.Length > column && column > 3)
            {
                cell = cell.Substring(0, column - 3);
                cell += "...";
            }
            else
            {
                switch (alignment)
                {
                    case Alignment.Left:
                        cell.PadRight(column);
                        break;
                    case Alignment.Right:
                        cell.PadLeft(column);
                        break;
                    default:
                        break;
                }
            }

            cell = cell.Length > column ? string.Empty.PadLeft(column) : cell;

            return cell;
        }
    }
}