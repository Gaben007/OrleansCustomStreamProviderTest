using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Configuration;
using System;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using System.Threading.Tasks;
using System.Net.Sockets;
using OrleansSimpleQueueCacheTest.QueueAdapter;
using Orleans.Streams;
using System.Collections.Generic;

namespace OrleansSimpleQueueCacheTest
{
    [TestClass]
    public class SimpleQueueCacheTests
    {
        private ISiloHost _host;

        protected IServiceProvider ServiceProvider { get; private set; }

        [TestInitialize]
        public async Task InitTest()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = configuration.GetConnectionString("Default");

            var builder = new SiloHostBuilder()
                 .ConfigureServices((hostBuilderContext, services) =>
                 {
                     services.AddDbContextPool<TestDbContext>(options =>
                         options.UseSqlServer(connectionString)
                     )
                     .AddSingleton<DbMigrator>()
                     .AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(provider => provider.GetRequiredService<DbMigrator>());
                 })
                 .Configure<ClusterOptions>(options =>
                 {
                     options.ClusterId = "dev";
                     options.ServiceId = "TEST";
                 })
                 .ConfigureEndpoints(22222, 40000, AddressFamily.InterNetwork, true)
                 .UseAdoNetClustering(options =>
                 {
                     options.Invariant = "System.Data.SqlClient";
                     options.ConnectionString = connectionString;
                 })
                 .AddAdoNetGrainStorage("TestDatabaseStorage", options =>
                 {
                     options.Invariant = "System.Data.SqlClient";
                     options.ConnectionString = connectionString;
                     options.UseJsonFormat = true;
                 })
                 .AddMemoryGrainStorage(name: "MemoryStorage")
                 .AddPersistentStreams("TestStreamProvider", new TestAdapterFactory.FactoryProvider(ProvideMessages).Create, streamBuilder =>
                     streamBuilder.Configure<StreamPullingAgentOptions>(ob =>
                         ob.Configure(options => options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(100))
                 ))
                 .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(SimpleQueueCacheTests).Assembly).WithReferences());

            _host = builder.Build();
            this.ServiceProvider = _host.Services;
            await _host.StartAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            await Task.Delay(1000);
            ;
        }

        private IEnumerable<IBatchContainer> ProvideMessages()
        {
            return new List<IBatchContainer>();
        }
    }
}
