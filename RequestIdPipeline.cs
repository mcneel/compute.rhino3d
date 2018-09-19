using System;

namespace RhinoCommon.Rest
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
            pipelines.AfterRequest += SetHostName;
            pipelines.AfterRequest += AddCORSSupport;
        }

        private static Response SetRequestId(NancyContext context)
        {
            return null;
        }

        private static void SetHostName(NancyContext context)
        {
            // TODO: The response ID should be set very early in the response handler and used in our internal logging.
            // Then, that ID should be returned here.
            context.Response.Headers.Add("x-compute-id", Guid.NewGuid().ToString());
            context.Response.Headers.Add("x-compute-host", GetFQDN());
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
