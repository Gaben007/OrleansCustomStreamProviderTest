using Microsoft.Extensions.Logging;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansSimpleQueueCacheTest.QueueAdapter
{
    internal class TestQueueAdapterReceiver : IQueueAdapterReceiver
    {
        private readonly ILogger _logger;
        private readonly Func<IEnumerable<IBatchContainer>> _queueMessagesProvider;

        public TestQueueAdapterReceiver(
            Func<IEnumerable<IBatchContainer>> queueMessagesProvider,
            ILoggerFactory loggerFactory
        )
        {
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _queueMessagesProvider = queueMessagesProvider ?? throw new ArgumentNullException(nameof(queueMessagesProvider));
            _logger = loggerFactory.CreateLogger<TestQueueAdapterReceiver>();

        }

        public Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            return Task.FromResult<IList<IBatchContainer>>(_queueMessagesProvider().ToList());
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
