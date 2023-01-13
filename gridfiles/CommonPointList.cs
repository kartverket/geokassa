using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
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
    public class CommonPointList
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
    }

    public static class JsonGenerator
    {
        public static void GenerateSchema<T>(string fileName, string uriId = null, string schemaVersion = null)
        {
            var generator = new JSchemaGenerator();

            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            var schema = generator.Generate(typeof(T));

            schema.Type = JSchemaType.Object;
            schema.Title = typeof(T).Name;
            
            if (uriId != null && !Uri.IsWellFormedUriString(uriId, UriKind.Absolute))
                return;

            if (schemaVersion != null && !Uri.IsWellFormedUriString(schemaVersion, UriKind.Absolute))
                return;

            /*
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string fullPath = Assembly.GetAssembly(typeof(T)).Location;
            string filePath2 = new Uri(Assembly.GetAssembly(typeof(T)).CodeBase).LocalPath;
            var dsds = AppDomain.CurrentDomain.BaseDirectory;
            var wewwe = AppContext.BaseDirectory;
            var ewew = System.IO.Directory.GetCurrentDirectory();
            */

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                return;

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
        }
    }
}
