namespace ClientTDDApi.Exceptions
{
    public class EntityNotFoundException : ApiException
    {
        public EntityNotFoundException(string msg) : base(msg) { }

        public override string GetError()
        {
            return "Entity not found";
        }

        public override int GetStatus()
        {
            return 404;
        }
    }
}
