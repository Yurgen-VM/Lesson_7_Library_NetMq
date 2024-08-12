using System.Text.Json;
using System.Threading.Channels;

namespace Core
{

    public abstract class ChatBase
    {
        protected CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        protected CancellationToken CancellationToken => CancellationTokenSource.Token;
        
        protected abstract Task Listener();
        public abstract Task Start();
    }
}
