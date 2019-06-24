namespace CompeTournament.Backend.Helpers
{
    public interface IBaseEntity
    {
        int Id { get; set; }

        string Name { get; set; }

        bool Active { get; set; }
    }

    public class BaseEntity 
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

    }
}
