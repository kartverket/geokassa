using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gridfiles
{
    public class CommandParams
    {
        public FileInfo Output { get; set; }
        public string GridName { get; set; } = "";
        public string Area { get; set; } = "";
        public string Desc { get; set; } = "";
        public string Email { get; set; } = "";
        public int TileSize { get; set; }
        public GeoTiffFile.TiffOutputTypeshort Type { get; set; }
    }

    public class Lsc2GeoTiffCommandParams : CommandParams
    {
        public FileInfo InputSource { get; set; }
        public FileInfo InputTarget { get; set; }
        public FileInfo InputJson { get; set; }
        public int Dim { get; set; }
        public string Epsg2d { get; set; }
        public string EpsgSource { get; set; }
        public string EpsgTarget { get; set; }
        public double LowerLeftLongitude { get; set; }
        public double LowerLeftLatitude { get; set; }
        public double DeltaLongitude { get; set; }
        public double DeltaLatitude { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public double Agl { get; set; } = 0;
        public double C0 { get; set; }
        public double Cl { get; set; }
        public double Sn { get; set; }
    }

    public class Bin2GeoTiffCommandParams : CommandParams
    {
        public FileInfo Input { get; set; }
        public string Epsg2d { get; set; }
        public string Epsg3d { get; set; }
        public string EpsgSource { get; set; }
        public string EpsgTarget { get; set; }
    }

    public class Gri2GeoTiffCommandParams : CommandParams
    {
        public FileInfo InputE { get; set; }
        public FileInfo InputN { get; set; }
        public FileInfo InputU { get; set; }
        public string Epsg2d { get; set; }
        public string Epsg3d { get; set; }
        public string EpsgTarget { get; set; }
        public string EpsgSource { get; set; }
        public int Dim =>
            (InputE != null ? 1 : 0) +
            (InputN != null ? 1 : 0) +
            (InputU != null ? 1 : 0);
    }

    public class Gtx2GeoTiffCommandParams : CommandParams
    {
        public FileInfo Input { get; set; }
        public string Epsg2d { get; set; }
        public string Epsg3d { get; set; }
        public string EpsgTarget { get; set; }
    }

    public class Ct2Gtx2GeoTiffCommandParams : CommandParams
    {
        public FileInfo Ct2 { get; set; }
        public FileInfo Gtx { get; set; }
        public string Epsg2d { get; set; }
        public string EpsgSource { get; set; }
        public string EpsgTarget { get; set; }
        public int Dim { get; set; }
    }

    public class Csvs2Ct2CommandParams
    {
        public double FLat { get; set; }
        public double FLon { get; set; }
        public string Desc { get; set; } = "";
        public GridFile.Direction Dir { get; set; } = GridFile.Direction.fwd;
        public FileInfo FromSys { get; set; }
        public FileInfo ToSys { get; set; }
        public FileInfo Output { get; set; }
        public FileInfo GJs { get; set; }
    }

    public class MakeGridCommandParams
    {
        public FileInfo Output { get; set; }
        public double LowerLeftLongitude { get; set; }
        public double LowerLeftLatitude { get; set; }
        public double DeltaLongitude { get; set; }
        public double DeltaLatitude { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string Sep { get; set; } = " ";
    }

    public class MergeGridsCommandParams
    {
        public string Desc { get; set; } = "";
        public GridFile.GridType Type { get; set; }
        public FileInfo Grid1 { get; set; }
        public FileInfo Grid2 { get; set; }
        public FileInfo GridTarget { get; set; }
    }
    
    public class ClipCommandParams
    {
        public FileInfo InputFile { get; set; }
        public FileInfo OutputFile { get; set; }
        public GridFile.GridType Type { get; set; }
        public string[] Bbox { get; set; }
        public int TileSize { get; set; }
    }
}
