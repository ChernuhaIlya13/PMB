using System;

namespace PMB.Wpf.Client.Infra
{
    public class LogItem
    {
        public DateTime Time { get; set; } = new();

        public string BookmakerName { get; set; } = "";

        public string SportType { get; set; } = "";

        public string Info { get; set; } = "";

        public string Team { get; set; } = "";

        public string Teams { get; set; } = "";
    }
}