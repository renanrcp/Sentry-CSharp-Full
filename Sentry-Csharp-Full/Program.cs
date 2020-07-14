using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Extensions.Logging;

namespace Sentry_Csharp_Full
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSentry(options =>
                    {
                        options.Dsn = "YOUR_DSN_HERE";
                        options.AddLogEntryFilter((category, level, eventId, exception)
                            => eventId.Id == 10000
                            && string.Equals(eventId.Name, "Microsoft.EntityFrameworkCore.Update.SaveChangesFailed", StringComparison.Ordinal)
                            && string.Equals(category, "Microsoft.EntityFrameworkCore.Database.Command", StringComparison.Ordinal));

                        // options.AddLogEntryFilter((category, level, eventId, exception)
                        //     => eventId.Id == 20102
                        //     && string.Equals(eventId.Name, "Microsoft.EntityFrameworkCore.Database.Command.CommandError", StringComparison.Ordinal)
                        //     && string.Equals(category, "Microsoft.EntityFrameworkCore.Database.Command", StringComparison.Ordinal));
                        options.BeforeSend = HandleBeforeEvent;
                    });
                });

        public static SentryEvent HandleBeforeEvent(SentryEvent sentryEvent)
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
