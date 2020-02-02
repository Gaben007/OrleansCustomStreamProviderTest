using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansSimpleQueueCacheTest.QueueAdapter
{
    internal class TestQueueAdapter : IQueueAdapter
    {
        private readonly Func<IEnumerable<IBatchContainer>> _queueMessagesProvider;
        private readonly Action<IEnumerable<IBatchContainer>> _onMessagesDelivered;
        private readonly ILoggerFactory _loggerFactory;

        public string Name { get; }

        public bool IsRewindable => false;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadOnly;

        public TestQueueAdapter(
            string providerName,
            Func<IEnumerable<IBatchContainer>> queueMessagesProvider,
            Action<IEnumerable<IBatchContainer>> onMessagesDelivered,
            ILoggerFactory loggerFactory
        )
        {
            this.Name = providerName;
            _queueMessagesProvider = queueMessagesProvider ?? throw new ArgumentNullException(nameof(queueMessagesProvider));
            _onMessagesDelivered = onMessagesDelivered ?? throw new ArgumentNullException(nameof(onMessagesDelivered));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            return new TestQueueAdapterReceiver(
                _queueMessagesProvider,
                _onMessagesDelivered,
                _loggerFactory
            );
        }

        public Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            throw new NotSupportedException();
        }
    }
}
