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
    // https://scispace.com/pdf/use-of-ntv2-transformation-grids-in-engineering-applications-5tdrxu6sv4.pdf
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
        public string SYSTEM_F { get; set; } = "";

        [BinaryOffset(96)]
        public string SYSTEM_T { get; set; } = "";

        [BinaryOffset(112)]
        public double MAJOR_F { get; set; } = 0d;

        [BinaryOffset(128)]
        public double MINOR_F { get; set; } = 0d;

        [BinaryOffset(144)]
        public double MAJOR_T { get; set; } = 0d;

        [BinaryOffset(160)]
        public double MINOR_T { get; set; } = 0d;
    }

    public class SubFileHeader
    {
        [BinaryOffset(176)]
        public string SUB_NAME { get; set; } = string.Empty;

        [BinaryOffset(192)]
        public string PARENT { get; set; } = "NONE";

        [BinaryOffset(208)]
        public string CREATED { get; set; } = string.Empty;

        [BinaryOffset(224)]
        public string UPDATED { get; set; } = string.Empty;

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

    public class EndBlock
    {
        public int END { get; set; }
    }

    public class GsbFile : GridFile
    {
        private GridParam _gridParam;
        private GriFile _griFileField1;
        private GriFile _griFileField2;
        private GriFile _griFileField3;
        private GriFile _griFileField4;
        private Header _header;
        private SubFileHeader _subFileHeader;
        private EndBlock _endBlock;

        public GsbFile() : base()
        {           
        }

        public GsbFile(string inputFileName) : base()
        {
            InputFileName = inputFileName;          
        }

        public string InputFileName { get; set; } = string.Empty;

        public GridParam GridParam
        {
            get => _gridParam ??= new GridParam();
            set
            {
                 if (value is null)
                    throw new ArgumentNullException(nameof(value), "GridParam cannot be null.");

                _gridParam = value;
                
                /*
                Header.MAJOR_F = value.MajorF;
                Header.MINOR_F = value.MinorF;
                Header.SYSTEM_F = value.SystemF;
                */

                SubFileHeader.CREATED = DateTime.Now.ToString("yy-MM-dd");
                SubFileHeader.UPDATED = DateTime.Now.ToString("yy-MM-dd");
                SubFileHeader.SUB_NAME = $"SUB_{value.LowerLeftLatitude}_{value.LowerLeftLongitude}";
                SubFileHeader.S_LAT = value.LowerLeftLatitude;
                SubFileHeader.N_LAT = value.UpperLeftLatitude;
                SubFileHeader.W_LONG = value.LowerLeftLongitude;
                SubFileHeader.E_LONG = value.UpperLeftLongitude;
                SubFileHeader.LAT_INC = value.DeltaLatitude;
                SubFileHeader.LONG_INC = value.DeltaLongitude;
                SubFileHeader.GS_COUNT = value.NRows * value.NColumns;
            }
        }

        public Header Header
        { 
            get => _header ??= new Header();
            set => _header = value;
        }

        public SubFileHeader SubFileHeader
        { 
            get => _subFileHeader ??= new SubFileHeader();
            set => _subFileHeader = value;
        }

        public EndBlock EndBlock
        {
            get => _endBlock ??= new EndBlock();
            set => _endBlock = value;
        }

        public GriFile GriFileField1
        {
            get => _griFileField1 ??= new GriFile(GridParam);
            set => _griFileField1 = value;
        }

        public GriFile GriFileField2
        {
            get => _griFileField2 ??= new GriFile(GridParam);
            set => _griFileField2 = value;
        }

        public GriFile GriFileField3
        {
            get => _griFileField3 ??= new GriFile(GridParam);
            set => _griFileField3 = value;
        }

        public GriFile GriFileField4
        {
            get => _griFileField4 ??= new GriFile(GridParam);
            set => _griFileField4 = value;
        }

        public List<float> DataField1
        {
            get => GriFileField1.Data;
            set => GriFileField1.Data = value;
        }

        public List<float> DataField2
        {
            get => GriFileField2.Data;
            set => GriFileField2.Data = value;
        }

        public List<float> DataField3
        {
            get => GriFileField3.Data;
            set => GriFileField3.Data = value;
        }

        public List<float> DataField4
        {
            get => GriFileField4.Data;
            set => GriFileField4.Data = value;
        }

        /*
        public double S_LAT
        { 
            get => SubFileHeader.S_LAT; set => SubFileHeader.S_LAT = value;
        }
        */

        public override bool Read(string inputFileName)
        {
            inputFileName = inputFileName.ToLower();

            var index = 0;
            var col = 0;
           
            if (!File.Exists(inputFileName))
                return false;

            DataField1.Clear();
            DataField2.Clear();
            DataField3.Clear();
            DataField4.Clear();

            try
            {
                using (FileStream fs = new FileStream(inputFileName, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (br.BaseStream.Length < 352)
                        {
                            br.Close();
                            fs.Close();

                            return false;
                        }

                        ReadObject(br, Header);
                        ReadObject(br, SubFileHeader);

                        while (br.BaseStream.Position + 16 < br.BaseStream.Length)
                        {
                            DataField1.Add(br.ReadSingle());
                            DataField2.Add(br.ReadSingle());
                            DataField3.Add(br.ReadSingle());
                            DataField4.Add(br.ReadSingle());
                        }
                        var nameBytes = br.ReadBytes(8);
                        var recordName = Encoding.ASCII.GetString(nameBytes).Trim();

                        if (recordName == "END")
                        {
                            var valueBytes = br.ReadBytes(8);
                            EndBlock.END = BitConverter.ToInt32(valueBytes, 0);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            throw new NotImplementedException();
        }

        public override bool GenerateGridFile(string outputFileName, bool isRandom = false)
        {
            if (OutputFileName == "")
                return false;

            using (FileStream fs = new FileStream(OutputFileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    WriteObject(bw, Header);
                    WriteObject(bw, SubFileHeader);

                    foreach (var v in DataField1)
                        bw.Write(v);

                    WriteObject(bw, EndBlock);

                    bw.Flush();
                    bw.Close();
                }
                fs.Close();

                return true;
            }
            /*
            using var bw1 = new BinaryWriter(fs1);

            WriteObject(bw1, _header);
            WriteObject(bw1, _subFileHeader);

            foreach (var v in DataField1)
                bw1.Write(v);

            WriteObject(bw1, endBlock);

            bw1.Flush();
            bw1.Close();

            return true;
            */
            using (FileStream fs = new FileStream(OutputFileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Close();
                }
                fs.Close();

                return true;
            }

            throw new NotImplementedException();
        }

        private static void ReadObject(BinaryReader reader, object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<BinaryOffsetAttribute>();
                if (attribute is null)
                    continue;

                reader.BaseStream.Position = attribute.Offset;

                var nameBytes = reader.ReadBytes(8);
                if (nameBytes.Length != 8)
                    throw new InvalidDataException("Unexpected end of file.");

                var recordName = Encoding.ASCII.GetString(nameBytes).Trim();
                if (!string.Equals(recordName, property.Name, StringComparison.Ordinal))
                    throw new InvalidDataException($"Expected record '{property.Name}', found '{recordName}'.");

                var valueBytes = reader.ReadBytes(8);
                if (valueBytes.Length != 8)
                    throw new InvalidDataException("Unexpected end of file.");

                object value = property.PropertyType switch
                {
                    _ when property.PropertyType == typeof(int) =>
                        BitConverter.ToInt32(valueBytes, 0),
                    _ when property.PropertyType == typeof(double) =>
                        BitConverter.ToDouble(valueBytes, 0),
                    _ when property.PropertyType == typeof(string) =>
                        Encoding.ASCII.GetString(valueBytes).Trim(),
                    _ when property.PropertyType == typeof(GS_TYPE) =>
                        Enum.Parse(typeof(GS_TYPE), Encoding.ASCII.GetString(valueBytes).Trim().Trim('\0'), true),
                    _ => throw new NotSupportedException(
                        $"Type {property.PropertyType} is not supported.")
                };

                if (property.CanWrite)
                    property.SetValue(obj, value);
                else if (!Equals(property.GetValue(obj), value))
                    throw new InvalidDataException(
                        $"Record '{property.Name}' has an unexpected value.");
            }
        }

        private static void WriteObject(BinaryWriter writer, object obj)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(obj);

            foreach (var property in obj.GetType().GetProperties())
            {
                var attr = property.GetCustomAttribute<BinaryOffsetAttribute>();

                if (attr is null)
                    // continue;
                    writer.BaseStream.Position = writer.BaseStream.Position;
                else
                    writer.BaseStream.Position = attr.Offset;

                // Write the property name first, then its value.
                WriteFixedAscii(writer, property.Name);

                object? value = property.GetValue(obj);
                if (value is null)
                    throw new InvalidDataException($"Property {property.Name} has a null value.");

                long valueStart = writer.BaseStream.Position;

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
                        WriteFixedAscii(writer, s);
                        break;
                    case GS_TYPE t:
                        WriteFixedAscii(writer, t.ToString());
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Type {value.GetType()} is not supported.");
                }

                long bytesWritten = writer.BaseStream.Position - valueStart;
                if (bytesWritten > 8)
                    throw new InvalidDataException(
                        $"Value for {property.Name} exceeds its 8-byte field.");

                for (long i = bytesWritten; i < 8; i++)
                    writer.Write((byte)0);
            }
        }

        private static void WriteFixedAscii(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length > 8)
                throw new InvalidDataException($"'{value}' exceeds the 8-byte field.");

            writer.Write(bytes);
            for (int i = bytes.Length; i < 8; i++)
                writer.Write((byte)' ');
        }
    }
}
