using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Backend;

namespace ProxymusCore.Router
{
    public class RoundRobinRouter : IRouter
    {
        private int _lastIndex = -1;
        public IBackendHost? Route(IBackendHost[] backendHosts)
        {
            IBackendHost availableHost = null;
            var newIndex = 0;
            for (int i = 0; i < backendHosts.Length; i++)
            {
                if (backendHosts[i].IsConnected)
                {
                    if (i > _lastIndex)
                    {
                        _lastIndex = i;
                        return backendHosts[i];
                    }
                    else
                    {
                        if (i <= newIndex)
                        {
                            newIndex = i;
                            availableHost = backendHosts[i];
                        }
                    }
                }
            }
            _lastIndex = newIndex;
            return availableHost;
        }
    }
}