using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansSimpleQueueCacheTest.QueueAdapter
{
    internal class TestQueueAdapterReceiver : IQueueAdapterReceiver
    {
        private readonly ILogger _logger;
        private readonly string _queueName;
        private long _sequenceId = 0;

        public TestQueueAdapterReceiver(string queueName, ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = loggerFactory.CreateLogger<TestQueueAdapterReceiver>();

        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var batch = new List<IBatchContainer>();
            return batch;
        }

        public Task Initialize(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }

        public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            return Task.CompletedTask;
        }

        public Task Shutdown(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }
    }
}
