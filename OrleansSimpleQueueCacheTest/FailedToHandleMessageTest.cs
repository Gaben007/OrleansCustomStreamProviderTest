using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Streams;
using OrleansSimpleQueueCacheTest.QueueAdapter;
using System.Linq;
using FluentAssertions;
using Orleans;

namespace OrleansSimpleQueueCacheTest
{
    [TestClass]
    public class FailedToHandleMessageTest : TestBase
    {
        private bool _isFirst = true;
        private TestBatchContainer _sampleMessage = new TestBatchContainer(Guid.Parse("997dbf27-ccba-4cdf-8765-cb1936335085"), "FailedToHandleMessageTest", new object[] { "sample message" }.ToList(), 0);
        private List<IBatchContainer> _deliveredMessages = new List<IBatchContainer>();

        [TestMethod]
        public async Task MessageHandlerThrowsException()
        {
            for (int i = 0; i < 7; i++)
            {
                // wait for message delivery
                await Task.Delay(10000);

                _deliveredMessages.Should().HaveCount(0);
            }

            //_deliveredMessages.Should().HaveCount(1);
            //_deliveredMessages.First().StreamGuid.Should().Be(_sampleMessage.StreamGuid);
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

    [ImplicitStreamSubscription("FailedToHandleMessageTest")]
    public class FailedToHandleMessageTestGrain : Grain, IFailedToHandleMessageTestGrain, IAsyncObserver<string>
    {
        public override Task OnActivateAsync()
        {
            // TODO: subscribe

            return base.OnActivateAsync();
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(string item, StreamSequenceToken token = null)
        {
            throw new NotImplementedException();
        }
    }
}
