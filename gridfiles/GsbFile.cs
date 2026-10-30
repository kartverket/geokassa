using NetTopologySuite.Densify;
using NetTopologySuite.Operation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static NetTopologySuite.Geometries.Utilities.GeometryMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gridfiles
{
    /*
    ┌────────────────────────────────────────────────────────┐
    │ Overview Header (11 records x 16 bytes = 176 bytes)    │
    ├────────────────────────────────────────────────────────┤
    │ Sub-File Header (11 records x 16 bytes = 176 bytes)   │
    ├────────────────────────────────────────────────────────┤
    │ Grid Nodes (Lat/Lon/Accuracy data... variable size)   │
    ├────────────────────────────────────────────────────────┤
    │ ... (More Sub-Files & Grid Nodes can follow)           │
    ├────────────────────────────────────────────────────────┤
    │ End Record                                             │
    └────────────────────────────────────────────────────────┘
    */

    /*    
    Offset (Bytes)	Identifier	Data Type	Description
    0	NUM_OREC	Integer	Number of overview records (always 11)
    16	NUM_SREC	Integer	Number of sub-file header records (always 11)
    32	NUM_FILE	Integer	Total number of sub-files contained within the .gsb
    48	GS_TYPE	String	Grid shift units (typically SECONDS, MINUTES, or DEGREES)
    64	VERSION	String	NTv2 version id identifier (e.g., GDA2020 or NTv2.0)
    80	SYSTEM_F	String	"From" Coordinate Reference System / Datum
    96	SYSTEM_T	String	"To" Coordinate Reference System / Datum
    112	MAJOR_F	Double	Semi-major axis of the "From" ellipsoid (meters)
    128	MINOR_F	Double	Semi-minor axis of the "From" ellipsoid (meters)
    144	MAJOR_T	Double	Semi-major axis of the "To" ellipsoid (meters)
    160	MINOR_T	Double	Semi-minor axis of the "To" ellipsoid (meters) 
    */

    /*
    SUB_NAME	String	Unique identity name of the sub-file
    PARENT	String	Name of parent sub-file (set to NONE for the base layer)
    CREATED	String	Date of creation (e.g., YYYYMMDD)
    UPDATED	String	Date of last revision
    S_LAT	Double	Southern boundary limit of the sub-grid
    N_LAT	Double	Northern boundary limit of the sub-grid
    E_LONG	Double	Eastern boundary limit (Note: Positive West / Negative East)
    W_LONG	Double	Western boundary limit (Note: Positive West / Negative East)
    LAT_INC	Double	Grid step spacing along the Latitude axis 
    */
         
    public enum GS_TYPE
    { 
        SECONDS,
        MINUTES,
        DEGREES
    };

    [AttributeUsage(AttributeTargets.Property)]
    public class BinaryOffsetAttribute : Attribute
    {
        public long Offset { get; }

        public BinaryOffsetAttribute(long offset)
        {
            Offset = offset;
        }
    }
    
    public class Header
    { 
        [BinaryOffset(0)]
        public int NUM_OREC { get; } = 11;

        [BinaryOffset(16)]
        public int NUM_SREC { get; } = 11;

        [BinaryOffset(32)]
        public int NUM_FILE { get; } = 1;

        [BinaryOffset(48)]
        public GS_TYPE GS_TYPE { get; set; } = GS_TYPE.SECONDS;

        [BinaryOffset(64)]
        public string VERSION { get; set; } = "NTv2.0";

        [BinaryOffset(80)]
        public string SYSTEM_F { get; set; } = "EPSG:11022";

        [BinaryOffset(96)]
        public string SYSTEM_T { get; set; } = "EPSG:5972";

        [BinaryOffset(112)]
        public double MAJOR_F { get; set; }

        [BinaryOffset(128)]
        public double MINOR_F { get; set; }

        [BinaryOffset(144)]
        public double MAJOR_T { get; set; }

        [BinaryOffset(160)]
        public double MINOR_T { get; set; }
    }

    public class SubFileHeader
    {
        [BinaryOffset(176)]
        public string SUB_NAME { get; set; }

        [BinaryOffset(192)]
        public string PARENT { get; set; }

        [BinaryOffset(208)]
        public string CREATED { get; set; }

        [BinaryOffset(224)]
        public string UPDATED { get; set; }

        [BinaryOffset(240)]
        public double S_LAT { get; set; }

        [BinaryOffset(256)]
        public double N_LAT { get; set; }

        [BinaryOffset(272)]
        public double E_LONG { get; set; }

        [BinaryOffset(288)]
        public double W_LONG { get; set; }

        [BinaryOffset(304)]
        public double LAT_INC { get; set; }

        [BinaryOffset(320)]
        public double LONG_INC { get; set; }

        [BinaryOffset(336)]
        public int GS_COUNT { get; set; }
    }

    public class GsbFile : GridFile
    {
        private GridParam _gridParam;

        private const int _NUM_OREC = 11;
        private const int _NUM_SREC = 11;
        private const int _NUM_FILE = 1;
        private GS_TYPE _GS_TYPE = GS_TYPE.SECONDS;
        private string _VERSION = "NTv2.0";
        private string _SYSTEM_F = "EPSG:11022";
        private string _SYSTEM_T = "EPSG:5972";
        private double _MAJOR_F;
        private double _MINOR_F;
        private double _MAJOR_T;
        private double _MINOR_T;

        private string _subName = string.Empty;
        private string _parent = "NONE";
        private string _created = string.Empty;
        private string _updated = string.Empty;
        private double _southLatitude;
        private double _northLatitude;
        private double _eastLongitude;
        private double _westLongitude;
        private double _latitudeIncrement;
        private double _longitudeIncrement;
        private int _gridNodeCount;

        public GsbFile() 
        {   
            _gridParam = new GridParam(); 
        }

        public GsbFile(string outputFileName) : base()
        {
            OutputFileName = outputFileName;
            _gridParam = new GridParam();
        }
        
        public override bool Read(string inputFileName)
        {
             throw new NotImplementedException();
        }

        public static void WriteObject(BinaryWriter writer, object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var attr = property.GetCustomAttribute<BinaryOffsetAttribute>();
                if (attr == null)
                    continue;

                writer.BaseStream.Position = attr.Offset;
                object value = property.GetValue(obj)!;

                switch (value)
                {
                    case int i:
                        writer.Write(i);
                        break;

                    case long l:
                        writer.Write(l);
                        break;

                    case double d:
                        writer.Write(d);
                        break;

                    case float f:
                        writer.Write(f);
                        break;

                    case bool b:
                        writer.Write(b);
                        break;

                    case string s:
                        writer.Write(s);
                        break;

                    case GS_TYPE t:
                        writer.Write(t.ToString());
                        break;

                    default:
                        throw new NotSupportedException(
                                $"Type {value.GetType()} is not supported");
                }
            }
        }

        public override bool GenerateGridFile(string outputFileName, bool isRandom = false)
        {
            if (OutputFileName == "")
                return false;

            var header = new Header
            {
                VERSION = "NTv2.0",
                MAJOR_F = 232332,
                MINOR_F = 232332,
                MAJOR_T = 232332,
                MINOR_T = 232332
            };

            using var fs1 = File.Create(outputFileName);
            using var bw1 = new BinaryWriter(fs1);

            WriteObject(bw1, header);

            return true;

            using (FileStream fs = new FileStream(OutputFileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Seek(0, SeekOrigin.Begin);
                    bw.Write(_NUM_OREC);
                    bw.Seek(16, SeekOrigin.Begin);
                    bw.Write(_NUM_SREC);
                    bw.Write(_NUM_FILE);
                    bw.Write(_GS_TYPE.ToString());
                    //bw.Write(Version);
                    bw.Write(_SYSTEM_F);
                    bw.Write(_SYSTEM_T);

                    bw.Close();
                }
                fs.Close();

                return true;
            }

            throw new NotImplementedException();
        }
    }
}
