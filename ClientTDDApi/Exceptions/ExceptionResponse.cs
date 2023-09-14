namespace ClientTDDApi.Exceptions
{
    public class ExceptionResponse
    {
        public string Error { get; set; } =string.Empty;
        public string Message { get; set; } = string.Empty;

        public ExceptionResponse() { }

        public ExceptionResponse(string error, string message)
        {
            Error = error;
            Message = message;
        }
    }
}
