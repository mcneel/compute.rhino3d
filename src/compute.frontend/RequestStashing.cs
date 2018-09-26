using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Newtonsoft.Json.Linq;
using Amazon.S3;
using Serilog;


namespace compute.frontend
{
    public enum StashProviders
    {
        None = 0,
        TempFile = 1,
        AmazonS3 = 2
    }

    public static class RequestStashing
    {
        static bool _s3bucket_created = false;

        public static void AddRequestStashing(this IPipelines pipelines)
        {
            var stash_method = Env.GetEnvironmentString("COMPUTE_STASH_METHOD", "TEMPFILE");
            switch (stash_method)
            {
                case "TEMPFILE":
                {
                    pipelines.BeforeRequest += TempFileStasher;
                    Log.Information("Request stashing enabled via TempFileStasher");
                    break;
                }
                case "AMAZONS3":
                {
                    pipelines.BeforeRequest += AmazonS3RequestStasher;
                    Log.Information("Request stashing enabled via AmazonS3RequestStasher");
                    break;
                }
            }
        }
        public static Response AmazonS3RequestStasher(NancyContext context)
        {
            if (context.Request.Method != "POST")
                return null;

            object request_id_obj = null;
            if (!context.Items.TryGetValue("RequestId", out request_id_obj))
            {
                return null;
            }
            string requestId = request_id_obj as string;

            var bucket = Environment.GetEnvironmentVariable("COMPUTE_STASH_S3_BUCKET");
            if (string.IsNullOrWhiteSpace(bucket))
            {
                Log.ForContext("RequestId", requestId)
                    .Warning("COMPUTE_STASH_S3_BUCKET not set");
                return null;
            }

            var aws_key = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var aws_secret_key = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var aws_region_endpoint_env = Env.GetEnvironmentString("AWS_REGION_ENDPOINT", "us-east-1");
            var aws_region_endpoint = Amazon.RegionEndpoint.GetBySystemName(aws_region_endpoint_env);

            AmazonS3Client client = null;
            if (!string.IsNullOrWhiteSpace(aws_key) && !string.IsNullOrWhiteSpace(aws_secret_key))
                client = new AmazonS3Client(aws_key, aws_secret_key, aws_region_endpoint);
            else
                client = new AmazonS3Client(aws_region_endpoint);

            if (!_s3bucket_created)
            {
                var pbr = new Amazon.S3.Model.PutBucketRequest();
                pbr.BucketName = bucket;
                pbr.UseClientRegion = true;
                client.PutBucket(pbr);
                _s3bucket_created = true;
            }

            var por = new Amazon.S3.Model.PutObjectRequest();
            por.Key = requestId;
            por.BucketName = bucket;
            por.ContentBody = GetRequestJson(context);
            client.PutObjectAsync(por).ContinueWith(c => {
                Log.Information("Stashed request to {RequestId}", requestId);
            });

            return null;
        }

        public static Response TempFileStasher(NancyContext context)
        {
            if (context.Request.Method != "POST")
                return null;

            var stashDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Compute", "Requests");
            if (!System.IO.Directory.Exists(stashDir))
                System.IO.Directory.CreateDirectory(stashDir);

            string requestId = context.Items["RequestId"] as string;
            string filename = System.IO.Path.Combine(stashDir, $"{requestId}.request.log");

            System.IO.File.WriteAllText(filename, GetRequestJson(context));
            Log.Information("Stashed request to {RequestId}", requestId);

            return null;
        }

        static string GetRequestJson(NancyContext context)
        {
            var body = context.Request.Body.AsString();
            context.Items["request-body"] = body;

            var request = new JObject();
            request.Add("body", body);  // Do not assume that the body is valid, parsable JSON; save it as it arrives.
            request.Add("path", context.Request.Url.Path);
            request.Add("query", context.Request.Url.Query);
            object auth_user = null;
            if (context.Items.TryGetValue("auth_user", out auth_user))
                request.Add("auth_user", auth_user as string);
            var headers = new JObject();
            foreach (var header in context.Request.Headers)
            {
                var headerValues = new JObject();
                headers.Add(header.Key, JToken.FromObject(header.Value));
            }
            request.Add("headers", headers);
            return request.ToString();
        }
    }
}
