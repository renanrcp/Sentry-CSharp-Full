using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Extensibility;
using Sentry_Csharp_Full.Event;

namespace Sentry_Csharp_Full
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISentryEventProcessor, SentryEFCoreEventProccessor>();

            services.AddDbContext<SampleContext>((sp, options) =>
            {
                options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                options.UseApplicationServiceProvider(sp);
                options.UseNpgsql("YOUR_CONNECTION_STRING_HERE");
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            app.Use(async (context, next) =>
            {
                var log = context.RequestServices.GetService<ILoggerFactory>()
                    .CreateLogger<Startup>();

                using var dbContext = context.RequestServices.GetService<SampleContext>();

                log.LogInformation("Handling some request...");

                if (context.Request.Path == "/throw")
                {
                    var hub = context.RequestServices.GetService<IHub>();
                    hub.ConfigureScope(s =>
                    {
                        // More data can be added to the scope like this:
                        s.SetTag("Sample", "ASP.NET Core"); // indexed by Sentry
                        s.SetExtra("Extra!", "Some extra information");
                    });

                    log.LogInformation("Logging info...");
                    log.LogWarning("Logging some warning!");

                    //log.LogError("ops an error");

                    // The following exception will be captured by the SDK and the event
                    // will include the Log messages and any custom scope modifications
                    // as exemplified above.
                    throw new Exception("An exception thrown from the ASP.NET Core pipeline");
                }
                else if (context.Request.Path == "/efcore")
                {
                    await dbContext.Users.FirstAsync();
                }
                else if (context.Request.Path == "/efcore-insert")
                {
                    var user = new User();

                    await dbContext.Users.AddAsync(user);

                    await dbContext.SaveChangesAsync();
                }

                await next();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
