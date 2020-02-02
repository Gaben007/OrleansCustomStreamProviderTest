using Orleans.Providers.Streams.Common;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrleansSimpleQueueCacheTest.QueueAdapter
{
    internal class TestBatchContainer : IBatchContainer
    {
        private readonly List<object> _events;
        private readonly EventSequenceToken _eventSequenceToken;

        public Guid StreamGuid { get; }

        public string StreamNamespace { get; }

        public StreamSequenceToken SequenceToken => _eventSequenceToken;


        public TestBatchContainer(Guid streamGuid, string streamNamespace, List<object> events, long sequenceId)
        {
            StreamGuid = streamGuid;
            StreamNamespace = streamNamespace;
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _eventSequenceToken = new EventSequenceToken(sequenceId);
        }

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return _events
                .OfType<T>()
                .Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, _eventSequenceToken?.CreateSequenceTokenForEvent(i)))
                .ToList();
        }

        public bool ImportRequestContext()
        {
            return false;
        }

        public bool ShouldDeliver(IStreamIdentity stream, object filterData, StreamFilterPredicate shouldReceiveFunc)
        {
            return true;
        }
    }
}
