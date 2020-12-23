using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(compute.geometry.Startup))]

namespace compute.geometry
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.Use<LoggingMiddleware>();
            var options = new Nancy.Owin.NancyOptions { Bootstrapper = new Bootstrapper() };
            app.UseNancy(options);
        }
    }

    /// <summary>
    /// Custom request logging for debugging.
    /// </summary>
    internal class LoggingMiddleware : OwinMiddleware
    {
        public LoggingMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext ctx)
        {
            IOwinRequest req = ctx.Request;

            // invoke the next middleware in the pipeline
            await Next.Invoke(ctx);

            IOwinResponse res = ctx.Response;
            string contentLength = res.ContentLength > -1 ? res.ContentLength.ToString() : "-";

            // log request in apache format
            string msg = $"{req.RemoteIpAddress} - [{DateTime.Now:o}] \"{req.Method} {req.Uri.AbsolutePath} {req.Protocol}\" {res.StatusCode} {contentLength}";
            Console.WriteLine(msg);
        }
    }
}
