namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class MessageReceiver : IMessageReceiver
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly object _lockObject = new object();
        private CancellationTokenSource _cancellationSource;

        public void Start()
        {
            lock (_lockObject)
            {
                if (_cancellationSource == null)
                {
                    _cancellationSource = new CancellationTokenSource();
                    Task.Factory.StartNew(
                        () => ReceiveMessages(_cancellationSource.Token),
                        _cancellationSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Current);
                }
            }
        }

        /// <summary>
        /// Receives the messages in an endless loop.
        /// </summary>
        private void ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var pipe = new NamedPipeClientStream(".", "Glen.ShoppingList.Pipe", PipeDirection.InOut, PipeOptions.None);
                Console.WriteLine("Connecting...");
                pipe.Connect();

                var sr = new StreamReader(pipe);

                var jsonMessage = sr.ReadLine();

                Console.WriteLine("Message received {0}", jsonMessage);

                var message = JsonConvert.DeserializeObject<Message>(jsonMessage);

                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                using (_cancellationSource)
                {
                    if (_cancellationSource != null)
                    {
                        _cancellationSource.Cancel();
                        _cancellationSource = null;
                    }
                }
            }
        }
    }
}
