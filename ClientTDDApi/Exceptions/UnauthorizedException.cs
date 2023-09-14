namespace ClientTDDApi.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string msg) : base(msg)
        {
        }

        public override string GetError()
        {
            return "Unauthorized";
        }

        public override int GetStatus()
        {
            return 401;
        }
    }
}
