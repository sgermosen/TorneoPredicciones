using CompeTournament.Backend.Helpers;

namespace CompeTournament.Backend.Data.Entities
{
    public class Group : AuditEntity,  IBaseEntity
    {
        public string Requirements { get; set; }

        public string Logo { get; set; }

    }
}