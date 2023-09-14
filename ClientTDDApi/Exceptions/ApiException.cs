namespace ClientTDDApi.Exceptions
{
    public abstract class ApiException : Exception
    {
        public ApiException(string msg): base(msg) { }
        public abstract string GetError();

        public abstract int GetStatus();
    }
}
