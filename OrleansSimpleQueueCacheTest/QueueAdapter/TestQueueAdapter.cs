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
        private readonly ILoggerFactory _loggerFactory;

        public string Name { get; }

        public bool IsRewindable => false;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadOnly;

        public TestQueueAdapter(string providerName, ILoggerFactory loggerFactory)
        {
            this.Name = providerName;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            return new TestQueueAdapterReceiver(
                "TestQueue",
                _loggerFactory
            );
        }

        public Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            throw new NotSupportedException();
        }
    }
}
