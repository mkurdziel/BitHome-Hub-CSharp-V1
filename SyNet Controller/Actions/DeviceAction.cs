using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SyNet.Actions
{
  /// <summary>
  ///   Class representing a DeviceAction
  /// </summary>
  public class DeviceAction : Action
  {

    /// <summary>
    ///   Device ID
    /// </summary>
    [XmlAttribute]
    public UInt64 DeviceID { get; set; }

    /// <summary>
    ///   Function ID represented by this action
    /// </summary>
    [XmlAttribute]
    public int FunctionID { get; set; }

    /// <summary>
    ///   String type identifier
    /// </summary>
    public override string TypeString
    {
      get { return "Device"; }
    }

    /// <summary>
    ///   Gets the formal name of the action based on its type and lineage
    /// </summary>
    public override string FormalName
    {
      get
      {
        return String.Format("{0}.{1}", this.Device.DeviceName, this.DeviceFunction.Name);
      }
    }

    /// <summary>
    ///   FUll list of parameters in this action
    /// </summary>
    [XmlIgnore]
    public override List<ActionParameter> Parameters
    {
      get
      {
        return ActionParameters;
      }
    }

    /// <summary>
    ///   List of action parameters maintained in this action
    /// </summary>
    [XmlElement(ElementName = "ActionParameter")]
    public List<ActionParameter> ActionParameters
    { get; set; }

    /// <summary>
    ///   Retreives a reference to the actual DeviceFunction contained in this
    ///   DeviceAction
    /// </summary>
    [XmlIgnore]
    private DeviceFunction DeviceFunction
    {
      get
      {
        DeviceFunction function = null;
        Device d = NodeManager.Instance.GetNode(DeviceID);

        if (d != null)
        {
          function = d.GetFunction(FunctionID);
        }
        else
        {
          throw new Exception("DeviceActionFunction - Accessing null device");
        }

        return function;
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public DeviceAction() : this(0)
    {
      
    }

    /// <summary>
    ///   Constructor with ID initializer
    /// </summary>
    /// <param name="p_actionID"></param>
    public DeviceAction(UInt64 p_actionID)
      : base(p_actionID)  
    {
      ActionParameters = new List<ActionParameter>();
    }

    /// <summary>
    ///   Copy constructor for a DeviceFunction
    /// </summary>
    /// <param name="p_function"></param>
    public DeviceAction(DeviceFunction p_function)
      : this(p_function.ActionID)
    {
      DeviceID = p_function.DeviceID;
      FunctionID = p_function.ID;
      Name = p_function.Name;

      //
      // Set the action ID of the DeviceFunction to this action ID.
      // This way we can persist the dynamically created device actions
      //
      p_function.ActionID = this.ActionID;

      // TODO: mkurdziel - make device parameter property that
      // does not include the return parameter
      foreach (DeviceParameter param in p_function.Parameters.Values)
      {
        if (param.ID != 0)
        {
          ActionParameter aParam = new ActionParameter(param);
          aParam.ActionID = ActionID;
          param.PreviousParamID = aParam.ParamID;
          ActionParameters.Add(aParam);
        }
      }
    }


    /// <summary>
    ///   Executes the device action
    /// </summary>
    /// <returns></returns>
    public override bool Execute()
    {
      bool bRetVal = false;
      bool bValid = true;
      if (DeviceFunction != null)
      {
        if (DeviceFunction.LockDevice())
        {
          foreach (ActionParameter aParam in ActionParameters)
          {
            Parameter dParam =
              ActionManager.Instance.GetParameter(aParam.ParentParamID);
            if (dParam != null &&
                aParam.IsValid)
            {
              dParam.StringValue = aParam.StringValue;
            }
            else
            {
              if (dParam != null)
                Debug.WriteLine(
                  string.Format("[ERR] DeviceAction.Execute - invalid param: {0}", dParam.Name));
              bValid = false;
              break;
            }
          }

          if (bValid)
          {
            bRetVal = DeviceFunction.Send();
          }
          DeviceFunction.UnlockDevice();
        }
      }
      return bRetVal;
    }

    /// <summary>
    ///   ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      Device d = this.Device;
      if (d != null)
      {
        return String.Format(
          "{0} - {1}", d.DeviceName, Name);
      }
      return Name;
    }

    /// <summary>
    ///   Gets a reference to the device object
    /// </summary>
    private Device Device
    {
      get
      {
        return NodeManager.Instance.GetNode(DeviceID);
      }
    }
  }
}