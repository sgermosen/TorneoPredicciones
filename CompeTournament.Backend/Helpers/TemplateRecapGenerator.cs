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
            if (leaderboard == null || leaderboard.Count == 0)
            {
                return Task.FromResult($"Todavia no hay puntos en {context.GroupName}. Se el primero en predecir y toma la delantera.");
            }

            var builder = new StringBuilder();
            var leader = leaderboard[0];
            builder.Append($"En {context.GroupName}, {leader.FullName} manda con {leader.Points} puntos.");

            if (leaderboard.Count > 1)
            {
                var second = leaderboard[1];
                var gap = leader.Points - second.Points;
                builder.Append(gap <= 1
                    ? $" {second.FullName} pisa los talones a solo {gap} punto de distancia."
                    : $" {second.FullName} persigue de cerca con {second.Points} puntos.");
            }

            if (leaderboard.Count > 2)
            {
                builder.Append($" Hay {leaderboard.Count} jugadores peleando por la cima.");
            }

            if (!string.IsNullOrWhiteSpace(context.LastMatchSummary))
            {
                builder.Append($" El ultimo partido cerro {context.LastMatchSummary} y sacudio la tabla.");
            }

            builder.Append(" La proxima jornada puede cambiarlo todo.");
            return Task.FromResult(builder.ToString());
        }
    }
}
