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

        public static GDALMetadata StringToSerialize(FieldValue[] dataToString)
        {
            GDALMetadata obj = null;
           
            XmlSerializer ser = new XmlSerializer(typeof(GDALMetadata));

            string xmlString = dataToString[1].ToString();
            xmlString = xmlString.Replace("\n", "\r\n");
            xmlString = xmlString.Remove(xmlString.Length - 1, 1);

            using (var stream = new StringReader(xmlString))
            {   
                using (var reader = new XmlTextReader(stream))
                {
                    if (ser.CanDeserialize(reader))
                        obj = (GDALMetadata)ser.Deserialize(reader);
                    
                    reader.Close();
                }
                stream.Close();
            }
            return obj;
        }
    }

    [Serializable]
    [XmlRoot("SerializedObject"), XmlType("SerializedObject")]
    public class SerializedObject
    {
        private List<SerializedItem> _itemList = new List<SerializedItem>();

        public SerializedObject()
        {
        }

        [XmlElement(ElementName = nameof(SerializedItem))]
        public List<SerializedItem> ItemList
        {
            get { return _itemList; }
            set { _itemList = value; }
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

        public static SerializedObject StringToSerialize()
        {
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "SerializedObject";
            xRoot.IsNullable = true;

            SerializedObject objIn = new SerializedObject();

            objIn.ItemList.Add(new SerializedItem() { Name = Item.NameType.UNITTYPE, MyString = "Sjef", Role = "Rolle" });
            objIn.ItemList.Add(new SerializedItem() { Name = Item.NameType.DESCRIPTION, MyString = "Konge" });
            string xmlString = SerializeToString(objIn);
            
            SerializedObject obj = null;
            XmlSerializer ser = new XmlSerializer(typeof(SerializedObject));             

            using (var stream = new StringReader(xmlString))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    if (ser.CanDeserialize(reader))
                        obj = (SerializedObject)ser.Deserialize(reader);

                    reader.Close();
                }
                stream.Close();
            }
            return obj;
        }
    }

    [Serializable]
    public class SerializedItem
    {
        private Item.NameType _name;

        public  SerializedItem()
        {
        }

        [XmlAttribute("name")]
        public Item.NameType Name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }

        [DefaultValue(false)]
        [XmlAttribute("sample")]
        public string Sample { get; set; }

        [DefaultValue(false)]
        [XmlAttribute("role")]
        public string Role { get; set; }

        [XmlText]
        public String MyString { get; set; } = "";
    }
}
