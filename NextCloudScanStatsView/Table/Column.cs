namespace NextCloudScanStatsView.Interface
{
    public class Column
    {
        public int Width { get; set; }
        public string Header { get; set; }

        public Column(int width, string header)
        {
            Width = width;
            Header = header;
        }
    }
}