using CompeTournament.Shared.Scoring;
using Xunit;

namespace CompeTournament.Tests
{
    public class PredictionScoringTests
    {
        [Theory]
        [InlineData(2, 1, 2, 1, 3)]
        [InlineData(0, 0, 0, 0, 3)]
        [InlineData(3, 1, 2, 0, 1)]
        [InlineData(0, 0, 1, 1, 1)]
        [InlineData(1, 2, 2, 1, 0)]
        [InlineData(2, 2, 3, 0, 0)]
        public void CalculatePoints_ReturnsExpected(int matchLocal, int matchVisitor, int predLocal, int predVisitor, int expected)
        {
            var points = PredictionScoring.CalculatePoints(matchLocal, matchVisitor, predLocal, predVisitor);
            Assert.Equal(expected, points);
        }

        [Theory]
        [InlineData(3, 1, MatchOutcome.LocalWin)]
        [InlineData(1, 3, MatchOutcome.VisitorWin)]
        [InlineData(2, 2, MatchOutcome.Draw)]
        public void GetOutcome_ReturnsExpected(int local, int visitor, MatchOutcome expected)
        {
            Assert.Equal(expected, PredictionScoring.GetOutcome(local, visitor));
        }

        [Fact]
        public void ExactPrediction_BeatsOutcomeOnlyPrediction()
        {
            var exact = PredictionScoring.CalculatePoints(2, 1, 2, 1);
            var outcomeOnly = PredictionScoring.CalculatePoints(2, 1, 3, 0);

            Assert.True(exact > outcomeOnly);
            Assert.Equal(PredictionScoring.ExactPoints, exact);
            Assert.Equal(PredictionScoring.OutcomePoints, outcomeOnly);
        }
    }
}
