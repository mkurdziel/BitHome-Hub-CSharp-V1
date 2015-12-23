using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SyNet.DataHelpers
{
  /// <summary>
  ///   Serializable generic dictionary using an ID as a key
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  [Serializable]
  [XmlRoot("dictionary")]
  public class SerializableDictionaryWithId<TKey, TValue> : Dictionary<TKey, TValue>,
    IXmlSerializable where TValue : IObjectWithID<TKey>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    ///   Read the xml stream
    /// </summary>
    /// <param name="reader"></param>
    public void ReadXml(XmlReader reader)
    {
      XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
      XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

      bool wasEmpty = reader.IsEmptyElement;
      reader.Read();

      if (wasEmpty)
        return;

      while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
      {
        TValue value = (TValue)valueSerializer.Deserialize(reader);
        this.Add(value.ID, value);
        reader.MoveToContent();
      }
      reader.ReadEndElement();

    }

    /// <summary>
    ///   Write the XML stream
    /// </summary>
    /// <param name="writer"></param>
    public void WriteXml(XmlWriter writer)
    {
      try
      {
        XmlSerializer valueSerializer = new XmlSerializer(typeof (TValue));

        foreach (TKey key in this.Keys)
        {
          TValue value = this[key];
          valueSerializer.Serialize(writer, value);
        }
      } catch (System.Exception ex)
      {
        MessageBox.Show(ex.InnerException.Message+ex.InnerException.StackTrace) ;
      }
    }
  }
}
