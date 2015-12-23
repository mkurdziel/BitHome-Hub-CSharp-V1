using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;

namespace SyNet.DataHelpers
{
  /// <summary>
  ///   Generic dictionary that can be XML Serialized
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  [XmlRoot("dictionary")]
  public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
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
      //
      // XML Reader <= 1.0
      //
      if (SyNetSettings.Instance.XMLVersion >= 1.0)
      {
        Type[] paramTypes = new Type[] { typeof(string) };
        MethodInfo keyParse = typeof(TKey).GetMethod("Parse", paramTypes);
        MethodInfo valueParse = typeof(TValue).GetMethod("Parse", paramTypes);

        //bool wasEmpty = p_reader.IsEmptyElement;
        p_reader.Read();

        //if (wasEmpty)
        //  return;

        while (p_reader.NodeType !=
               System.Xml.XmlNodeType.EndElement)
        {
          //p_reader.ReadElementString("Item");

          try
          {
            TKey key;
            TValue value;

            
            String keyString = p_reader["Key"];
            String valueString = p_reader["Value"];

            if (typeof(TKey) == typeof(string))
            {
              key = (TKey)Convert.ChangeType(keyString, typeof(TKey));
            }
            else
            {
              key = (TKey) keyParse.Invoke(null, new object[] {keyString});
            }

            if (typeof(TValue) == typeof(string))
            {
              value = (TValue)Convert.ChangeType(keyString, typeof(TValue));
            }
            else
            {
              value = (TValue)valueParse.Invoke(null, new object[] { valueString });
            }

            this.Add(key, value);

          }
          catch (Exception)
          {
            Debug.WriteLine("[ERR] SerializableDictionary - read error");
          }
          p_reader.Read();
        }
        p_reader.ReadEndElement();
      }
      else
      {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        bool wasEmpty = p_reader.IsEmptyElement;
        p_reader.Read();

        if (wasEmpty)
          return;

        while (p_reader.NodeType !=
               System.Xml.XmlNodeType.EndElement)
        {
          p_reader.ReadStartElement("item");

          p_reader.ReadStartElement("key");
          TKey key = (TKey)keySerializer.Deserialize(p_reader);
          p_reader.ReadEndElement();

          p_reader.ReadStartElement("value");
          TValue value = (TValue)valueSerializer.Deserialize(p_reader);
          p_reader.ReadEndElement();

          this.Add(key, value);

          p_reader.ReadEndElement();
          p_reader.MoveToContent();
        }
        p_reader.ReadEndElement();
      }
    }

    /// <summary>
    ///   Write the XML stream
    /// </summary>
    /// <param name="p_writer"></param>
    public void WriteXml(System.Xml.XmlWriter p_writer)
    {
      MethodInfo keyMethod = typeof(TKey).GetMethod("ToString", new Type[]{});
      MethodInfo valueMethod = typeof(TValue).GetMethod("ToString", new Type[]{});

      foreach (TKey key in this.Keys)
      {
        p_writer.WriteStartElement("Item");

        p_writer.WriteAttributeString("Key", (String)keyMethod.Invoke(key, null));
        p_writer.WriteAttributeString("Value", (String)valueMethod.Invoke(this[key], null));

        p_writer.WriteEndElement();
      }
    }
    #endregion

    /// <summary>
    ///   Default constructor
    /// </summary>
    public SerializableDictionary()
    {
    }

    /// <summary>
    ///   Initilization constructor
    /// </summary>
    /// <param name="p_dict"></param>
    public SerializableDictionary(IDictionary<TKey, TValue> p_dict)
      : base(p_dict)
    {
      //
      // Make sure both methods have a tostring and a tryparse
      if ((typeof(TKey)).GetMethod("ToString") == null ||
          (typeof(TKey)).GetMethod("TryParse") == null ||
          (typeof(TValue)).GetMethod("ToString") == null ||
          (typeof(TValue)).GetMethod("TryParse") == null
        )
      {
        throw new ArgumentException("[ERR] SerializableDictionary - Type missing method");
      }
    }

  }
}