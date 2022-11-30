using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BitMiracle.LibTiff.Classic;

namespace gridfiles
{    
    [Serializable]
    public static class SerializedObject<T>
    {
        public static T StringToSerialize(FieldValue[] dataToString)
        {
            T obj = default(T);

            XmlSerializer ser = new XmlSerializer(typeof(T));

            string xmlString = dataToString[1].ToString();
            xmlString = xmlString.Replace("\n", "\r\n");
            xmlString = xmlString.Remove(xmlString.Length - 1, 1);

            using (var stream = new StringReader(xmlString))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    if (ser.CanDeserialize(reader))
                        obj = (T)ser.Deserialize(reader);

                    reader.Close();
                }
                stream.Close();
            }
            return obj;
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
