using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SyNet.GuiControls
{
  /// <summary>
  ///   This class represents a color item that can be represented as a color 
  ///   object or a SolidColorBrush
  /// </summary>
  public class ColorItem
  {
    /// <summary>
    /// 
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    ///   Solid color brush representation of the color object
    /// </summary>
    public SolidColorBrush Brush
    {
      get { return new SolidColorBrush(Color); }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    /// <param name="p_color"></param>
    public ColorItem( Color p_color )
    {
      Color = p_color;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Color.ToString();
    }

  }

  /// <summary>
  ///   Custom color combo box item class
  /// </summary>
  /// <remarks>
  ///   Necessary to access the highlight ability
  /// </remarks>
  public class ColorComboBoxItem : ComboBoxItem
  {

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ColorComboBoxItem()
    {
      IsSelectionSuppressed = false;
    }

    /// <summary>
    ///   Accesses the base property to unhighlight the item
    /// </summary>
    /// <remarks>
    ///   This is necessary because the IsHighlighted property of the
    ///   built-in ComboBoxItem is private.
    /// </remarks>
    public void Unhighlight()
    {
      IsHighlighted = false;
    }


    /// <summary>
    ///   Dependency property so the XAML template can use this property
    ///   to determine whether or not to highlight the color object
    /// </summary>
    public static readonly DependencyProperty IsSelectionSuppressedProperty =
      DependencyProperty.Register("IsSelectionSuppressed",
                                  typeof(Boolean), typeof(ColorComboBoxItem));

    /// <summary>
    ///   This allows the highlight selection of the object to be suppressed.
    /// </summary>
    /// <remarks>
    ///   This is necessary beacuse the selected item of a combobox that is
    ///   based on binding, cannot be unselected.  Therefore, the combobox
    ///   must always have some item selected.  If we select the custom color
    ///   in our custom combobox, there will still be a selected color in the
    ///   combobox items, effectively portraying two selected colors to the
    ///   user.  By supressing the selection, we can leave a combobox item
    ///   selected but still fake the user into thinking that it is not.
    /// </remarks>
    public bool IsSelectionSuppressed
    {
      get { return (bool)GetValue(IsSelectionSuppressedProperty); }
      set { SetValue(IsSelectionSuppressedProperty, value); }
    }
  }

  /// <summary>
  ///   Custom color combo box class
  /// </summary>
  /// <remarks>
  ///   Necessary to manipulate the highlighting of the combo box items
  /// </remarks>
  public class ColorComboBox : ComboBox, INotifyPropertyChanged
  {
    private Color m_selectedColor;

    // Store a reference to the custom color swatch so that we don't have to
    // search the visual tree everytime we want to access it
    private Button m_customColorButtonSwatch;

    //
    // These are the default colors that show up as options in the combobox.
    // They are in groups of 3 because the combobox is sized to show in groups
    // of three.
    //
    private readonly Color[] m_defaultColors = {
                                                 Colors.Black,
                                                 Colors.DarkRed,
                                                 Colors.Green,
                                                 Colors.Olive,

                                                 Colors.Navy,
                                                 Colors.Purple,
                                                 Colors.Teal,
                                                 Colors.Silver,

                                                 Colors.DarkSeaGreen,
                                                 Colors.LightSkyBlue,
                                                 Colors.Ivory,
                                                 Colors.SlateGray,

                                                 Colors.Gray,
                                                 Colors.Red,
                                                 Colors.Lime,
                                                 Colors.Yellow,

                                                 Colors.MediumBlue,
                                                 Colors.Magenta,
                                                 Colors.Cyan,
                                                 Colors.White
                                               };

    /// <summary>
    ///   Gets a boolean of whether a custom color is selected
    /// </summary>
    public bool IsCustom
    {
      get { return (GetComboBoxIndex(SelectedColor) == -1); }
    }

    /// <summary>
    ///   Gets whether the custom color swatch should be visible
    /// </summary>
    public Visibility CustomVisibility
    {
      get
      {
        return IsCustom
                 ? Visibility.Visible
                 : Visibility.Collapsed;
      }
    }

    /// <summary>
    ///   Gets or sets the selected color of the combobox
    /// </summary>
    public Color SelectedColor
    {
      get { return m_selectedColor; }
      set
      {

        m_selectedColor = value;
        SetComboBoxToSelected();

        // These properties are all dependent on the selected color value
        // so we need to notify them all when the selected color changes
        OnPropertyChanged("SelectedColor");
        OnPropertyChanged("SelectedColorBrush");
        OnPropertyChanged("CustomVisibility");
      }
    }

    /// <summary>
    ///   Gets a SolidColorBrush representation of the selected color.
    /// </summary>
    /// <remarks>
    ///   This is necessary beacuse certain components in the XAML styles
    ///   (such as the selected color border background in the combobox)
    ///   need to be bound to the brush representation of the selected color.
    /// </remarks>
    public SolidColorBrush SelectedColorBrush
    {
      get { return new SolidColorBrush(SelectedColor); }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ColorComboBox()
    {
      Loaded += ColorComboBox_Loaded;
      SelectedColor = Colors.Black;
    }

    /// <summary>
    ///   Constructor with initial selected color.
    /// </summary>
    /// <param name="p_defaultColor">Color to set as selected color.</param>
    public ColorComboBox( Color p_defaultColor )
      : this()
    {
      SelectedColor = p_defaultColor;
    }

    /// <summary>
    ///   Combobox selected changed event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    void ColorComboBox_SelectionChanged( object p_sender, SelectionChangedEventArgs p_e )
    {
      ColorItem colorItem = SelectedItem as ColorItem;
      if (colorItem != null)
      {
        SelectedColor = colorItem.Color;
      }
    }

    /// <summary>
    ///   Combobox DropDownOpened event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    void ColorComboBox_DropDownOpened( object p_sender, EventArgs p_e )
    {
      //
      // If there is a custom color selected, we need to set the 
      // focus (highlighting) of the custom color swatch and supress the
      // selection (highlighting) of the combo box items
      //
      // If there is not a custom color selected, we need to remove 
      // supression on the combobox items so the proper one can be highlighted
      //
      if (IsCustom)
      {
        FocusCustomButton();
        SetSelectionSuppressed(true);
      }
      else
      {
        SetSelectionSuppressed(false);
      }
    }

    /// <summary>
    ///   Combobox loaded event handler
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    void ColorComboBox_Loaded( object p_sender, RoutedEventArgs p_e )
    {
      DropDownOpened += ColorComboBox_DropDownOpened;
      SelectionChanged += ColorComboBox_SelectionChanged;

      // Get the custom color button swatch from the template
      m_customColorButtonSwatch = Template.FindName("m_CustomColorSwatch", this)
                                  as Button;
      if (m_customColorButtonSwatch != null)
      {
        m_customColorButtonSwatch.PreviewMouseMove +=
          CustomColorButton_MouseMove;
        m_customColorButtonSwatch.Click += CustomColorSwatch_Click;
      }

      // Get the "other.." button to wire up the click
      Button customColorButton =
        Template.FindName("m_customColorSelectorButton", this) as Button;
      if (customColorButton != null)
      {
        customColorButton.Click += CustomColorButton_Click;
      }


      // Add the default colors to the box
      AddDefaultColors();

      // Set the selected color
      SetComboBoxToSelected();
    }


    /// <summary>
    ///   Override to populate the combo box with custom combo box items
    /// </summary>
    /// <returns>
    ///   This is necessary because we need to access the IsHighlighted
    ///   property of the comboboxitem.
    /// </returns>
    protected override DependencyObject GetContainerForItemOverride()
    {
      ColorComboBoxItem item = new ColorComboBoxItem();

      // Add the mouse move event handler to each combobox item so we can
      // manipulate the highlighting between the standard combobox items
      // and the custom color swatch
      item.AddHandler(MouseMoveEvent,
                      new RoutedEventHandler(ItemMouseOver));
      return item;
    }

    /// <summary>
    ///   Iterates through the internal default color list and adds
    ///   the colors to the combo box.
    /// </summary>
    private void AddDefaultColors()
    {
      Items.Clear();

      foreach (Color color in m_defaultColors)
      {
        AddColor(color);
      }
    }

    /// <summary>
    ///   Add a single color to the combo box.
    /// </summary>
    /// <param name="p_color"></param>
    private void AddColor( Color p_color )
    {
      Items.Add(new ColorItem(p_color));
    }

    /// <summary>
    ///   Sets the combobox to the selected color
    /// </summary>
    private void SetComboBoxToSelected()
    {
      int idx = GetComboBoxIndex(SelectedColor);
      if (idx != -1)
      {
        SelectedIndex = idx;
      }
    }

    /// <summary>
    ///   Compares the ARGB values of two colors
    /// </summary>
    /// <param name="p_color1"></param>
    /// <param name="p_color2"></param>
    /// <returns></returns>
    private static bool AreColorsEqual( Color p_color1, Color p_color2 )
    {
      return (p_color1.A == p_color2.A &&
              p_color1.R == p_color2.R &&
              p_color1.G == p_color2.G &&
              p_color1.B == p_color2.B);
    }

    /// <summary>
    ///   Gets the index of a particular color in the combo box.
    /// </summary>
    /// <param name="p_color">Color to search for</param>
    /// <returns>The combobox item index if found or -1 if not found</returns>
    private int GetComboBoxIndex( Color p_color )
    {
      int retVal = -1;
      for (int i = 0; i < Items.Count; i++)
      {
        ColorItem colorItem = Items[i] as ColorItem;
        if (colorItem != null)
        {
          if (AreColorsEqual(colorItem.Color, p_color))
          {
            retVal = i;
            break;
          }
        }
      }
      return retVal;
    }

    /// <summary>
    ///   Uses the mouse handlers to grab the last potentially highlighted
    ///   combo box item.
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_args"></param>
    private void ItemMouseOver( object p_sender, RoutedEventArgs p_args )
    {
      ColorComboBoxItem item = p_sender as ColorComboBoxItem;
      if (item != null)
      {
        // If the item has the mouse over it, we don't want to suppress the
        // selection. Highlighting occurs when either the combobox item 
        // is selected or the mouse is over the item, and we only want to
        // supress the "combobox opens and an item is selected" highlighting.
        item.IsSelectionSuppressed = false;

        // By doing this, we're potentially removing focus from the custom
        // color swatch, which will give the visual appearance of removing
        // it's highlighting.  All this is necessary to only highlight one
        // color swatch at a time.
        if (m_customColorButtonSwatch != null)
        {
          FocusManager.SetFocusedElement(this, item);
        }
      }
    }

    /// <summary>
    ///   Sets focus to the custom color swatch
    /// </summary>
    /// <remarks>
    ///   By setting focus to the custom color swatch, it gives the appearance 
    ///   of being selected or highlighted.
    /// </remarks>
    public void FocusCustomButton()
    {
      if (m_customColorButtonSwatch != null)
      {
        m_customColorButtonSwatch.Focusable = true;
        FocusManager.SetFocusedElement(this, m_customColorButtonSwatch);
      }
    }

    /// <summary>
    ///   Clears the highlighting of all comboboxitems in the combobox.
    /// </summary>
    /// <remarks>
    ///   This is used mainly when the mouse is over the custom color swatch,
    ///   and therefore want only that swatch to be highlighted. Since
    ///   the custom color swatch is not part of the combobox items, the
    ///   "highlight only one item" does not apply to this custom swatch.
    /// </remarks>
    public void ClearHightlighting()
    {
      foreach (var item in Items)
      {
        ColorItem colorItem = item as ColorItem;
        if (colorItem != null)
        {
          // The visual comboboxitems are generated by the data template
          // in the xaml file, so we must access those items instead of just 
          // the combobox.items collection
          ColorComboBoxItem cbi =
            (ColorComboBoxItem)
            ItemContainerGenerator.ContainerFromItem(colorItem);
          if (cbi != null)
          {
            cbi.Unhighlight();
          }
        }
      }
    }

    /// <summary>
    ///   Clears the selected combobox item.
    /// </summary>
    public void SetSelectionSuppressed( bool p_suppression )
    {
      foreach (var item in Items)
      {
        ColorItem colorItem = item as ColorItem;
        if (colorItem != null)
        {
          ColorComboBoxItem cbi =
            (ColorComboBoxItem)
            ItemContainerGenerator.ContainerFromItem(colorItem);
          if (cbi != null)
          {
            cbi.IsSelectionSuppressed = p_suppression;
          }
        }
      }
    }


    /// <summary>
    ///   Custom color swatch click event handler
    /// </summary>
    /// <remarks>
    ///   When the custom color swatch is clicked, that color has already been
    ///   selected so all we need to do is close the drop down.
    /// </remarks>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void CustomColorSwatch_Click( object p_sender, RoutedEventArgs p_e )
    {
      IsDropDownOpen = false;
    }

    /// <summary>
    ///   Mouse handler for the custom color button that clears any highlighting
    ///   on the other combo box color items.
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void CustomColorButton_MouseMove( object p_sender, MouseEventArgs p_e )
    {
      ClearHightlighting();
    }

    /// <summary>
    ///   Custom color button click event handler. This brings up a color
    ///   dialog to select a custom color.
    /// </summary>
    /// <param name="p_sender"></param>
    /// <param name="p_e"></param>
    private void CustomColorButton_Click( object p_sender, RoutedEventArgs p_e )
    {
      ColorDialog colorDialog = new ColorDialog
                                  {
                                    Color =
                                      System.Drawing.Color.FromArgb(
                                      SelectedColor.A,
                                      SelectedColor.R, 
                                      SelectedColor.G,
                                      SelectedColor.B),
                                    FullOpen = true
                                  };

      // Show the dialog, and if OK is pressed
      if (colorDialog.ShowDialog() == DialogResult.OK)
      {
        SelectedColor = Color.FromArgb(colorDialog.Color.A,
                                       colorDialog.Color.R,
                                       colorDialog.Color.G,
                                       colorDialog.Color.B);
        IsDropDownOpen = false;
      }
    }

    /// <summary>
    ///   Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Property changed event helper
    /// </summary>
    /// <param name="p_propertyName"></param>
    protected void OnPropertyChanged( string p_propertyName )
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(p_propertyName));
    }
  }

  /// <summary>
  ///   Custom combo box used for picking colors.
  /// </summary>
  public partial class ColorPickerComboBox : INotifyPropertyChanged
  {

    /// <summary>
    ///   Gets or sets the selected color of the color picker combo box
    /// </summary>
    public Color SelectedColor
    {
      get
      {
        return x_colorComboBox.SelectedColor;
      }
      set
      {
        x_colorComboBox.SelectedColor = value;
        SetValue(SelectedColorProperty, new SolidColorBrush(value));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty SelectedColorProperty =
      DependencyProperty.Register("SelectedColor",
                                  typeof(SolidColorBrush), typeof(ColorComboBox),
                                  new UIPropertyMetadata(new SolidColorBrush(Colors.Black),
                                                         SelectedColorChanged_CallBack ));


    private static void SelectedColorChanged_CallBack( DependencyObject p_d,
                                                       DependencyPropertyChangedEventArgs p_e )
    {
      ColorComboBox combobox = p_d as ColorComboBox;
      if (combobox != null)
      {
        combobox.SelectedColor = ((SolidColorBrush) p_e.NewValue).Color;
      }
    }

    /// <summary>
    ///   Default constructor
    /// </summary>
    public ColorPickerComboBox()
    {
      InitializeComponent();
      this.x_colorComboBox.PropertyChanged += ColorComboBox_PropertyChanged;
    }

    void ColorComboBox_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if (e.PropertyName == "SelectedColor")
      {
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs(e.PropertyName));
        }
      }
    }

    #region Implementation of INotifyPropertyChanged

    /// <summary>
    ///   Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}