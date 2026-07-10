namespace CompeTournament.Shared.Scoring
{
    public enum MatchOutcome
    {
        LocalWin = 1,
        VisitorWin = 2,
        Draw = 3
    }

    public static class PredictionScoring
    {
        public const int ExactPoints = 3;
        public const int OutcomePoints = 1;
        public const int NoPoints = 0;

        public static MatchOutcome GetOutcome(int localPoints, int visitorPoints)
        {
            if (localPoints > visitorPoints)
            {
                return MatchOutcome.LocalWin;
            }

            if (visitorPoints > localPoints)
            {
                return MatchOutcome.VisitorWin;
            }

            return MatchOutcome.Draw;
        }

        public static int CalculatePoints(int matchLocalPoints, int matchVisitorPoints, int predictionLocalPoints, int predictionVisitorPoints)
        {
            if (matchLocalPoints == predictionLocalPoints && matchVisitorPoints == predictionVisitorPoints)
            {
                return ExactPoints;
            }

            var matchOutcome = GetOutcome(matchLocalPoints, matchVisitorPoints);
            var predictionOutcome = GetOutcome(predictionLocalPoints, predictionVisitorPoints);

            return matchOutcome == predictionOutcome ? OutcomePoints : NoPoints;
        }
    }
}
