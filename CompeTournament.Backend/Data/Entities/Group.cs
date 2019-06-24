using CompeTournament.Backend.Helpers;

namespace CompeTournament.Backend.Data.Entities
{
    public class Group : AuditEntity
    {
        public string Requirements { get; set; }

        public string Logo { get; set; }

    }
}