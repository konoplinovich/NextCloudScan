namespace NextCloudScanStatsView.Interface
{
    public class Column
    {
        public int Width { get; set; }
        public string Header { get; set; }
        public Alignment Alignment { get; set; }

        public Column(int width, string header, Alignment alignment = Alignment.Right)
        {
            Width = width;
            Header = header;
            Alignment = alignment;
        }
    }
}