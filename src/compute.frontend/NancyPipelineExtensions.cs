using System;
using System.Linq;
using Serilog;
using System.Net;
using System.Net.NetworkInformation;
using Nancy;
using Nancy.Bootstrapper;
using Serilog.Core;
using Serilog.Events;
using System.IO;
using Nancy.Responses.Negotiation;
using Nancy.ErrorHandling;

namespace compute.frontend
{
    public static class NancyPipelineExtensions
    {
        private static string _hostname;

        public static void AddHeadersAndLogging(this IPipelines pipelines)
        {
            pipelines.BeforeRequest += LogRequest;
            pipelines.AfterRequest += AddResponseHeaders;
            pipelines.AfterRequest += AddCORSSupport;
            pipelines.AfterRequest += LogResponse;
            pipelines.OnError += (ctx, ex) =>
            {
                Log.Error(ex, "An exception occured while processing request \"{RequestId}\"", ctx.Items["RequestId"] as string);
                // cors handled in default status code handler
                return null;
            };
        }

        private static Response LogRequest(NancyContext context)
        {
            // TODO: get request id from X-Amzn-Trace-Id
            context.Items.Add("RequestId", Guid.NewGuid().ToString());
            context.Items.Add("Hostname", GetMachineId());
            context.Items.Add("StartTicks", DateTime.UtcNow.Ticks);

            if (context.Request.Path == "/healthcheck")
                return null;

            Log.ForContext<Nancy.Request>()
                .ForContext(new RequestLogEnricher(context))
                .Information("\"{Method} {Path}\" - \"{Agent}\"", context.Request.Method, context.Request.Path,
                    context.Request.Headers.UserAgent);
            return null;
        }

        private static void AddResponseHeaders(NancyContext context)
        {
            if (context.Items.TryGetValue("RequestId", out var requestId))
                context.Response.Headers.Add("X-Compute-Id", requestId as string);
            if (context.Items.TryGetValue("RequestId", out var hostname))
                context.Response.Headers.Add("X-Compute-Host", hostname as string);
        }

        internal static void AddCORSSupport(NancyContext context)
        {
            if (context.Request.Headers.Keys.Contains("Origin"))
            {
                context.Response
                    .WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "OPTIONS,POST,GET,HEAD")
                    .WithHeader("Access-Control-Allow-Headers", "Authorization,Origin,Accept,Content-Type,User-Agent," +
                                "Access-Control-Allow-Headers,Access-Control-Request-Method,Access-Control-Request-Headers,api_token");
            }
        }

        private static void LogResponse(NancyContext context)
        {
            if (context.Request.Path == "/healthcheck" && context.Response.StatusCode == Nancy.HttpStatusCode.OK)
                return;

            Log.ForContext<Nancy.Response>()
                .ForContext(new RequestLogEnricher(context))
                .Information("\"{Method} {Path}\" {StatusCode} \"{Agent}\"", context.Request.Method,
                    context.Request.Path, (int)context.Response.StatusCode, context.Request.Headers.UserAgent);
        }



        public static string GetMachineId()
        {
            // if we already have a machine identifier, return it
            if (!string.IsNullOrWhiteSpace(_hostname))
                return _hostname;

            // if we're running on EC2, use the instance id
            try
            {
               _hostname = Amazon.Util.EC2InstanceMetadata.InstanceId;
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(_hostname))
                return _hostname;

            // fallback to the "fully qualified domain name"
            _hostname = GetFQDN();

            return _hostname;
        }

        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            if (string.IsNullOrWhiteSpace(domainName)) // exit early if no domain name
                return hostName;

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName)) // if hostname does not already include domain name
            {
                hostName += domainName; // add the domain name part
            }

            return hostName;
        }
    }

    public class RequestLogEnricher : ILogEventEnricher
    {
        private NancyContext _context;

        public RequestLogEnricher(NancyContext ctx)
        {
            this._context = ctx;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var ctx = _context;

            if (ctx.Items.TryGetValue("RequestId", out var requestId))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", requestId as string));
            if (ctx.Items.TryGetValue("Hostname", out var hostname))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Hostname", hostname as string));

            object auth_user;
            if (ctx.Items.TryGetValue("auth_user", out auth_user))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "UserId", auth_user as string));
            }

            if (ctx.Response != null)
            {
                object start;
                if (ctx.Response != null && ctx.Items.TryGetValue("StartTicks", out start))
                {
                    var elapsed = (DateTime.UtcNow.Ticks - (long)start) / TimeSpan.TicksPerMillisecond;
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "ElapsedTime", elapsed.ToString()));

                    using (var stream = new MemoryStream())
                    {
                        ctx.Response.Contents.Invoke(stream);
                        try
                        {
                            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                                "ResponseContentLength", stream.Length));
                        }
                        catch (ObjectDisposedException)
                        {
                            // skip logging content length if stream is prematurely closed
                            // seems to happen a lot when debugging
                        }
                    }

                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ResponseContentType", ctx.Response.ContentType));
                }
            }
            else
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "UserHostAddress", GetUserIpAddress()));

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "RequestContentLength", ctx.Request.Headers.ContentLength));

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "RequestContentType", ctx.Request.Headers.ContentType));

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "Host", ctx.Request.Headers.Host));
            }
        }

        string GetUserIpAddress()
        {
            // assume compute is behind a load balancer
            var ip = _context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            // if not, get the user's ip directly
            if (string.IsNullOrEmpty(ip))
            {
                ip = _context.Request.UserHostAddress;
            }

            return ip;
        }
    }

    /// <summary>
    /// Extends the default status code handler (404 and 500) with CORS headers.
    /// See https://git.io/JfyRA
    /// </summary>
    public class DefaultStatusCodeHandler : Nancy.ErrorHandling.DefaultStatusCodeHandler, IStatusCodeHandler
    {
        public DefaultStatusCodeHandler(IResponseNegotiator responseNegotiator) : base(responseNegotiator) { }

        public new void Handle(Nancy.HttpStatusCode statusCode, NancyContext context)
        {
            base.Handle(statusCode, context);
            NancyPipelineExtensions.AddCORSSupport(context);
        }
    }
}
