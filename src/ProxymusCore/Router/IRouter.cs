using ProxymusCore.Backend;

namespace ProxymusCore.Router
{
    public interface IRouter
    {
        public IBackendHost? Route(IBackendHost[] backendHosts);
    }
}