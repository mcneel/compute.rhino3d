using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class Schema
    {
        public Schema() {}

        public Schema Duplicate()
        {
            return (Schema)this.MemberwiseClone();
        }

        [JsonProperty(PropertyName = "absolutetolerance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AbsoluteTolerance { get; set; } = 0;

        [JsonProperty(PropertyName = "angletolerance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AngleTolerance { get; set; } = 0;

        [JsonProperty(PropertyName = "modelunits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ModelUnits { get; set; }

        // Rhino version of data to be serialized and returned to the client
        [JsonProperty(PropertyName = "dataversion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int DataVersion { get; set; } = 7;

        [JsonProperty(PropertyName = "algo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Algo { get; set; }

        [JsonProperty(PropertyName = "pointer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pointer { get; set; }

        // If true on input, the solve results are cached based on this schema.
        // When true the cache is searched for already computed results and used
        [JsonProperty(PropertyName = "cachesolve", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool CacheSolve { get; set; } = false;

        // Used for nested calls
        [JsonProperty(PropertyName = "recursionlevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int RecursionLevel { get; set; } = 0;

        [JsonProperty(PropertyName = "values", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<DataTree<ResthopperObject>> Values { get; set; } = new List<DataTree<ResthopperObject>>();

        // Return warnings from GH
        [JsonProperty(PropertyName = "warnings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Warnings { get; set; }

        // Return errors from GH
        [JsonProperty(PropertyName = "errors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Errors { get; set; }
    }

    public class IoQuerySchema
    {
        [JsonProperty(PropertyName = "requestedFile")]
        public string RequestedFile { get; set; }

    }

    public class IoParamSchema
    {
        [JsonProperty(PropertyName = "Name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Nickname", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "ParamType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ParamType { get; set; }
    }

    public class InputParamSchema : IoParamSchema
    {
        [JsonProperty(PropertyName = "Description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "AtLeast")]
        public int AtLeast { get; set; } = 1;

        [JsonProperty(PropertyName = "AtMost", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AtMost { get; set; } = int.MaxValue;

        [JsonProperty(PropertyName = "TreeAccess", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool TreeAccess { get; set; } = false;

        [JsonProperty(PropertyName = "Default", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Default { get; set; } = null;

        [JsonProperty(PropertyName = "Minimum", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Minimum { get; set; } = null;

        [JsonProperty(PropertyName = "Maximum", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Maximum { get; set; } = null;
    }

    public class IoResponseSchema
    {
        [JsonProperty(PropertyName = "Description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "CacheKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CacheKey { get; set; }

        [JsonProperty(PropertyName = "InputNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> InputNames { get; set; }

        [JsonProperty(PropertyName = "OutputNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> OutputNames { get; set; }

        [JsonProperty(PropertyName = "Icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "Inputs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<InputParamSchema> Inputs { get; set; }

        [JsonProperty(PropertyName = "Outputs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<IoParamSchema> Outputs { get; set; }

        [JsonProperty(PropertyName = "Warnings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Warnings { get; set; }

        [JsonProperty(PropertyName = "Errors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Errors { get; set; } 
    }

    public class HTTPArchive
    {
        [JsonConstructor]
        public HTTPArchive()
        {
            io = new IO();
            solve = new Solve();
        }
        public IO io { get; set; }
        public Solve solve { get; set; }
    }

    public class RequestHeaders
    {
        [JsonConstructor]
        public RequestHeaders() { }

        [JsonProperty(PropertyName = "Date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "User-Agent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserAgent { get; set; }

        [JsonProperty(PropertyName = "Accept", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Accept { get; set; }

        [JsonProperty(PropertyName = "RhinoComputeKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Authorization { get; set; }

        [JsonProperty(PropertyName = "Cache-Control", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CacheControl { get; set; }

        [JsonProperty(PropertyName = "Pragma", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pragma { get; set; }
    }

    public class ResponseHeaders
    {
        [JsonConstructor]
        public ResponseHeaders() { }

        [JsonProperty(PropertyName = "Date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "Content-Type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = "Connection", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Connection { get; set; }

        [JsonProperty(PropertyName = "Server", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Server { get; set; }

        [JsonProperty(PropertyName = "Age", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Age { get; set; }

        [JsonProperty(PropertyName = "Expires", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Expires { get; set; }

        [JsonProperty(PropertyName = "Cache-Control", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CacheControl { get; set; }

        [JsonProperty(PropertyName = "Pragma", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pragma { get; set; }

        [JsonProperty(PropertyName = "Location", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Location { get; set; }
    }

    public class Request
    {
        [JsonConstructor]
        public Request() 
        {
            Headers = new RequestHeaders();
        }
        [JsonProperty(PropertyName = "Request URL")]
        public string URL { get; set; }

        [JsonProperty(PropertyName = "Request Method")]
        public string RequestMethod { get; set; }

        [JsonProperty(PropertyName = "Headers")]
        public RequestHeaders Headers { get; set; }

        [JsonProperty(PropertyName = "Content")]
        public Schema Content { get; set;}
    }

    public class Response
    {
        [JsonConstructor]
        public Response()
        {
            Headers = new ResponseHeaders();
        }

        [JsonProperty(PropertyName = "Status Code")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "Headers")]
        public ResponseHeaders Headers { get; set; }
    }

    public class IOResponse : Response
    {
        [JsonProperty(PropertyName = "Content", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IoResponseSchema Content { get; set; }
    }

    public class SolveResponse : Response
    {
        [JsonProperty(PropertyName = "Content", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Schema Content { get; set; }
    }

    public class IO
    {
        [JsonConstructor]
        public IO() 
        {
            Request = new Request();
            Response = new IOResponse();
        }

        public Request Request { get; set; }
        public IOResponse Response { get; set; }
    }
    public class Solve
    {
        [JsonConstructor]
        public Solve() 
        {
            Request = new Request();
            Response = new SolveResponse();
        }

        public Request Request { get; set; }
        public SolveResponse Response { get; set; }
    }

    public class ResthopperObject : IEquatable<ResthopperObject>
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonIgnore]
        public object ResolvedData { get; set; }

        [JsonConstructor]
        public ResthopperObject()
        {
        }

        public ResthopperObject(object obj)
        {
#if COMPUTE_CORE
            Data = JsonConvert.SerializeObject(obj, compute.geometry.GeometryResolver.Settings);
#else
            Data = JsonConvert.SerializeObject(obj);//, compute.geometry.GeometryResolver.Settings);
#endif
            Type = obj.GetType().FullName;
        }

        public bool Equals(ResthopperObject other)
        {
            return string.Equals(Type, other.Type) && string.Equals(Data, other.Data);
        }
    }
}
