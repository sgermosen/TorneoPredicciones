namespace Backend.Classes
{
    public class Response
    {
       // public bool Succeeded { get; set; }

       // public string Message { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }
    }
}