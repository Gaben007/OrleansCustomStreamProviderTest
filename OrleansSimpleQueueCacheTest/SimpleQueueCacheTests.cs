using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Configuration;
using System;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace OrleansSimpleQueueCacheTest
{
    [TestClass]
    public class SimpleQueueCacheTests
    {
        private IHost _host;

        protected IServiceProvider ServiceProvider { get; private set; }

        [TestInitialize]
        public async Task InitTest()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = configuration.GetConnectionString("Default");

            //var services = new ServiceCollection();
            //services.AddDbContextPool<TestDbContext>(options =>
            //    options.UseSqlServer(connectionString)
            //);

            //var provider = services.BuildServiceProvider(true);
            //this.ServiceProvider = provider.CreateScope().ServiceProvider;

            
            var builder = new HostBuilder()
                    .UseOrleans((context, siloBuilder) =>
                    {
                        context.Configuration.Bind(connectionString);

                        siloBuilder
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
                            //.UseAdoNetReminderService(options =>
                            //{
                            //    options.Invariant = "System.Data.SqlClient";
                            //    options.ConnectionString = connectionString;
                            //})
                            .AddAdoNetGrainStorage("TestDatabaseStorage", options =>
                            {
                                options.Invariant = "System.Data.SqlClient";
                                options.ConnectionString = connectionString;
                                options.UseJsonFormat = true;
                            })
                            .AddMemoryGrainStorage(name: "MemoryStorage")
                            .AddPersistentStreams("TestStreamProvider", TestAdapterFactory.Create, streamBuilder =>
                                streamBuilder.Configure<StreamPullingAgentOptions>(ob =>
                                    ob.Configure(options => options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(100))
                            ))
                            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(SimpleQueueCacheTests).Assembly).WithReferences());
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddDbContextPool<TestDbContext>(options =>
                            options.UseSqlServer(connectionString)
                        )
                        .Configure<ConsoleLifetimeOptions>(options =>
                        {
                            options.SuppressStatusMessages = true;
                        });

                    });

            _host = builder.UseConsoleLifetime().Build();
            this.ServiceProvider = _host.Services;
            await _host.RunAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            await Task.Delay(100000);
            ;
        }
    }
}
