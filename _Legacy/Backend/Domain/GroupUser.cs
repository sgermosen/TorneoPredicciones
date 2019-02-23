namespace Domain
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class GroupUser
    {
        [Key]
        public int GroupUserId { get; set; }

        [Index("GroupUser_GroupId_UserId_Index", IsUnique = true, Order = 1)]
        [Display(Name = "Group")]
        public int GroupId { get; set; }

        [Index("GroupUser_GroupId_UserId_Index", IsUnique = true, Order = 2)]
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "Is accepted?")]
        public bool IsAccepted { get; set; }

        [Display(Name = "Is blocked?")]
        public bool IsBlocked { get; set; }

        public int Points { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
    }

}
