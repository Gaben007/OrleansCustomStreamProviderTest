using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public interface IFailedToHandleSecondMessageTestGrain : IGrainWithGuidKey { }

    [ImplicitStreamSubscription("FailedToHandleSecondMessageTest")]
    public class FailedToHandleSecondMessageTestGrain : Grain, IFailedToHandleMessageTestGrain, IAsyncObserver<string>
    {
        private readonly FailedToHandleMessageTestState _state;

        public FailedToHandleSecondMessageTestGrain(FailedToHandleMessageTestState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task OnActivateAsync()
        {
            await this.GetStreamProvider("TestStreamProvider").GetStream<string>(this.GetPrimaryKey(), "FailedToHandleSecondMessageTest").SubscribeAsync(this);
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
            if (item.Equals("message_2", StringComparison.OrdinalIgnoreCase))
                throw new NotImplementedException();

            _state.FinishedRetryOnError.Add(this.GetPrimaryKey(), false);
            return Task.CompletedTask;
        }
    }
}
