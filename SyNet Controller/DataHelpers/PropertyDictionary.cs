using System.Collections.Generic;
using System.Xml.Serialization;

namespace SyNet.DataHelpers
{
  /// <summary>
  ///   Generic dictionary that can be XML Serialized
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  public class PropertyDictionary
    : Dictionary<string, string>, IXmlSerializable
  {
    #region IXmlSerializable Members
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Xml.Schema.XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    ///   Read the object from XML
    /// </summary>
    /// <param name="p_reader"></param>
    public void ReadXml(System.Xml.XmlReader p_reader)
    {

      bool wasEmpty = p_reader.IsEmptyElement;
      p_reader.Read();

      if (wasEmpty)
        return;

      while (p_reader.NodeType != System.Xml.XmlNodeType.EndElement)
      {
        string name = p_reader.GetAttribute("Name");
        string value = p_reader.GetAttribute("Value");

        this.Add(name, value);

        p_reader.Read();
      }
      p_reader.ReadEndElement();
    }

    /// <summary>
    ///   Write the XML stream
    /// </summary>
    /// <param name="p_writer"></param>
    public void WriteXml(System.Xml.XmlWriter p_writer)
    {
      foreach (string key in this.Keys)
      {
        p_writer.WriteStartElement("Property");

        p_writer.WriteAttributeString("Name", key);
        p_writer.WriteAttributeString("Value", this[key]);
        p_writer.WriteEndElement();
      }
    }
    #endregion

    /// <summary>
    ///   Default constructor
    /// </summary>
    public PropertyDictionary()
    {
    }

    /// <summary>
    ///   Initilization constructor
    /// </summary>
    /// <param name="p_dict"></param>
    public PropertyDictionary(IDictionary<string, string> p_dict)
      : base(p_dict)
    {
    }

  }
}