using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelaunatorSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace gridfiles
{
    public class TinModel
    {
        private CptFile _cptFile = new CptFile();

        public TinModel()
        { }

        public TinModel(Coordinate coord)
        {
            Coord = coord;
        }

        public string OutputFileName { get; set; } = "";

        public CrsCode EpsgSource { get; set; } = new CrsCode();

        public CrsCode EpsgTarget { get; set; } = new CrsCode();

        public CptFile CptFile
        {
            get => _cptFile;
            set => _cptFile = value;
        }

        public Coordinate Coord { get; set; } = 
            new Coordinate()
            {
                TransformedComponents = new List<TransformedComponent>(),
                TrianglesColumns = new List<string>(),
                VerticesColumns = new List<string>(),
                Vertices = new List<List<object>>(),
                Triangles = new List<List<object>>(),
                Authority = new Authority(),
                Extent = new Extent() { Parameters= new Parameters() { Bbox = new List<double>() } }
            };

        public void InitTriangleObject()
        {
            try
            {
                // TODO: Move to options in CommandLine
                Coord.Description = "Triangulation to transform coordinates from EUREF89 (EPSG:4258) to NGO1948 (EPSG:4273), with longitude/latitude order.";
                Coord.InputCrs = EpsgSource.CodeString;
                Coord.OutputCrs = EpsgTarget.CodeString;
                Coord.PublicationDate = DateTimeOffset.Now;
                Coord.Name = OutputFileName;
                Coord.License = "Creative Commons Attribution 4.0 International";
                Coord.TransformedComponents.Add(TransformedComponent.horizontal);
                Coord.VerticesColumns.AddRange(new List<string>() { "source_x", "source_y", "target_x", "target_y" });
                Coord.TrianglesColumns.AddRange(new List<string>() { "idx_vertex1", "idx_vertex2", "idx_vertex3" });

                Coord.Authority.Name = "Kartverket";
                Coord.Authority.Url = new Uri("https://www.kartverket.no/til-lands/posisjon/transformere-koordinater");
                Coord.Authority.Email = "post@kartverket.no";
                Coord.Authority.Address = "Postboks 600 Sentrum, NO-3507 Hoenefoss, NORWAY";

                Coord.Extent.Name = "Norway - onshore";
                Coord.Extent.Type = TypeEnum.Bbox;
                Coord.Extent.Parameters.Bbox.AddRange(new List<double> { 4.68d, 57.93d, 31.22d, 71.21d });

                foreach (var point in CptFile.CommonPointList)
                    Coord.Vertices.Add(new List<object>(4)
                {
                    point.Lon1.GetPrecision(10),
                    point.Lat1.GetPrecision(10),
                    point.Lon2.GetPrecision(10),
                    point.Lat2.GetPrecision(10)
                });
            }
            catch (Exception ex)
            {                 
                throw ex;
            }
        }

        public bool Triangulate()
        {
            try
            {
                var index = 0;
                var noOfPoints = CptFile.CommonPointList.Count();

                if (noOfPoints < 3)
                    return false;

                IPoint[] pointList = new IPoint[noOfPoints];

                foreach (var point in CptFile.CommonPointList)
                    pointList[index++] = new Point(point.Lon1, point.Lat1);

                var delaunator = new Delaunator(pointList);

                if (delaunator == null)
                    return false;

                if (delaunator.Triangles.Count() == 0)
                    return false;

                for (int i = 0; i < delaunator.Triangles.Count(); i += 3)
                {
                    int index1 = delaunator.Triangles[i + 0];
                    int index2 = delaunator.Triangles[i + 1];
                    int index3 = delaunator.Triangles[i + 2];

                    Coord.Triangles.Add(new List<object>(3) { index1, index2, index3 });
                }
                return true;
            }
            catch (Exception ex)
            {                
                throw ex;               
            }
        }

        public bool SerializeJson()
        {
            try
            {
                if (OutputFileName == "")
                    return false;

                var json = JsonConvert.SerializeObject(Coord, Converter.Settings);

                using (StreamWriter file = File.CreateText(OutputFileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, Coord, typeof(Coordinate));
                    file.Close();
                }
                return true;
            }
            catch (Exception ex)
            {            
                throw ex;
            }           
        }
    }

    /// <summary>
    /// Schema for triangulation based transformation
    /// </summary>
    public partial class Coordinate
    {
        /// <summary>
        /// Basic information about the agency responsible for the data set
        /// </summary>
        [JsonProperty("authority", NullValueHandling = NullValueHandling.Ignore)]
        public Authority Authority { get; set; }

        /// <summary>
        /// A text description of the file
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Defines the region within which the triangulation is defined. This should be a bounding
        /// box defined as an array of [west,south,east,north] coordinate values in a unspecified
        /// geographic CRS. This bounding box should be seen as approximate, given that triangulation
        /// may be defined with projected coordinates, and also because some triangulations may not
        /// cover the whole bounding box.
        /// </summary>
        [JsonProperty("extent", NullValueHandling = NullValueHandling.Ignore)]
        public Extent Extent { get; set; }

        /// <summary>
        /// File type. Always "triangulation_file"
        /// </summary>
        [JsonProperty("file_type")]
        [JsonConverter(typeof(FileTypeConverter))]
        public FileType FileType { get; set; }

        [JsonProperty("format_version")]
        [JsonConverter(typeof(FormatVersionConverter))]
        public FormatVersion FormatVersion { get; set; }

        /// <summary>
        /// String identifying the CRS of source coordinates in the vertices. Typically "EPSG:XXXX".
        /// If the transformation is for vertical component, this should be the code for a compound
        /// CRS (can be EPSG:XXXX+YYYY where XXXX is the code of the horizontal CRS and YYYY the code
        /// of the vertical CRS). For example, for the KKJ->ETRS89 transformation, this is EPSG:2393
        /// ("KKJ / Finland Uniform Coordinate System"). The input coordinates are assumed to be
        /// passed in the "normalized for visualisation" / "GIS friendly" order, that is longitude,
        /// latitude for geographic coordinates and easting, northing for projected coordinates.
        /// </summary>
        [JsonProperty("input_crs", NullValueHandling = NullValueHandling.Ignore)]
        public string InputCrs { get; set; }

        /// <summary>
        /// License under which the file is published
        /// </summary>
        [JsonProperty("license", NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; set; }

        /// <summary>
        /// Links to related information
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public List<Link> Links { get; set; }

        /// <summary>
        /// A brief descriptive name of the triangulation
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// String identifying the CRS of target coordinates in the vertices. Typically "EPSG:XXXX".
        /// If the transformation is for vertical component, this should be the code for a compound
        /// CRS (can be EPSG:XXXX+YYYY where XXXX is the code of the horizontal CRS and YYYY the code
        /// of the vertical CRS). For example, for the KKJ->ETRS89 transformation, this is EPSG:3067
        /// ("ETRS89 / TM35FIN(E,N)"). The output coordinates will be returned in the "normalized for
        /// visualisation" / "GIS friendly" order, that is easting, that is longitude, latitude for
        /// geographic coordinates and easting, northing for projected coordinates.
        /// </summary>
        [JsonProperty("output_crs", NullValueHandling = NullValueHandling.Ignore)]
        public string OutputCrs { get; set; }

        /// <summary>
        /// The date on which this version of the triangulation was published (or possibly the date
        /// on which it takes effect?)
        /// </summary>
        [JsonProperty("publication_date", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ESDateTimeConverter))]
        public DateTimeOffset? PublicationDate { get; set; }

        /// <summary>
        /// Specify which component of the coordinates are transformed. Either "horizontal",
        /// "vertical" or both
        /// </summary>
        [JsonProperty("transformed_components")]     
        public List<TransformedComponent> TransformedComponents { get; set; }

        /// <summary>
        /// an array whose items are themselves arrays with as many columns as described in
        /// "triangles_columns". The value of the "idx_vertexN" columns must be indices (between 0
        /// and len("vertices"-1) of items of the "vertices" array
        /// </summary>
        [JsonProperty("triangles")]
        public List<List<object>> Triangles { get; set; }

        /// <summary>
        /// Specify the name of the columns of the rows in the "triangles" array. There must be
        /// exactly as many elements in "triangles_columns" as in a row of "triangles". The following
        /// names have a special meaning: "idx_vertex1", "idx_vertex2", "idx_vertex3". They are
        /// compulsory.
        /// </summary>
        [JsonProperty("triangles_columns")]
        public List<string> TrianglesColumns { get; set; }

        /// <summary>
        /// A string identifying the version of the triangulation. The format for specifying version
        /// will be defined by the agency responsible for the triangulation
        /// </summary>
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        /// <summary>
        /// an array whose items are themselves arrays with as many columns as described in
        /// "vertices_columns"
        /// </summary>
        [JsonProperty("vertices")]
        public List<List<object>> Vertices { get; set; }

        /// <summary>
        /// Specify the name of the columns of the rows in the "vertices" array. There must be
        /// exactly as many elements in "vertices_columns" as in a row of "vertices". The following
        /// names have a special meaning: "source_x", "source_y", "target_x", "target_y", "source_z",
        /// "target_z" and "offset_z".  "source_x" and "source_y" are compulsory. "source_x" is for
        /// the source longitude (in degree) or easting. "source_y" is for the source latitude (in
        /// degree) or northing.  "target_x" and "target_y" are compulsory when "horizontal" is
        /// specified in "transformed_components". ("source_z" and "target_z") or "offset_z" are
        /// compulsory when "vertical" is specified in "transformed_components".
        /// </summary>
        [JsonProperty("vertices_columns")]
        public List<string> VerticesColumns { get; set; }
    }

    /// <summary>
    /// Basic information about the agency responsible for the data set
    /// </summary>
    public partial class Authority
    {
        /// <summary>
        /// The postal address of the agency
        /// </summary>
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        /// <summary>
        /// An email contact address for the agency
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        /// <summary>
        /// The name of the agency
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The url of the agency website
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    /// <summary>
    /// Defines the region within which the triangulation is defined. This should be a bounding
    /// box defined as an array of [west,south,east,north] coordinate values in a unspecified
    /// geographic CRS. This bounding box should be seen as approximate, given that triangulation
    /// may be defined with projected coordinates, and also because some triangulations may not
    /// cover the whole bounding box.
    /// </summary>
    public partial class Extent
    {
        /// <summary>
        /// Name of the extent (e.g. "Finland - mainland south of 66°N")
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("parameters")]
        public Parameters Parameters { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(TypeEnumConverter))]
        public TypeEnum Type { get; set; }
    }

    public partial class Parameters
    {
        [JsonProperty("bbox", NullValueHandling = NullValueHandling.Ignore)]
        public List<double> Bbox { get; set; }
    }

    public partial class Link
    {
        /// <summary>
        /// The URL holding the information
        /// </summary>
        [JsonProperty("href")]
        public Uri Href { get; set; }

        /// <summary>
        /// The relationship to the dataset. Proposed relationships are:
        /// - "about": a web page for human consumption describing the model
        /// - "source": the authoritative source data from which the triangulation is built.
        /// - "metadata": ISO 19115 XML metadata regarding the triangulation.
        /// </summary>
        [JsonProperty("rel", NullValueHandling = NullValueHandling.Ignore)]
        public string Rel { get; set; }

        /// <summary>
        /// Description of the link
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public enum TypeEnum { Bbox };

    /// <summary>
    /// File type. Always "triangulation_file"
    /// </summary>
    public enum FileType { TriangulationFile };

    public enum FormatVersion { The10 };

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransformedComponent
    {
        [JsonProperty("Horizontal")]        
        horizontal,         
        [JsonProperty("Vertical")]
        vertical
    };

    public partial class Coordinate
    {
        public static Coordinate FromJson(string json) => JsonConvert.DeserializeObject<Coordinate>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Coordinate self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

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
     
    public class TinModelParams
    {
        public FileInfo Input { get; set; }
        public FileInfo Output { get; set; }         
        public string EpsgSource { get; set; }
        public string EpsgTarget { get; set; }
        public string Version { get; set; }
    }
}
