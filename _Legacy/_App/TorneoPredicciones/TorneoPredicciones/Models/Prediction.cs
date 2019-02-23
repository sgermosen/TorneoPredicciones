namespace TorneoPredicciones.Models
{
  public  class Prediction
    {

        public int PredictionId { get; set; }

      
        public int UserId { get; set; }

       
        public int MatchId { get; set; }

       
        public int LocalGoals { get; set; }

       
        public int VisitorGoals { get; set; }

        public int Points { get; set; }

       

    }
}
