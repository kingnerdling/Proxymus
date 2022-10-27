namespace ProxymusCore.Frontend.Client
{
    public interface IClient
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Created { get; }
        public void Dispose();
    }
}