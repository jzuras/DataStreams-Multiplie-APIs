using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DataStreams.DataModel.Models;

//[DataContract]
[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/DataStreams.DataModel.Models")]
public class Stream
{
    [Key]
    [DataMember]
    public string Name { get; set; } = string.Empty;
    [DataMember]
    public int? MinValue { get; set; } = null;
    [DataMember]
    public int? MaxValue { get; set; } = null;

    public string ToXml()
    {
        using (var memoryStream = new MemoryStream())
        {
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
            {
                var serializer = new XmlSerializer(typeof(Stream));
                serializer.Serialize(xmlWriter, this);
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
