using System;
using System.Collections.Generic;
using System.Linq;

namespace compute.frontend
{
    using System.Net;
    using System.Net.NetworkInformation;
    using Nancy;
    using Nancy.Bootstrapper;
    public static class RequestIdPipeline
    {
        private static string _hostname;

        public static void AddRequestId(this IPipelines pipelines)
        {
            pipelines.BeforeRequest += SetRequestId;
            pipelines.AfterRequest += SetHostName;
            pipelines.AfterRequest += AddCORSSupport;
        }

        private static Response SetRequestId(NancyContext context)
        {
            var request_id = context.Request.Headers["x-compute-id"].FirstOrDefault();
            var compute_host = context.Request.Headers["x-compute-host"].FirstOrDefault();

            if (string.IsNullOrEmpty(request_id))
                context.Items.Add("x-compute-id", Guid.NewGuid().ToString());
            else
                context.Items.Add("x-compute-id", request_id);

            if (string.IsNullOrEmpty(request_id))
                context.Items.Add("x-compute-host", GetFQDN());
            else
                context.Items.Add("x-compute-host", compute_host);

            context.Items.Add("x-start-ticks", DateTime.UtcNow.Ticks);
            return null;
        }

        private static void SetHostName(NancyContext context)
        {
            // TODO: The response ID should be set very early in the response handler and used in our internal logging.
            // Then, that ID should be returned here.
            context.Response.Headers.Add("x-compute-id", context.Items["x-compute-id"] as string);
            context.Response.Headers.Add("x-compute-host", context.Items["x-compute-host"] as string);
            var data = new Dictionary<string, string>();
            data.Add("statusCode", ((int)context.Response.StatusCode).ToString());

            var now = DateTime.UtcNow.Ticks;
            object start;
            if (context.Items.TryGetValue("x-start-ticks", out start))
            {
                var elapsed = (now - (Int64)start)/10000;
                data.Add("elapsedTime", elapsed.ToString());
            }

            Logger.Info(context, data);
        }

        private static void AddCORSSupport(NancyContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS,POST,GET");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
        }

        public static string GetFQDN()
        {
            if (!string.IsNullOrWhiteSpace(_hostname))
                return _hostname;

            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName += domainName;   // add the domain name part
            }

            _hostname = hostName;
            return _hostname;
        }
    }



}
