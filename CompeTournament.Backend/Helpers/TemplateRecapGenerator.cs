namespace CompeTournament.Backend.Helpers
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TemplateRecapGenerator : IJornadaRecapGenerator
    {
        public Task<string> GenerateAsync(RecapContext context)
        {
            var leaderboard = context.Leaderboard;
            if (leaderboard == null || leaderboard.Count == 0 || leaderboard[0].Points == 0)
            {
                return Task.FromResult($"Todavía no hay puntos en {context.GroupName}. Sé el primero en predecir y toma la delantera.");
            }

            var builder = new StringBuilder();
            var leader = leaderboard[0];
            builder.Append($"En {context.GroupName}, {leader.FullName} manda con {Points(leader.Points)}.");

            if (leaderboard.Count > 1)
            {
                var second = leaderboard[1];
                var gap = leader.Points - second.Points;
                builder.Append(gap == 0
                    ? $" {second.FullName} está empatado en la cima."
                    : gap == 1
                        ? $" {second.FullName} le pisa los talones a solo 1 punto de distancia."
                        : $" {second.FullName} persigue con {Points(second.Points)}.");
            }

            if (leaderboard.Count > 2)
            {
                builder.Append($" Hay {leaderboard.Count} jugadores peleando por la cima.");
            }

            if (!string.IsNullOrWhiteSpace(context.LastMatchSummary))
            {
                builder.Append($" El último partido cerró {context.LastMatchSummary} y sacudió la tabla.");
            }

            builder.Append(" La próxima jornada puede cambiarlo todo.");
            return Task.FromResult(builder.ToString());
        }

        private static string Points(int points) => points == 1 ? "1 punto" : $"{points} puntos";
    }
}
