namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class Match
    {
        [Key]
        public int MatchId { get; set; }

        [Display(Name = "Date")]
        public int DateId { get; set; }

        [Display(Name = "Date time")]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        [Display(Name = "Local")]
        public int LocalId { get; set; }

        [Display(Name = "Visitor")]
        public int VisitorId { get; set; }

        [Display(Name = "Local goals")]
        public int? LocalGoals { get; set; }

        [Display(Name = "Visitor goals")]
        public int? VisitorGoals { get; set; }

        [Display(Name = "Status")]
        public int StatusId { get; set; }

        [Display(Name = "Group")]
        public int TournamentGroupId { get; set; }

        [JsonIgnore]
        public virtual Date Date { get; set; }
        [JsonIgnore]
        public virtual Team Local { get; set; }
        [JsonIgnore]
        public virtual Team Visitor { get; set; }
        [JsonIgnore]
        public virtual Status Status { get; set; }
        [JsonIgnore]
        public virtual TournamentGroup TournamentGroup { get; set; }
        [JsonIgnore]
        public virtual ICollection<Prediction> Predictions { get; set; }
    }

}
