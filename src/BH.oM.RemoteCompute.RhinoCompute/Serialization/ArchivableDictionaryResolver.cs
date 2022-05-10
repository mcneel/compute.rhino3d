using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public class ArchivableDictionaryResolver : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Rhino.Collections.ArchivableDictionary);
        }

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
            string json = JsonConvert.SerializeObject(new DictHelper((Rhino.Collections.ArchivableDictionary)value));
            writer.WriteValue(json);
        }


        [Serializable]
        class DictHelper : ISerializable
        {
            public Rhino.Collections.ArchivableDictionary SerializedDictionary { get; set; }
            public DictHelper(Rhino.Collections.ArchivableDictionary d) { SerializedDictionary = d; }
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                SerializedDictionary.GetObjectData(info, context);
            }
            protected DictHelper(SerializationInfo info, StreamingContext context)
            {
                Type t = typeof(Rhino.Collections.ArchivableDictionary);
                var constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                  null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
                SerializedDictionary = constructor.Invoke(new object[] { info, context }) as Rhino.Collections.ArchivableDictionary;
            }
        }
    }


}
