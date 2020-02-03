using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Streams;
using OrleansSimpleQueueCacheTest.QueueAdapter;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Grains;

namespace OrleansSimpleQueueCacheTest
{
    /*
     * Test case:
     * One grain subscribes for "FailedToHandleMessageTest" stream namespace and throws exception while try to processing message.
     * Test sends one sample message for grain.
     * 
     * Expected behavior:
     * Grain's OnNextAsync method should be retried for 1 min and after that OnErrorAsync should be called.
     * IQueueAdapterReceiver.MessagesDeliveredAsync sholdn't be called with the sample message.
     * 
     * Subscribed grain's implementation: FailedToHandleMessageTestGrain.cs
     * 
     * Issue:
     * IQueueAdapterReceiver.MessagesDeliveredAsync is immediately called after the sample message retrived from the IQueueAdapterReceiver.
     * 
     */
    [TestClass]
    public class FailedToHandleMessageTest : TestBase
    {
        private bool _isFirst = true;
        private TestBatchContainer _sampleMessage = new TestBatchContainer(Guid.Parse("997dbf27-ccba-4cdf-8765-cb1936335085"), "FailedToHandleMessageTest", new object[] { "sample message" }.ToList(), 0);
        private List<IBatchContainer> _deliveredMessages = new List<IBatchContainer>();
        private FailedToHandleMessageTestState _state = new FailedToHandleMessageTestState();

        [TestMethod]
        public async Task MessageHandlerThrowsException_MessageShouldntBeDelivered()
        {
            var startAt = DateTime.Now;

            // wait for retry
            while (!_state.FinishedRetryOnError.GetValueOrDefault(_sampleMessage.StreamGuid))
            {
                // timeout for test - built in retry logic should try until 1 min
                if (startAt.AddMinutes(2) < DateTime.Now)
                    throw new TimeoutException();

                // before the retry is finifhed for the message it shouldn't be marked as delivered
                _deliveredMessages.Should().HaveCount(0);
                await Task.Delay(300);
            }

            // wait for message delivering
            await Task.Delay(1000);

            // after the retry is finifhed for the message it shouldn't be marked as delivered
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
}
