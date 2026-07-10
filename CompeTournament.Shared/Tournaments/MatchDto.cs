namespace CompeTournament.Shared.Tournaments
{
    using System;

    public class MatchDto
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public int GroupId { get; set; }

        public int LocalId { get; set; }

        public string LocalName { get; set; }

        public string LocalInitials { get; set; }

        public string LocalPictureUrl { get; set; }

        public int VisitorId { get; set; }

        public string VisitorName { get; set; }

        public string VisitorInitials { get; set; }

        public string VisitorPictureUrl { get; set; }

        public int? LocalPoints { get; set; }

        public int? VisitorPoints { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; }

        public bool IsOpen { get; set; }

        public PredictionDto MyPrediction { get; set; }
    }
}
