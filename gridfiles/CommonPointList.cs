using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;

namespace gridfiles
{ 
    [Description("Schema for geocentric common point list")]
    [JsonObject("common_point_list", ItemIsReference = true, Title = "Common point list", IsReference = true)]
    public class CommonPointList : JsonObject
    {
        private List<CommonPointXYZ> _list = null;
        private string _sourceCrs = "";
        private string _targetCrs = "";
        private string _fileName = "";  
       
        [JsonConstructor()]
        public CommonPointList()
        {   
        }

        [Description("File name of json file.")]
        [JsonProperty("file_name", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]
        public string FileName
        { 
            get => _fileName;
            set => _fileName = value;
        }

        [Description("List of common point in source and target crs.")]
        [JsonProperty("point_list", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]
        [JsonConverter(typeof(ArrayConverter))]
        public List<CommonPointXYZ> PointList
        {
            get => _list = _list ?? new List<CommonPointXYZ>();
            set => _list = value;
        }

        [Description("String identifying the CRS of source coordinates in the vertices. Typically \"EPSG:XXXX\"")]
        [JsonProperty("source_crs", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]
        public string SourceCrs
        {
            get => _sourceCrs;
            set => _sourceCrs = value;
        }

        [Description("String identifying the CRS of target coordinates in the vertices. Typically \"EPSG:XXXX\"")]
        [JsonProperty("target_crs", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]        
        public string TargetCrs
        {
            get => _targetCrs;
            set => _targetCrs = value;
        }

        [JsonIgnore]
        public override JSchema JSchema
        {
            get
            {   
                var generator = new JSchemaGenerator();

                generator.GenerationProviders.Add(new StringEnumGenerationProvider());
                
                var schema = generator.Generate(typeof(CommonPointList));

                return schema;
            }
        }

        [JsonIgnore]
        public override string JSchemaString
        {
            get
            {
                var generator = new JSchemaGenerator();

                generator.GenerationProviders.Add(new StringEnumGenerationProvider());

                var schema = generator.Generate(typeof(CommonPointList));

                return schema.ToString();
            }
        }

        public override bool ValidateJson(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);

            string jsonSchemaString = JSchemaString;
            var schema = JSchema.Parse(jsonSchemaString);

            IList<string> message;
            bool valid = jsonObject.IsValid(schema, out message);

            if (!valid)
                return false;

            return true;
        }
    }

    public static class JsonGenerator
    {
        public static bool GenerateSchema<T>(string fileName, string uriId = null, string schemaVersion = null)
        {
            var generator = new JSchemaGenerator();
            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            
            var schema = generator.Generate(typeof(T));
            schema.Type = JSchemaType.Object;
            schema.Title = typeof(T).Name;
            
            if (uriId != null && !Uri.IsWellFormedUriString(uriId, UriKind.Absolute))
                return false;

            if (schemaVersion != null && !Uri.IsWellFormedUriString(schemaVersion, UriKind.Absolute))
                return false;

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                return false;

            schema.Id = uriId != null ? new Uri(uriId) : schema.Id;
            schema.SchemaVersion = schemaVersion != null ? new Uri(schemaVersion) : schema.SchemaVersion;

            using (StreamWriter file = File.CreateText(fileName))
            {
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    schema.WriteTo(writer);
                    writer.Close();
                }
                file.Close();
            }
            return true;
        }
         
        public static void WriteToJsonFile(object obj, string fileName)
        {   
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, CommonPointListConverter.Settings);

            File.WriteAllText(fileName, jsonString);
        }

        public static object ReadFromJsonFile<T>(object obj, string fileName) where T : JsonObject
        {
            try
            {
                var jsonString = File.ReadAllText(fileName);
                JObject jsonObject = JObject.Parse(jsonString);

                if (obj is JsonObject)
                {
                    var objAsJsonObject = obj as JsonObject;
                    if (!objAsJsonObject.ValidateJson(jsonString))
                        return obj;

                    JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
                    JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader);
                    validatingReader.Schema = JSchema.Parse(objAsJsonObject.JSchemaString);

                    IList<string> messages = new List<string>();
                    validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

                    JsonSerializer serializer = new JsonSerializer();
                    T p = serializer.Deserialize<T>(validatingReader);

                    return p;
                }
                else
                {
                    return obj;
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
            /*
            string jsonSchemaString = @CommonPointList.JSchema.ToString();
            var schema = JSchema.Parse(jsonSchemaString);
            
            IList<string> message;
            bool valid = jsonObject.IsValid(schema, out message);

            if (!valid)
                return false;
            
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
            JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader);
            validatingReader.Schema = JSchema.Parse(CommonPointList.JSchema.ToString());

            IList<string> messages = new List<string>();
            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

            JsonSerializer serializer = new JsonSerializer();
            T p = serializer.Deserialize<T>(validatingReader);
            */             
        }
    }
     
    public partial class T
    {
        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(json, Converter.Settings);
    }

    public static class SerializeJson<T>
    {
        public static string ToJson(T self) => JsonConvert.SerializeObject(self, Converter.Settings);        
    }
}
