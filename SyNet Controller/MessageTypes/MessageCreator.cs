using System;
using System.Diagnostics;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Class to create simulated messages for testing purposes
  /// </summary>
  static public class MessageCreator
  {

    /// <summary>
    ///   Creates a function receive message
    /// </summary>
    /// <param name="p_serialNumber"></param>
    /// <param name="p_networkAddress"></param>
    /// <param name="p_functionID"></param>
    /// <param name="p_dataType"></param>
    /// <param name="p_paramValue"></param>
    /// <returns></returns>
    public static Msg CreateSyNetFunctionReceive
      (
        UInt64 p_serialNumber,
        UInt16 p_networkAddress,
        byte p_functionID,
        EsnDataTypes p_dataType,
        string p_paramValue
      )
    {
      MsgZigbee msg = new MsgZigbee();

      UInt16 size = 16;

      switch (p_dataType)
      {
        case EsnDataTypes.BYTE:
          size += 1;
          break;
        case EsnDataTypes.WORD:
          size += 2;
          break;
        default:
          throw new NotImplementedException();
      }

      //
      // start byte
      //
      msg.AddFrameByte(MsgZigbee.XBEE_PACKET_START);

      //
      // Add size bytes
      //
      byte[] sizeBytes = EBitConverter.GetBytes((UInt16)size);
      foreach (byte b in sizeBytes)
      {
        msg.AddFrameByte(b);
      }

      //
      // Add API
      //
      msg.AddFrameByte((byte)EsnAPI.FUNCTION_RECEIVE);

      //
      // Add 64 bit address
      //
      byte[] serialBytes = EBitConverter.GetBytes(p_serialNumber);
      foreach (byte b in serialBytes)
      {
        msg.AddFrameByte(b);
      }

      //
      // Add 16 bit address
      //
      byte[] networkBytes = EBitConverter.GetBytes(p_networkAddress);
      foreach (byte b in networkBytes)
      {
        msg.AddFrameByte(b);
      }

      //
      // Add options
      // 1 = packet acknowledged
      //
      msg.AddFrameByte(0x01);

      //
      // Add synet byte
      //
      msg.AddFrameByte((byte)Protocol.EsnSyNetValues.SYNET_PACKET_START);

      //
      // Add api byte
      //
      msg.AddFrameByte((byte)Protocol.EsnAPI.FUNCTION_RECEIVE);

      //
      // Add the function id
      //
      msg.AddFrameByte(p_functionID);

      //
      // Add the return value
      //
      switch (p_dataType)
      {
        case EsnDataTypes.BYTE:
          msg.AddFrameByte((byte)EsnDataTypes.BYTE);
          msg.AddFrameByte(byte.Parse(p_paramValue));
          break;

        case EsnDataTypes.WORD:
          msg.AddFrameByte((byte)EsnDataTypes.WORD);
          byte[] valueBytes = EBitConverter.GetBytes(UInt16.Parse(p_paramValue));
          foreach (byte b in valueBytes)
          {
            msg.AddFrameByte(b);
          }
          break;

        default:
          throw new NotImplementedException();
      }

      bool bRetVal = msg.SkipChecksum();

      return msg;
    }
  }
}
