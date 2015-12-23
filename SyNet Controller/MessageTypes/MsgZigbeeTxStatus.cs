using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyNet.MessageTypes
{
  class MsgZigbeeTxStatus : MsgZigbee {

    /// <summary>
    ///   Copy Constructor
    /// </summary>
    /// <param name="p_msg"></param>
    public MsgZigbeeTxStatus(Msg p_msg)
      : base(p_msg)
    {

    }

    /// <summary>
    ///   Return a string representation of the message type
    /// </summary>
    public override string MsgType
    {
      get
      {
        return "ZigbeeTxStatus";
      }
    }
  }
}