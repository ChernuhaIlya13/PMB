using System;
using System.Collections.Generic;

namespace PMB.Pinnacle.Models
{
    public class Sport
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

    public class League
    {
        public int AgeLimit { get; set; }
        
        public object External { get; set; }
        
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
        
        public Sport Sport { get; set; }
    }

    public class Participant
    {
        public string Alignment { get; set; }
        
        public string Name { get; set; }
        
        public int Order { get; set; }
    }

    public class ParticipantExtended : Participant
    {
        public State State { get; set; }
    }

    public class State
    {
        public int RedCards { get; set; }
        
        public int Score { get; set; }
    }

    public class StateMinutes
    {
        public int? Minutes { get; set; }
        
        public int? State { get; set; }
    }

    public class PeriodInfo
    {
        public DateTime? CutoffAt { get; set; }
        
        public bool HasMoneyline { get; set; }
        
        public bool HasSpread { get; set; }
        
        public bool HasTeamTotal { get; set; }
        
        public bool HasTotal { get; set; }
        
        public int Period { get; set; }
        
        public string Status { get; set; }
    }

    public class RelatedMatchUpsResult
    {
        public int AgeLimit { get; set; }
        
        public bool AltTeaser { get; set; }
        
        public bool HasAltSpread { get; set; }
        
        public bool HasAltTotal { get; set; }
        
        public bool HasLive { get; set; }
        
        public bool HasMarkets { get; set; }
        
        public long Id { get; set; }
        
        public bool IsBetshareEnabled { get; set; }
        
        public bool IsHighlighted { get; set; }
        
        public bool IsLive { get; set; }
        
        public bool IsPromoted { get; set; }
        
        public League League { get; set; }
        
        public string LiveMode { get; set; }
        
        public object Parent { get; set; }
                      
        public object ParentId { get; set; }
                      
        public string ParlayRestriction { get; set; }
        
        public List<Participant> Participants { get; set; }
        
        public List<PeriodInfo> Periods { get; set; }
        
        public int Rotation { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public string Status { get; set; }
        
        public StateMinutes State { get; set; }
        
        public int TotalMarketCount { get; set; }
        
        public string Type { get; set; }
        
        public string Units { get; set; }
        
        public int Version { get; set; }
    }
}