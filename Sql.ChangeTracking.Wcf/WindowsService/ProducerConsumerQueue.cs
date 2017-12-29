using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlChangeTrackingProducerConsumer
{

    public class ProducerConsumerQueue<T> : IDisposable
    {
        private readonly Action<T> _consumer;
        private readonly BlockingCollection<T> _blockingCollection;
        private readonly Task[] _workers;

        public ProducerConsumerQueue(Action<T> consumer, int degreeOfParallelism, ILogger logger, CancellationToken token)
        {
            _consumer = consumer;

            _blockingCollection = new BlockingCollection<T>();

            Logger = logger;

            _workers = Enumerable.Range(1, degreeOfParallelism)
                .Select
                (
                    _ => Task.Run(() => DoWork())
                    .ContinueWith(
                        (c,d) =>
                        {
                            foreach (var item in c.Exception.InnerExceptions)
                            {
                                Console.WriteLine(item.Message);
                                Logger.Error(item, item.Message);
                            }
                        }, 
                        TaskContinuationOptions.OnlyOnFaulted , 
                        token)
                ).ToArray();
        }

        public ILogger Logger { get; }

        public void Produce(T item)
        {
            Console.WriteLine("Adding item to the collection.");
            _blockingCollection.Add(item);
            Console.WriteLine("Items in collection: " + _blockingCollection.Count);
        }

        public void CompleteProcessing()
        {
            _blockingCollection.CompleteAdding();
        }

        public void Dispose()
        {
            if (!_blockingCollection.IsAddingCompleted)
            {
                _blockingCollection.CompleteAdding();
            }

            Task.WaitAll(_workers);
            _blockingCollection.Dispose();
        }

        private void DoWork()
        {
            foreach (var item in _blockingCollection.GetConsumingEnumerable())
            {
                _consumer(item);
            }
        }
    }

}
