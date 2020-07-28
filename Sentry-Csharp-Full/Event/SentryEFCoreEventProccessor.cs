using System;
using Sentry;
using Sentry.Extensibility;

namespace Sentry_Csharp_Full.Event
{
    public class SentryEFCoreEventProccessor : ISentryEventProcessor
    {
        public SentryEvent Process(SentryEvent sentryEvent)
        {
            if (sentryEvent.LogEntry != null)
            {
                if (sentryEvent.Logger.Equals("Microsoft.EntityFrameworkCore.Database.Command", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
            }

            return sentryEvent;
        }
    }
}