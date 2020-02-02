using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans.Streams;

namespace OrleansSimpleQueueCacheTest
{
    [TestClass]
    public class FailedMessageHandlingTest : TestBase
    {
        [TestMethod]
        public async Task MessageHandlerThrowsException()
        {
            await Task.Delay(100);
        }

        protected override void OnMessagesDelivered(IEnumerable<IBatchContainer> messages)
        {
            
        }

        protected override IEnumerable<IBatchContainer> ProvideMessages()
        {
            return new List<IBatchContainer>();
        }
    }
}
