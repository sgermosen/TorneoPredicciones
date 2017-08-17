using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class EditPredictionViewModel : Match
    {

        private Match match;

        public EditPredictionViewModel(Match match)
        {
            this.match= match;

            DateId = match.DateId;
            DateTime = match.DateTime;
            Local = match.Local;
            LocalGoals = match.LocalGoals;
            LocalId = match.LocalId;
            MatchId = match.MatchId;
            StatusId = match.StatusId;
            TournamentGroupId = match.TournamentGroupId;
            Visitor = match.Visitor;
            VisitorGoals = match.VisitorGoals;
            VisitorId = match.VisitorId;
            WasPredicted = match.WasPredicted
        }
    }
}
