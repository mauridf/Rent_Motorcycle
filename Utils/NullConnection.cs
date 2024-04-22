using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rent_Motorcycle.Utils
{
    public class NullConnection : IConnection
    {
        public ushort ChannelMax => 0;

        public IDictionary<string, object> ClientProperties => null;

        public ShutdownEventArgs CloseReason => null;

        public AmqpTcpEndpoint Endpoint => null;

        public uint FrameMax => 0;

        public TimeSpan Heartbeat => TimeSpan.Zero;

        public bool IsOpen => false;

        public AmqpTcpEndpoint[] KnownHosts => null;

        public IProtocol Protocol => null;

        public IDictionary<string, object> ServerProperties => null;

        public IList<ShutdownReportEntry> ShutdownReport => null;

        public string ClientProvidedName => null;

        public int LocalPort => 0;

        public int RemotePort => 0;

        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        public event EventHandler<EventArgs> ConnectionUnblocked;

        public void Abort() { }

        public void Abort(ushort reasonCode, string reasonText) { }

        public void Abort(TimeSpan timeout) { }

        public void Abort(ushort reasonCode, string reasonText, TimeSpan timeout) { }

        public void Close() { }

        public void Close(ushort reasonCode, string reasonText) { }

        public void Close(TimeSpan timeout) { }

        public void Close(ushort reasonCode, string reasonText, TimeSpan timeout) { }

        public IModel CreateModel() => null;

        public void Dispose() { }

        public void HandleConnectionBlocked(string reason) { }

        public void HandleConnectionUnblocked() { }

        public void UpdateSecret(string newSecret, string reason) { }
    }
}