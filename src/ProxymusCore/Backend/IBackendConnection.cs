using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Backend
{
    public interface IBackendConnection
    {
        public Guid Id { get; }
    }
}