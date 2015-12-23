using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SyNet.GuiControls
{
  /// <summary>
  ///   Class to hold font information
  /// </summary>
  public class FontInfo : INotifyPropertyChanged
  {
    ////////////////////////////////////////////////////////////////////////////
    #region FontFamily

    FontFamily fontFamily = new FontFamily();
    /// <summary>
    /// Gets or sets the FontFamily property.
    /// </summary>
    [XmlIgnore]
    public FontFamily FontFamily
    {
      get { return fontFamily; }
      set
      {
        if (fontFamily != value)
        {
          Typeface newTypeface = selectBestMatchingTypeface(value);

          fontFamily = value;
          OnPropertyChanged("FontFamily");
          Typeface = newTypeface;
        }
      }
    }

    #endregion FontFamily
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Select Best Matching Typeface
    /// <summary>
    /// Selects the best matching typeface.
    /// </summary>
    /// <param name="family">Font family.</param>
    /// <param name="style">Font style.</param>
    /// <param name="weight">Font weight.</param>
    /// <param name="stretch">Font stretch.</param>
    /// <returns></returns>
    static Typeface selectBestMatchingTypeface( FontFamily family, FontStyle style
      , FontWeight weight, FontStretch stretch )
    {
      ICollection<Typeface> typefaces = family.GetTypefaces();
      if (typefaces.Count == 0)
        return null;
      IEnumerable<Typeface> matchingTypefaces
        = from tf in typefaces
          where tf.Style == style && tf.Weight == weight && tf.Stretch == stretch
          select tf;
      if (matchingTypefaces.Count() == 0)
        matchingTypefaces = from tf in typefaces
                            where tf.Style == style && tf.Weight == weight
                            select tf;
      if (matchingTypefaces.Count() == 0)
        matchingTypefaces = from tf in typefaces
                            where tf.Style == style
                            select tf;
      if (matchingTypefaces.Count() == 0)
        return typefaces.First();
      return matchingTypefaces.First();
    }

    /// <summary>
    /// Selects the best matching typeface.
    /// </summary>
    /// <param name="family">The family.</param>
    /// <returns></returns>
    Typeface selectBestMatchingTypeface( FontFamily family )
    {
      if (Typeface == null)
      {
        ICollection<Typeface> typefaces = family.GetTypefaces();
        if (typefaces.Count == 0)
          return null;
        return typefaces.First();
      }
      return selectBestMatchingTypeface(family, Typeface.Style, Typeface.Weight, Typeface.Stretch);
    }
    #endregion Select Best Matching Typeface
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Typeface

    Typeface typeface;
    /// <summary>
    /// Gets or sets the typeface.
    /// </summary>
    /// <value>The typeface.</value>
    [XmlIgnore]
    public Typeface Typeface
    {
      get { return typeface; }
      set
      {
        if (typeface != value)
        {
          typeface = value;
          OnPropertyChanged("Typeface");
          OnPropertyChanged("FontStyle");
          OnPropertyChanged("FontWeight");
          OnPropertyChanged("FontStretch");
        }
      }
    }

    /// <summary>
    ///   Gets the font style from the typeface
    /// </summary>
    [XmlIgnore]
    public FontStyle FontStyle
    {
      get
      {
        return Typeface.Style;
      }
    }

    /// <summary>
    ///   Gets the font weight from the typeface
    /// </summary>
    [XmlIgnore]
    public FontWeight FontWeight
    {
      get
      {
        return Typeface.Weight;
      }
    }

    /// <summary>
    ///   Gets the font stretch from the typeface
    /// </summary>
    [XmlIgnore]
    public FontStretch FontStretch
    {
      get
      {
        return Typeface.Stretch;
      }
    }

    #endregion Typeface
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region FontSize

    double fontSize = 12.0;
    /// <summary>
    /// Gets or sets the FontSize property.
    /// </summary>
    [XmlAttribute]
    public double FontSize
    {
      get { return fontSize; }
      set
      {
        if (fontSize != value)
        {
          fontSize = value;
          OnPropertyChanged("FontSize");
        }
      }
    }

    #endregion FontSize
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region ForegroundColor

    private Color m_foregroundColor = Colors.Black;

    /// <summary>
    ///   Gets the foreground color of the font
    /// </summary>
    [XmlIgnore]
    public Color ForegroundColor
    {
      get { return m_foregroundColor; }
      set
      {
        m_foregroundColor = value;
        OnPropertyChanged("ForegroundColor");
      }
    }

    /// <summary>
    ///   Gets or sets the background color via hex string
    /// </summary>
    [XmlAttribute(AttributeName = "ForegroundColor")]
    public string ForegroundColorString
    {
      get
      {
        return ForegroundColor.ToString();
      }
      set
      {
        ForegroundColor = (Color)ColorConverter.ConvertFromString(value);
      }
    }



    #endregion
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Font Decorations

    //bool strikethrough;
    //[XmlIgnore]
    //public bool Strikethrough
    //{
    //  get { return strikethrough; }
    //  set
    //  {
    //    if (strikethrough != value)
    //    {
    //      strikethrough = value;
    //      OnPropertyChanged("Strikethrough");
    //      OnPropertyChanged("Decorations");
    //    }
    //  }
    //}

    //bool overline;
    //[XmlIgnore]
    //public bool Overline
    //{
    //  get { return overline; }
    //  set
    //  {
    //    if (overline != value)
    //    {
    //      overline = value;
    //      OnPropertyChanged("Overline");
    //      OnPropertyChanged("Decorations");
    //    }
    //  }
    //}

    //bool baseline;
    //[XmlIgnore]
    //public bool Baseline
    //{
    //  get { return baseline; }
    //  set
    //  {
    //    if (baseline != value)
    //    {
    //      baseline = value;
    //      OnPropertyChanged("Baseline");
    //      OnPropertyChanged("Decorations");
    //    }
    //  }
    //}

    //bool underline;
    //[XmlIgnore]
    //public bool Underline
    //{
    //  get { return underline; }
    //  set
    //  {
    //    if (underline != value)
    //    {
    //      underline = value;
    //      OnPropertyChanged("Underline");
    //      OnPropertyChanged("Decorations");
    //    }
    //  }
    //}

    ///// <summary>
    ///// Gets or sets the Font Decorations collection.
    ///// </summary>
    ///// <value>The decorations.</value>
    ///// <remarks>
    ///// Used in the dialog XAML.
    ///// </remarks>
    //[XmlIgnore]
    //public TextDecorationCollection Decorations
    //{
    //  get
    //  {
    //    TextDecorationCollection decorations = new TextDecorationCollection();
    //    if (Strikethrough)
    //    {
    //      decorations.Add(TextDecorations.Strikethrough[0]);
    //    }
    //    if (Underline)
    //    {
    //      decorations.Add(TextDecorations.Underline[0]);
    //    }
    //    if (Overline)
    //    {
    //      decorations.Add(TextDecorations.OverLine[0]);
    //    }
    //    if (Baseline)
    //    {
    //      decorations.Add(TextDecorations.Baseline[0]);
    //    }
    //    return decorations;
    //  }
    //  set
    //  {
    //    foreach (TextDecoration decoration in value)
    //    {
    //      if (decoration.Equals(TextDecorations.Strikethrough[0] as TextDecoration))
    //      {
    //        Strikethrough = true;
    //      }
    //      else if (decoration.Equals(TextDecorations.Underline[0] as TextDecoration))
    //      {
    //        Underline = true;
    //      }
    //      else if (decoration.Equals(TextDecorations.OverLine[0] as TextDecoration))
    //      {
    //        Overline = true;
    //      }
    //      else if (decoration.Equals(TextDecorations.Baseline[0] as TextDecoration))
    //      {
    //        Baseline = true;
    //      }
    //    }
    //  }
    //}
    #endregion Font Decorations
    ////////////////////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////////////////////
    #region Implementation of INotifyPropertyChanged

    /// <summary>
    ///   Proeprty changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Fires events for property changed
    /// </summary>
    /// <param name="p_strPropertyName"></param>
    protected void OnPropertyChanged(string p_strPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(
          this,
          new PropertyChangedEventArgs(p_strPropertyName));
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////
  }
}
