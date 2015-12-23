using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SyNet.Actions;


namespace SyNet.GuiHelpers
{

  /// <summary>
  ///   Converter class to convert device status to status icon path
  /// </summary>
  public class DeviceStatusToIconConverter : IValueConverter
  {
    /// <summary>
    ///   Convert method
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object o, Type type, object parameter,
                      CultureInfo culture)
    {
      Device.StatusEnum status = (Device.StatusEnum) o;
      switch (status)
      {
        case Device.StatusEnum.ACTIVE:
          return "pack://application:,,,/Resources/StatusActive.png";
        case Device.StatusEnum.RECENT:
          return "pack://application:,,,/Resources/StatusRecent.png";
        case Device.StatusEnum.UNKNOWN:
          return "pack://application:,,,/Resources/StatusUnknown.png";
      }
      return "pack://application:,,,/Resources/StatusDead.png";
    }

    /// <summary>
    ///   Convert back method
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object o, Type type, object parameter,
                              CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  ///   Convert serial port status to icon path
  /// </summary>
  public class SerialStatusToIconConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object o, Type type, object parameter,
                      CultureInfo culture)
    {
      bool status = (bool)o;
      if (status)
      {
        return "pack://application:,,,/Resources/StatusActiveSmall.png";
      }
      return "pack://application:,,,/Resources/StatusDeadSmall.png";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object o, Type type, object parameter,
                              CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  ///   Converter to convert action running status to icon
  /// </summary>
  public class ActionRunningToIconConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object o, Type type, object parameter,
                      CultureInfo culture)
    {
      bool running = (bool) o;
      if (running)
      {
        return Visibility.Visible;
      }
      return Visibility.Hidden;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object o, Type type, object parameter,
                              CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  ///   Converts action type to icon path
  /// </summary>
  public class ActionTypeToIconConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object o, Type type, object parameter,
                      CultureInfo culture)
    {
      ActionParameter.EsnActionParameterType actiontype =
        (ActionParameter.EsnActionParameterType)o;
      switch (actiontype)
      {
        case ActionParameter.EsnActionParameterType.CONSTANT:
          return "pack://application:,,,/Resources/Parameters/ParameterSmallEdit.png";
        case ActionParameter.EsnActionParameterType.DEPENDENT:
          return "pack://application:,,,/Resources/Parameters/ParameterSmallLink.png";
        case ActionParameter.EsnActionParameterType.INPUT:
          return "pack://application:,,,/Resources/Parameters/ParameterSmallGo.png";
        case ActionParameter.EsnActionParameterType.INTERNAL:
          return "pack://application:,,,/Resources/Parameters/ParameterSmall.png";
      }
      return "pack://application:,,,/Resources/brick.png";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object o, Type type, object parameter,
                              CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }


  /// <summary>
  ///   Converts bool to visibility
  /// </summary>
  public class BoolToVisibilityConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object o, Type type, object parameter,
                      CultureInfo culture)
    {
      bool input = (bool) o;
      if (input)
      {
        return Visibility.Visible;
      }
      return Visibility.Collapsed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object o, Type type, object parameter,
                              CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  ///   Converts bool to Invisibility
  /// </summary>
  public class BoolToInvisibilityConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert( object o, Type type, object parameter,
                      CultureInfo culture )
    {
      bool input = (bool)o;
      if (!input)
      {
        return Visibility.Visible;
      }
      return Visibility.Collapsed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack( object o, Type type, object parameter,
                              CultureInfo culture )
    {
      throw new NotSupportedException();
    }
  }


  /// <summary>
  ///   Converts bool to visibility
  /// </summary>
  public class ColorToBrushConverter : IValueConverter
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert( object o, Type type, object parameter,
                      CultureInfo culture )
    {
      Color input = (Color)o;
      return new SolidColorBrush(input);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="type"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack( object o, Type type, object parameter,
                              CultureInfo culture )
    {
      throw new NotSupportedException();
    }
  }
}