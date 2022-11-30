using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BitMiracle.LibTiff.Classic;
using gridfiles;

namespace gridfiles
{
    [Serializable]
    [XmlRoot("GdalMetadata"), XmlType("GdalMetadata")]
    public class GdalMetadata : GDALMetadata
    {

    }

    [Serializable]
    [XmlRoot("GDALMetadata"), XmlType("GDALMetadata")]
    public class GDALMetadata
    {
        private List<Item> _gdalMetadataList = new List<Item>();
        
        public GDALMetadata()
        {
        }
       
        [XmlElement(ElementName = nameof(Item))]
        public List<Item> GdalMetadataList
        {
            get { return _gdalMetadataList; }
            set { _gdalMetadataList = value; }
        }

        public void AddItem(Item item)
        {
            _gdalMetadataList.Add(item);
        }

        public void Clear()
        {
            _gdalMetadataList.Clear();
        }

        public void SerializeObject(string filename)
        {
            var mySerializer = new XmlSerializer(typeof(GDALMetadata));
          
            using (var writer = new StreamWriter(filename))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                mySerializer.Serialize(writer, this, ns);
                writer.Close();
            }
        }      
    }
}
