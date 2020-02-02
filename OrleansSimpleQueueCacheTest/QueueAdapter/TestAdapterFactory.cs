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
        private readonly ILoggerFactory _loggerFactory;
        private IStreamQueueMapper _streamQueueMapper;
        private IQueueAdapterCache _adapterCache;

        public TestAdapterFactory(
            string name,
            SimpleQueueCacheOptions cacheOptions,
            ILoggerFactory loggerFactory
            )
        {
            _providerName = name;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(new HashRingStreamQueueMapperOptions() { TotalQueueCount = 1 }, _providerName);
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, this._providerName, this._loggerFactory);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            throw new NotImplementedException();
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

        public static TestAdapterFactory Create(IServiceProvider services, string providerName)
        {
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(providerName);
            return ActivatorUtilities.CreateInstance<TestAdapterFactory>(services, providerName, cacheOptions);
        }
    }
}
