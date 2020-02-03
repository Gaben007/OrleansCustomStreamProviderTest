using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using OrleansSimpleQueueCacheTest.QueueAdapter;
using Orleans.Configuration;
using Orleans;
using Orleans.Runtime;
using Orleans.Hosting;
using Orleans.Streams;
using Grains;

namespace OrleansSimpleQueueCacheTest
{
    public abstract class TestBase
    {
        private ISiloHost _host;

        protected IServiceProvider ServiceProvider { get; private set; }

        [TestInitialize]
        public async Task InitTest()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = configuration.GetConnectionString("Default");

            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services.AddDbContextPool<TestDbContext>(options =>
                        options.UseSqlServer(connectionString)
                    )
                    .AddSingleton<DbMigrator>()
                    .AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(provider => provider.GetRequiredService<DbMigrator>());

                    ConfigureServices(services);
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "TEST";
                })
                .AddMemoryGrainStorage("MemoryStorage")
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryGrainStorage("StorageProvider")
                .AddPersistentStreams("TestStreamProvider", new TestAdapterFactory.FactoryProvider(ProvideMessages, OnMessagesDelivered).Create, streamBuilder =>
                    streamBuilder.Configure<StreamPullingAgentOptions>(ob =>
                        ob.Configure(options => options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(100))
                ))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(FailedToHandleMessageTestGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());


            _host = builder.Build();
            this.ServiceProvider = _host.Services;
            await _host.StartAsync().ConfigureAwait(false);
        }

        [TestCleanup]
        public async Task CleanupTest()
        {
            if (_host != null)
            {
                await _host.StopAsync().ConfigureAwait(false);
                _host.Dispose();
                _host = null;
            }
        }

        protected abstract IEnumerable<IBatchContainer> ProvideMessages();

        protected abstract void OnMessagesDelivered(IEnumerable<IBatchContainer> messages);

        protected virtual void ConfigureServices(IServiceCollection services) { }
    }
}
