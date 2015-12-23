using SyNet.Protocol;

namespace SyNet.GuiHelpers
{
  using System;
  using System.Globalization;
  using System.Windows.Controls;

  internal class ParameterValidationRule : ValidationRule
  {
    public EsnDataTypes DataType { get; set; }
    public EsnParamValidationType ValidationType { get; set; }
    public Object Min { get; set; }
    public Object Max { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      try
      {
      }
      catch (Exception e)
      {
        return new ValidationResult(false, e.Message);
      }
      return new ValidationResult(true, null);
    }
  }
}