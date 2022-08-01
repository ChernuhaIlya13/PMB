namespace PMB.Domain.BrowserModels
{
    public class MatchData
    {
        /// <summary>
        /// На Марафоне это могут быть 2:4 (0:2, 2:1, 0:1)
        /// На Лигеставок это 2-й тайм
        /// </summary>
        public string AdditionalData { get; set; }
        
        public string AdditionalDataSet { get; set; }
        
        public string AdditionalDataGame { get; set; }
        
        public string AdditionalDataScore { get; set; }
        public Time Time { get; set; }
    }

    public class Time
    {
        public string MainTime { get; set; }
        /// <summary>
        /// Если есть дополнительное время,хранится здесь
        /// </summary>
        public string AdditionalTime { get; set; }
    }
}