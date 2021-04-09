using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace gridfiles
{
    [Serializable] 
    public class GdalMetadata
    {
        private List<Item> _gdalMetadataList = new List<Item>();
        
        public GdalMetadata()
        {}

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
            var mySerializer = new XmlSerializer(typeof(GdalMetadata));
          
            using (var writer = new StreamWriter(filename))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                mySerializer.Serialize(writer, this, ns);
                writer.Close();
            }
        }

        public static string SerializeToString(object dataToSerialize)
        {
            if (dataToSerialize == null)
                return null;

            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(dataToSerialize.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, dataToSerialize, emptyNamespaces);
                    writer.Close();
                }
                stream.Close();
                return stream.ToString();
            }
        }
    }
}
