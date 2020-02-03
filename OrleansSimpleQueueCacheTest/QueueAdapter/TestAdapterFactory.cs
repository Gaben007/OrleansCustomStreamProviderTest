using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansSimpleQueueCacheTest.QueueAdapter
{
    public class TestAdapterFactory : IQueueAdapterFactory
    {
        private readonly string _providerName;
        private readonly Func<IEnumerable<IBatchContainer>> _queueMessagesProvider;
        private readonly Action<IEnumerable<IBatchContainer>> _onMessagesDelivered;
        private readonly ILoggerFactory _loggerFactory;
        private IStreamQueueMapper _streamQueueMapper;
        private IQueueAdapterCache _adapterCache;

        public TestAdapterFactory(
            string name,
            SimpleQueueCacheOptions cacheOptions,
            Func<IEnumerable<IBatchContainer>> queueMessagesProvider,
            Action<IEnumerable<IBatchContainer>> onMessagesDelivered,
            ILoggerFactory loggerFactory
            )
        {
            _providerName = name;
            _queueMessagesProvider = queueMessagesProvider ?? throw new ArgumentNullException(nameof(queueMessagesProvider));
            _onMessagesDelivered = onMessagesDelivered ?? throw new ArgumentNullException(nameof(onMessagesDelivered));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(new HashRingStreamQueueMapperOptions() { TotalQueueCount = 1 }, _providerName);
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _providerName, _loggerFactory);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            return Task.FromResult<IQueueAdapter>(
                new TestQueueAdapter(_providerName, _queueMessagesProvider, _onMessagesDelivered, _loggerFactory)
            );
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return _adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }

        public class FactoryProvider
        {
            private readonly Func<IEnumerable<IBatchContainer>> _queueMessagesProvider;
            private readonly Action<IEnumerable<IBatchContainer>> _onMessagesDelivered;

            public FactoryProvider(Func<IEnumerable<IBatchContainer>> queueMessagesProvider, Action<IEnumerable<IBatchContainer>> onMessagesDelivered)
            {
                _queueMessagesProvider = queueMessagesProvider ?? throw new ArgumentNullException(nameof(queueMessagesProvider));
                _onMessagesDelivered = onMessagesDelivered ?? throw new ArgumentNullException(nameof(onMessagesDelivered));
            }

            public TestAdapterFactory Create(IServiceProvider services, string providerName)
            {
                var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(providerName);
                return ActivatorUtilities.CreateInstance<TestAdapterFactory>(services, providerName, cacheOptions, _queueMessagesProvider, _onMessagesDelivered);
            }
        }
    }
}
