
using System.Xml.Serialization;
using SyNet.Actions;
using SyNet.DataHelpers;

namespace SyNet.Gui.Models
{
  /// <summary>
  ///   A parameter, derived from ActionParameter, representing the GIU 
  ///   version of the ActionParameter
  /// </summary>
  public class GuiParameter : ActionParameter
  {
    ////////////////////////////////////////////////////////////////////////////
    #region Enumerations

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Member Variables

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Public Properties

    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Constructors

    /// <summary>
    ///   Default Constructor 
    /// </summary>
    public GuiParameter()
    {
    }

    /// <summary>
    ///   Copy Constructor for an ActionParameter
    /// </summary>
    /// <param name="p_actionParameter"></param>
    public GuiParameter( ActionParameter p_actionParameter )
      : base(p_actionParameter)
    {
    }
    #endregion
    ////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    #region Overrides of Parameter

    /// <summary>
    ///   Full name of parameter
    /// </summary>
    [XmlAttribute]
    public override string FullName
    {
      get { return base.Name; }
    }

    /// <summary>
    ///   String value of paramter
    /// </summary>
    [XmlIgnore]
    public override string StringValue
    {
      get { return base.Value; }
      set { base.Value = value; }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
  }
}
