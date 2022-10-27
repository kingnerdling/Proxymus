using ProxymusCore.Router;
using ProxymusCore.Backend;
using ProxymusCore.Frontend;
using System.Collections.Concurrent;
using ProxymusCore.Message;

namespace ProxymusCore.Proxy
{
    public class Proxy : IProxy
    {
        public IFrontend Frontend { get; }
        public IBackend Backend { get; }
        public IEnumerable<IMessage> MessageQueue => _messageQueue.ToArray();
        public int MessageQueueLength { get; }
        public Guid Id => new Guid();
        public string Name { get; }

        private BlockingCollection<IMessage> _messageQueue;
        public Proxy(string name, IFrontend frontend, IBackend backend, int messageQueueLength)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Frontend = frontend ?? throw new ArgumentNullException(nameof(frontend));
            this.Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            this.MessageQueueLength = messageQueueLength;
            _messageQueue = new BlockingCollection<IMessage>(messageQueueLength);
            frontend.New_Message(Frontend_newMessage);
        }

        public void Start()
        {
            Backend.Start();
            Frontend.Start();
        }

        public void Stop()
        {
            Backend.Stop();
            Frontend.Stop();
        }

        private void Frontend_newMessage(IMessage msg)
        {
            _messageQueue.Add(msg);
            Backend.ProcessMessage(msg);
        }
    }
}