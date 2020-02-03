using System;
using System.Collections.Generic;

namespace Grains
{
    public class FailedToHandleMessageTestState
    {
        public Dictionary<Guid, bool> FinishedRetryOnError { get; set; } = new Dictionary<Guid, bool>();
    }
}
