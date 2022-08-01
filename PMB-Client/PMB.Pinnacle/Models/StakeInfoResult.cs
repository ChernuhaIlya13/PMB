using System;
using System.Collections.Generic;

    public class MarketNew
    {
        public string Key { get; set; }
        public int Period { get; set; }
        public string Type { get; set; }
    }

    public class ExternalNew
    {
    }

    public class SportNew
    {
        public int FeatureOrder { get; set; }
        public int Id { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSticky { get; set; }
        public int MatchupCount { get; set; }
        public int MatchupCountSE { get; set; }
        public string Name { get; set; }
        public string PrimaryMarketType { get; set; }
    }

    public class LeagueNew
    {
        public int AgeLimit { get; set; }
        public ExternalNew External { get; set; }
        public int FeatureOrder { get; set; }
        public string Group { get; set; }
        public int Id { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHidden { get; set; }
        public bool IsPromoted { get; set; }
        public bool IsSticky { get; set; }
        public int MatchupCount { get; set; }
        public int MatchupCountSE { get; set; }
        public string Name { get; set; }
        public SportNew Sport { get; set; }
    }

    public class ParticipantNew
    {
        public string Alignment { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class StateNew
    {
    }

    public class MatchupNew
    {
        public int AgeLimit { get; set; }
        public bool AltTeaser { get; set; }
        public ExternalNew External { get; set; }
        public bool HasAltSpread { get; set; }
        public bool HasAltTotal { get; set; }
        public bool HasLive { get; set; }
        public bool HasMarkets { get; set; }
        public int Id { get; set; }
        public bool IsBetshareEnabled { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsLive { get; set; }
        public bool IsPromoted { get; set; }
        public LeagueNew League { get; set; }
        public object LiveMode { get; set; }
        public object Parent { get; set; }
        public object ParentId { get; set; }
        public object ParlayRestriction { get; set; }
        public List<ParticipantNew> Participants { get; set; }
        public List<object> Periods { get; set; }
        public object Rotation { get; set; }
        public DateTime StartTime { get; set; }
        public StateNew State { get; set; }
        public object Status { get; set; }
        public object TotalMarketCount { get; set; }
        public string Type { get; set; }
        public object Units { get; set; }
        public object Version { get; set; }
    }

    public class SelectionNew
    {
        public string Designation { get; set; }
        public MarketNew Market { get; set; }
        public MatchupNew Matchup { get; set; }
        public string Outcome { get; set; }
        public double Points { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }
    }
    public class StakeInfoResult
    {
        public DateTime CreatedAt { get; set; }
        public int Id { get; set; }
        public string OddsFormat { get; set; }
        public string Outcome { get; set; }
        public double Price { get; set; }
        public string RequestId { get; set; }
        public List<SelectionNew> Selections { get; set; }
        public object SettledAt { get; set; }
        public double Stake { get; set; }
        public string Status { get; set; }
        public double ToWin { get; set; }
        public string Type { get; set; }
        public int WagerNumber { get; set; }
        public object WinLoss { get; set; }
    }