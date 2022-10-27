using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Frontend.Client;

namespace ProxymusCore.Message
{
    public interface IMessage
    {
        public Guid Id { get; }
        public IClient Client { get; }
        public byte[] RequestData { get; }
        public byte[]? ResponseData { get; set; }
        public bool Errored { get; set; }
    }
}