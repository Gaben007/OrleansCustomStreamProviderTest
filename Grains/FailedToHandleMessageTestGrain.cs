using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public interface IFailedToHandleMessageTestGrain : IGrainWithGuidKey { }

    [ImplicitStreamSubscription("FailedToHandleMessageTest")]
    public class FailedToHandleMessageTestGrain : Grain, IFailedToHandleMessageTestGrain, IAsyncObserver<string>
    {
        private readonly FailedToHandleMessageTestState _state;

        public FailedToHandleMessageTestGrain(FailedToHandleMessageTestState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task OnActivateAsync()
        {
            await this.GetStreamProvider("TestStreamProvider").GetStream<string>(this.GetPrimaryKey(), "FailedToHandleMessageTest").SubscribeAsync(this);
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
