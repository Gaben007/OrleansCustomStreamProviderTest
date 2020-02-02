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
        private readonly Action<IEnumerable<IBatchContainer>> _onMessagesDelivered;

        public TestQueueAdapterReceiver(
            Func<IEnumerable<IBatchContainer>> queueMessagesProvider,
            Action<IEnumerable<IBatchContainer>> onMessagesDelivered,
            ILoggerFactory loggerFactory
        )
        {
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _queueMessagesProvider = queueMessagesProvider ?? throw new ArgumentNullException(nameof(queueMessagesProvider));
            _onMessagesDelivered = onMessagesDelivered ?? throw new ArgumentNullException(nameof(onMessagesDelivered));
            _logger = loggerFactory.CreateLogger<TestQueueAdapterReceiver>();

        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            await Task.Delay(100); // simulates real query
            return _queueMessagesProvider().ToList();
        }

        public Task Initialize(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }

        public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            _onMessagesDelivered(messages);
            return Task.CompletedTask;
        }

        public Task Shutdown(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }
    }
}
