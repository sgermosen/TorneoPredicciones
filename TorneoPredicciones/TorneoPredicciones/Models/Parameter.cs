using SQLite.Net.Attributes;

namespace TorneoPredicciones.Models
{
    public class Parameter
    {
        [PrimaryKey, AutoIncrement]
        public int ParameterId { get; set; }

        public string URLBase { get; set; }

        public string URLBase2 { get; set; }
      //  public string Option { get; set; }

        public override int GetHashCode()
        {
            return ParameterId;
        }
    }

}
