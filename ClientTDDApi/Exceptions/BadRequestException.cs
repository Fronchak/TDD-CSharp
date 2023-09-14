namespace ClientTDDApi.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string msg) : base(msg)
        {
        }

        public override string GetError()
        {
            return "Bad request";
        }

        public override int GetStatus()
        {
            return 400;
        }
    }
}
