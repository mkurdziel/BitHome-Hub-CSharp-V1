using System;
using System.Text;
using SyNet.Protocol;

namespace SyNet.MessageTypes
{
  /// <summary>
  ///   Message type for SyNet function transmit
  /// </summary>
  public class MsgSyNetFunctionTransmit : MsgZigbeeTxRequest
  {
    /// <summary>
    ///   Copy constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgSyNetFunctionTransmit(Msg p_msg)
      : base(p_msg)
    {
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_xbeeTarget"></param>
    /// <param name="p_functionID"></param>
    public MsgSyNetFunctionTransmit(DeviceXBee p_xbeeTarget, byte p_functionID)
      : base(p_xbeeTarget)
    {
      byte[] dataBytes = new byte[3];
      dataBytes[0] = (byte) EsnSyNetValues.SYNET_PACKET_START;
      dataBytes[1] = (byte) EsnAPI.FUNCTION_TRANSMIT;
      dataBytes[2] = p_functionID;
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get { return "SyNetFunctionTransmit"; }
    }

    /// <summary>
    ///   Information string
    /// </summary>
    public override string Information
    {
      get { return MsgData[PAYLOAD_OFFSET + 2].ToString(); }
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(byte p_param)
    {
      byte[] dataBytes = new byte[1];
      dataBytes[0] = p_param;
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(UInt16 p_param)
    {
      byte[] dataBytes = EBitConverter.GetBytes(p_param);
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(bool p_param)
    {
      byte b = p_param ? (byte)0x01 : (byte)0x00;
      AddParam(b);
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(UInt32 p_param)
    {
      byte[] dataBytes = EBitConverter.GetBytes(p_param);
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(UInt64 p_param)
    {
      byte[] dataBytes = EBitConverter.GetBytes(p_param);
      base.AddData(dataBytes);
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(Parameter p_param)
    {
      switch(p_param.DataType)
      {
        case EsnDataTypes.BOOL:
          AddParam(p_param.IntValue == 0 ? false : true);
          break;
        case EsnDataTypes.BYTE:
          AddParam((byte)p_param.IntValue);
          break;
        case EsnDataTypes.DWORD:
          AddParam((UInt32)p_param.IntValue);
          break;
        case EsnDataTypes.STRING:
          AddParam(p_param.StringValue);
          break;
        case EsnDataTypes.WORD:
          AddParam((UInt16)p_param.IntValue);
          break;
        default:
          throw new Exception(@"MsgSyNetDeviceFunctionTransmit.AddParam - adding invalid parameter");
      }
    }

    /// <summary>
    ///   Add a parameter to the function transmit
    /// </summary>
    /// <param name="p_param"></param>
    public void AddParam(String p_param)
    {
      byte[] dataBytes = new byte[p_param.Length + 1];  // leave room for zero terminator
      int nBytesEncoded = ASCIIEncoding.UTF8.GetBytes(p_param, 0, p_param.Length, dataBytes, 0);  // copy string
      dataBytes[nBytesEncoded] = 0;  // place string termination
      base.AddData(dataBytes);  // write outbound message
    }
  }
}