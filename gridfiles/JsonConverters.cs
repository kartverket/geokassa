using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace gridfiles
{
    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TypeEnumConverter.Singleton,
                FileTypeConverter.Singleton,
                FormatVersionConverter.Singleton,
                TransformedComponentConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal static class CommonPointListConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ArrayConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
   
    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "bbox")
            {
                return TypeEnum.Bbox;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            if (value == TypeEnum.Bbox)
            {
                serializer.Serialize(writer, "bbox");
                return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }

    internal class FileTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(FileType) || t == typeof(FileType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "triangulation_file")
            {
                return FileType.TriangulationFile;
            }
            throw new Exception("Cannot unmarshal type FileType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (FileType)untypedValue;
            if (value == FileType.TriangulationFile)
            {
                serializer.Serialize(writer, "triangulation_file");
                return;
            }
            throw new Exception("Cannot marshal type FileType");
        }

        public static readonly FileTypeConverter Singleton = new FileTypeConverter();
    }

    internal class FormatVersionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(FormatVersion) || t == typeof(FormatVersion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "1.0")
            {
                return FormatVersion.The10;
            }
            throw new Exception("Cannot unmarshal type FormatVersion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (FormatVersion)untypedValue;
            if (value == FormatVersion.The10)
            {
                serializer.Serialize(writer, "1.0");
                return;
            }
            throw new Exception("Cannot marshal type FormatVersion");
        }

        public static readonly FormatVersionConverter Singleton = new FormatVersionConverter();
    }

    internal class TransformedComponentConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TransformedComponent) || t == typeof(TransformedComponent?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "horizontal":
                    return TransformedComponent.horizontal;
                case "vertical":
                    return TransformedComponent.vertical;
            }
            throw new Exception("Cannot unmarshal type TransformedComponent");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TransformedComponent)untypedValue;
            switch (value)
            {
                case TransformedComponent.horizontal:
                    serializer.Serialize(writer, "horizontal");
                    return;
                case TransformedComponent.vertical:
                    serializer.Serialize(writer, "vertical");
                    return;
            }
            throw new Exception("Cannot marshal type TransformedComponent");
        }

        public static readonly TransformedComponentConverter Singleton = new TransformedComponentConverter();
    }

    internal class ESDateTimeConverter : IsoDateTimeConverter
    {
        public ESDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        }
    }

    internal class ArrayConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var list = Activator.CreateInstance(objectType) as System.Collections.IList;
            var itemType = objectType.GenericTypeArguments[0];

            if (token.Type.ToString() == "Object")
            {
                var child = token.Children();
                var newObject = Activator.CreateInstance(itemType);
                serializer.Populate(token.CreateReader(), newObject);

                list.Add(newObject);
            }
            else
            {
                foreach (var child in token.Children())
                {
                    var newObject = Activator.CreateInstance(itemType);
                    serializer.Populate(child.CreateReader(), newObject);

                    list.Add(newObject);
                }
            }
            return list;
        }

        // Alternative ReadJson():
        /*
         public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
        {
        var response = new List<Player>();
         
        JObject players = JObject.Load(reader);
        
        foreach (var player in players)
        {
        var p = JsonConvert.DeserializeObject<Player>(player.Value.ToString());
           
        p.Id = player.Key;
        response.Add(p);
        }

        return response;
        }
        */

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(List<>));
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }          
 
            JToken token = JToken.FromObject(value);
            token = token.First;

            writer.WriteStartArray();

            while (!(token is null))
            {
                writer.WriteRawValue(JsonConvert.SerializeObject(token));
                token = token.Next;
            }
            writer.WriteEndArray();
        }

        public static readonly ArrayConverter Singleton = new ArrayConverter();
    }
}
