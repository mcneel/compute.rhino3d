using System;
using System.IO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Rhino.Collections;
using System.Runtime.Serialization;

namespace Rhino.Compute
{
    public static class ComputeServer
    {
        public static string WebAddress { get; set; } = "https://compute.rhino3d.com";
        public static string AuthToken { get; set; }
        public static string Version => "{{VERSION}}";

        public static T Post<T>(string function, params object[] postData)
        {
            return PostWithConverter<T>(function, null, postData);
        }

        public static T PostWithConverter<T>(string function, JsonConverter converter, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException("AuthToken must be set");

            for( int i=0; i<postData.Length; i++ )
            {
                if( postData[i]!=null &&
                    postData[i].GetType().IsGenericType &&
                    postData[i].GetType().GetGenericTypeDefinition() == typeof(Remote<>) )
                {
                    var mi = postData[i].GetType().GetMethod("JsonObject");
                    postData[i] = mi.Invoke(postData[i], null);
                }
            }

            string json = converter == null ?
                JsonConvert.SerializeObject(postData, Formatting.None) :
                JsonConvert.SerializeObject(postData, Formatting.None, converter);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.UserAgent = "compute.rhino3d.cs/" + Version;
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (converter == null)
                    return JsonConvert.DeserializeObject<T>(result);
                return JsonConvert.DeserializeObject<T>(result, converter);
            }
        }

        public static T0 Post<T0, T1>(string function, out T1 out1, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException("AuthToken must be set");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                return ja[0].ToObject<T0>();
            }
        }

        public static T0 Post<T0, T1, T2>(string function, out T1 out1, out T2 out2, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException("AuthToken must be set");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                out2 = ja[2].ToObject<T2>();
                return ja[0].ToObject<T0>();
            }
        }

        public static string ApiAddress(Type t, string function)
        {
            string s = t.ToString().Replace('.', '/');
            return s + "/" + function;
        }
    }

    public class Remote<T>
    {
        string _url;
        T _data;

        public Remote(string url)
        {
            _url = url;
        }

        public Remote(T data)
        {
            _data = data;
        }

        public object JsonObject()
        {
            if( _url!=null )
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["url"] = _url;
                return dict;
            }
            return _data;
        }
    }


    public static class PythonCompute
    {
        // NOTE: If you are using a Rhino 6 based version of Rhino3dmIO, you will
        // get compile errors due to ArchivableDictionary not implementing ISerializble in V6
        //
        // Either update to a V7 version of Rhino3dmIO (check the prerelease box on nuget)
        // -or-
        // Delete the entire PythonCompute class here. Python functionality is only available
        // using a V7 based Rhino3dmIO assembly
        class ArchivableDictionaryResolver : JsonConverter
        {
            public override bool CanConvert(Type objectType) { return objectType == typeof(ArchivableDictionary); }
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string encoded = (string)reader.Value;
                var dh = JsonConvert.DeserializeObject<DictHelper>(encoded);
                return dh.SerializedDictionary;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string json = JsonConvert.SerializeObject(new DictHelper((ArchivableDictionary)value));
                writer.WriteValue(json);
            }


            [Serializable]
            class DictHelper : ISerializable
            {
                public ArchivableDictionary SerializedDictionary { get; set; }
                public DictHelper(ArchivableDictionary d) { SerializedDictionary = d; }
                public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    SerializedDictionary.GetObjectData(info, context);
                }
                protected DictHelper(SerializationInfo info, StreamingContext context)
                {
                    Type t = typeof(ArchivableDictionary);
                    var constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                      null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
                    SerializedDictionary = constructor.Invoke(new object[] { info, context }) as ArchivableDictionary;
                }
            }
        }


        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return "rhino/python/" + caller;
        }

        public static ArchivableDictionary Evaluate(string script, ArchivableDictionary input)
        {
            return ComputeServer.PostWithConverter<ArchivableDictionary>(ApiAddress(), new ArchivableDictionaryResolver(), script, input);
        }
    }
