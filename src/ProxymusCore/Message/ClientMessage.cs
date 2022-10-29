using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Frontend.Client;

namespace ProxymusCore.Message
{
    public class ClientMessage : IMessage
    {
        public Guid Id => new Guid();
        public IClient Client { get; }
        public byte[] RequestData { get; }
        public byte[]? ResponseData { get; set; }
        public bool Errored { get; set; }

        public ClientMessage(IClient client, byte[] data)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.RequestData = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}