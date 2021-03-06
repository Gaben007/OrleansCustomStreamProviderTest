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
     * One grain subscribes for "FailedToHandleSecondMessageTest" stream namespace and throws exception while try to processing second sample message.
     * Test sends two sample messages for grain.
     * 
     * Expected behavior:
     * Grain's OnNextAsync method should be retried for 1 min for the second sample message and after that OnErrorAsync should be called.
     * IQueueAdapterReceiver.MessagesDeliveredAsync shold be called with the first sample message.
     * 
     * Subscribed grain's implementation: FailedToHandleSecondMessageTestGrain.cs
     * 
     * Issue:
     * IQueueAdapterReceiver.MessagesDeliveredAsync is immediately called with both messages.
     * 
     */
    [TestClass]
    public class FailedToHandleSecondMessageTest : TestBase
    {
        private bool _isSecond = true;
        private TestBatchContainer _sampleMessage1 = new TestBatchContainer(Guid.Parse("58ffa8c7-bca7-4038-b4a4-48c36a250043"), "FailedToHandleSecondMessageTest", new object[] { "message_1" }.ToList(), 0);
        private TestBatchContainer _sampleMessage2 = new TestBatchContainer(Guid.Parse("57596046-c36f-422a-bf52-03dce83cd63a"), "FailedToHandleSecondMessageTest", new object[] { "message_2" }.ToList(), 1);
        private List<IBatchContainer> _deliveredMessages = new List<IBatchContainer>();
        private FailedToHandleMessageTestState _state = new FailedToHandleMessageTestState();

        [TestMethod]
        public async Task MessageHandlerThrowsException_SecondMessageShouldntBeDelivered_FirstShouldBe()
        {
            var startAt = DateTime.Now;

            // wait for retry
            while (!_state.FinishedRetryOnError.GetValueOrDefault(_sampleMessage2.StreamGuid) || !_state.FinishedRetryOnError.ContainsKey(_sampleMessage1.StreamGuid))
            {
                // timeout for test - built in retry logic should try until 1 min
                if (startAt.AddMinutes(2) < DateTime.Now)
                    throw new TimeoutException();

                // second message shouldn't be delivered before ending the retry
                _deliveredMessages.Should().HaveCountLessThan(2);

                // before the retry is finifhed for the message it shouldn't be marked as delivered
                await Task.Delay(300);
            }

            // wait for message delivering
            await Task.Delay(1000);

            // both message should be processed by grain
            _state.FinishedRetryOnError[_sampleMessage1.StreamGuid].Should().BeFalse();
            _state.FinishedRetryOnError[_sampleMessage2.StreamGuid].Should().BeTrue();

            // after the retry is finifhed for the second message it shouldn't be marked as delivered
            // and the second message should be
            _deliveredMessages.Should().HaveCount(1);
            _deliveredMessages.First().StreamGuid.Should().Be(_sampleMessage1.StreamGuid);
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
            if (_isSecond)
            {
                _isSecond = false;
                return new IBatchContainer[] { _sampleMessage1, _sampleMessage2 };
            }

            return Enumerable.Empty<IBatchContainer>();
        }
    }
}
