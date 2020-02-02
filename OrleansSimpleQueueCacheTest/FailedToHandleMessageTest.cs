using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Streams;
using OrleansSimpleQueueCacheTest.QueueAdapter;
using System.Linq;
using FluentAssertions;
using Orleans;
using Microsoft.Extensions.DependencyInjection;

namespace OrleansSimpleQueueCacheTest
{
    [TestClass]
    public class FailedToHandleMessageTest : TestBase
    {
        private bool _isFirst = true;
        private TestBatchContainer _sampleMessage = new TestBatchContainer(Guid.Parse("997dbf27-ccba-4cdf-8765-cb1936335085"), nameof(FailedToHandleMessageTest), new object[] { "sample message" }.ToList(), 0);
        private List<IBatchContainer> _deliveredMessages = new List<IBatchContainer>();
        private FailedToHandleMessageTestState _state = new FailedToHandleMessageTestState();

        [TestMethod]
        public async Task MessageHandlerThrowsException()
        {
            var startAt = DateTime.Now;

            // wait for retry
            while (!_state.FinishedRetryOnError.GetValueOrDefault(_sampleMessage.StreamGuid))
            {
                // timeout for test
                if (startAt.AddMinutes(2) < DateTime.Now)
                    throw new TimeoutException();

                // before the retry is finifhed for the message it shouldn't be marked as delivered
                _deliveredMessages.Should().HaveCount(0);
                await Task.Delay(300);
            }

            // wait for message delivering
            await Task.Delay(1000);

            _deliveredMessages.Should().HaveCount(0);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_state);
        }

        protected override void OnMessagesDelivered(IEnumerable<IBatchContainer> messages)
        {
            _deliveredMessages.AddRange(messages);
        }

        protected override IEnumerable<IBatchContainer> ProvideMessages()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return new IBatchContainer[] { _sampleMessage };
            }

            return Enumerable.Empty<IBatchContainer>();
        }
    }

    public interface IFailedToHandleMessageTestGrain : IGrainWithGuidKey { }

    [ImplicitStreamSubscription(nameof(FailedToHandleMessageTest))]
    public class FailedToHandleMessageTestGrain : Grain, IFailedToHandleMessageTestGrain, IAsyncObserver<string>
    {
        private readonly FailedToHandleMessageTestState _state;

        public FailedToHandleMessageTestGrain(FailedToHandleMessageTestState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task OnActivateAsync()
        {
            await this.GetStreamProvider("TestStreamProvider").GetStream<string>(this.GetPrimaryKey(), nameof(FailedToHandleMessageTest)).SubscribeAsync(this);
        }

        public Task OnCompletedAsync()
        {
            _state.FinishedRetryOnError.Add(this.GetPrimaryKey(), false);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _state.FinishedRetryOnError.Add(this.GetPrimaryKey(), true);
            return Task.CompletedTask;
        }

        public Task OnNextAsync(string item, StreamSequenceToken token = null)
        {
            throw new NotImplementedException();
        }
    }

    public class FailedToHandleMessageTestState
    {
        public Dictionary<Guid, bool> FinishedRetryOnError { get; set; } = new Dictionary<Guid, bool>();
    }
}
