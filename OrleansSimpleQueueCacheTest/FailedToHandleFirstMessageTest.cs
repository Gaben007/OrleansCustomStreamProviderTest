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
     * One grain subscribes for "FailedToHandleFirstMessageTest" stream namespace and throws exception while try to processing first sample message.
     * Test sends two sample messages for grain.
     * 
     * Expected behavior:
     * Grain's OnNextAsync method should be retried for 1 min for the first sample message and after that OnErrorAsync should be called.
     * IQueueAdapterReceiver.MessagesDeliveredAsync shold be called with the second sample message.
     * 
     * Subscribed grain's implementation: FailedToHandleFirstMessageTestGrain.cs
     * 
     * Issue:
     * IQueueAdapterReceiver.MessagesDeliveredAsync is called .
     * 
     */
    [TestClass]
    public class FailedToHandleFirstMessageTest : TestBase
    {
        private bool _isFirst = true;
        private TestBatchContainer _sampleMessage1 = new TestBatchContainer(Guid.Parse("1cbf76d7-1d21-4bf3-8eac-09905ded5673"), "FailedToHandleFirstMessageTest", new object[] { "message_1" }.ToList(), 0);
        private TestBatchContainer _sampleMessage2 = new TestBatchContainer(Guid.Parse("81bdf90c-51f3-4711-b4f2-023b73ca1c71"), "FailedToHandleFirstMessageTest", new object[] { "message_2" }.ToList(), 1);
        private List<IBatchContainer> _deliveredMessages = new List<IBatchContainer>();
        private FailedToHandleMessageTestState _state = new FailedToHandleMessageTestState();

        [TestMethod]
        public async Task MessageHandlerThrowsException_FirstMessageShouldntBeDelivered_SecondShouldBe()
        {
            var startAt = DateTime.Now;

            // wait for retry
            while (!_state.FinishedRetryOnError.GetValueOrDefault(_sampleMessage1.StreamGuid) || !_state.FinishedRetryOnError.ContainsKey(_sampleMessage2.StreamGuid))
            {
                // timeout for test - built in retry logic should try until 1 min
                if (startAt.AddMinutes(2) < DateTime.Now)
                    throw new TimeoutException();

                // before the retry is finifhed for the message it shouldn't be marked as delivered
                await Task.Delay(300);
            }

            // wait for message delivering
            await Task.Delay(1000);

            // both message should be processed by grain
            _state.FinishedRetryOnError[_sampleMessage1.StreamGuid].Should().BeTrue();
            _state.FinishedRetryOnError[_sampleMessage2.StreamGuid].Should().BeFalse();

            // after the retry is finifhed for the first message it shouldn't be marked as delivered
            // and the second message should be
            _deliveredMessages.Should().HaveCount(1);
            _deliveredMessages.First().StreamGuid.Should().Be(_sampleMessage2.StreamGuid);
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
                return new IBatchContainer[] { _sampleMessage1, _sampleMessage2 };
            }

            return Enumerable.Empty<IBatchContainer>();
        }
    }
}
