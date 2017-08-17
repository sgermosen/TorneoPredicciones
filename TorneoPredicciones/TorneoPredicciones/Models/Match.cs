namespace TorneoPredicciones.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        public int DateId { get; set; }

        public string DateTime { get; set; }

        public int LocalId { get; set; }

        public int VisitorId { get; set; }

        public int? LocalGoals { get; set; }

        public int? VisitorGoals { get; set; }

        public int StatusId { get; set; }

        public int TournamentGroupId { get; set; }

        public bool WasPredicted { get; set; }

        public Team Local { get; set; }

        public Team Visitor { get; set; }

        public int? LocalGoals2
        {
            get {
                if (WasPredicted)
                {
                    return LocalGoals;
                }

                return null;
            }
        }

        public int? VisitorGoals2
        {
            get {
                if (WasPredicted)
                {
                    return VisitorGoals;
                }

                return null;
            }
        }
    }

}
