namespace PMB.Browsers.Common
{
    public class SettingsOptions
    {
        public string Path { get; set; }
        
        public BookmakerInfo[] Bookmakers { get; set; }

        public class BookmakerInfo
        {
            public string Bookmaker { get; set; }
            
            public int LoadCount { get; set; }
            
            public bool NeedProxy { get; set; }
        }
    }
}