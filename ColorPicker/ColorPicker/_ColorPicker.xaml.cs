using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorPicker
{
	/// <summary>
	/// Interaction logic for _ColorPicker.xaml
	/// </summary>
	public partial class _ColorPicker
	{
        #region Variables
        private static ValueBox _vbR;
        private static ValueBox _vbG;
        private static ValueBox _vbB;
        private static Point _mousePos;
        private static CopyBox _cbRGB;
        private static CopyBox _cbHEX;
        private static Border _currentColor;
        private static Border _lastColor;
        #endregion

        #region DependencyProperty declaration
        private static readonly DependencyProperty IsCurrentColorProperty = DependencyProperty.Register(               
            "IsCurrentColor", typeof(Color), typeof(_ColorPicker),
            new FrameworkPropertyMetadata(Color.FromRgb(255, 255, 255),
            new PropertyChangedCallback(OnIsCurrentColorChanged)));

        private static readonly RoutedEvent CurrentColorChangedEvent = EventManager.RegisterRoutedEvent(
            "CurrentColorChanged",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler), typeof(_ColorPicker));

        private static readonly DependencyProperty IsLastColorProperty = DependencyProperty.Register(
            "IsLastColor", typeof(Color), typeof(_ColorPicker),
            new FrameworkPropertyMetadata(Color.FromRgb(255, 255, 255),
            new PropertyChangedCallback(OnIsLastColorChanged))); 
       
        private static readonly RoutedEvent LastColorChangedEvent = EventManager.RegisterRoutedEvent(
            "LastColorChanged",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),typeof(_ColorPicker));
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public _ColorPicker()
		{
			this.InitializeComponent();     
            AddValueBoxes();
            AddCopyBoxes();
            AddColorBar();
            AddColorSelectorGradient();
        }
        #endregion

        #region Event wrappers
        /// <summary>
        /// Occurs when the current color changed.
        /// </summary>
        public event RoutedEventHandler CurrentColorChanged
        {
            add
            {
              AddHandler(CurrentColorChangedEvent, value);
            }
            remove { RemoveHandler(CurrentColorChangedEvent, value); }
        }
        /// <summary>
        /// Occurs when the last color changed.
        /// </summary>
        public event RoutedEventHandler LastColorChanged
        {
            add { AddHandler(LastColorChangedEvent, value); }
            remove { RemoveHandler(LastColorChangedEvent, value); }
        }
        #endregion

        #region Properties
        ///<summary>
         ///Gets the current selected Color.
         /// </summary>
        public Color IsCurrentColor
        {
            get { return (Color)GetValue(IsCurrentColorProperty); }
        }
        /// <summary>
        /// Gets the last Selected Color.
        /// </summary>
        public Color IsLastColor
        {
            get { return (Color)GetValue(IsLastColorProperty); }
        }
        /// <summary>
        /// Gets the R value of the current color.
        /// </summary>
        public int R
        {
            get { return (int)((Color)GetValue(IsCurrentColorProperty)).R; }
        }
        /// <summary>
        /// Gets the G value of the current color.
        /// </summary>
        public int G
        {
            get { return (int)((Color)GetValue(IsCurrentColorProperty)).G; }
        }
        /// <summary>
        /// Gets the B value of the current color.
        /// </summary>
        public int B
        {
            get { return (int)((Color)GetValue(IsCurrentColorProperty)).B; }
        }
        
        #endregion

        #region Methodes
        /// <summary>
        /// Adds the three value boxes to show thr RGB values.
        /// </summary>
        private void AddValueBoxes()
        {
            _vbR = new ValueBox();       
            _vbR.Height = 22;
            _vbR.HorizontalAlignment = HorizontalAlignment.Stretch;
            _vbR.VerticalAlignment = VerticalAlignment.Top;
            _vbR.Margin = new Thickness(20, 3, 0, 0);

            _vbG = new ValueBox();
            _vbG.Height = 22;
            _vbG.HorizontalAlignment = HorizontalAlignment.Stretch;
            _vbG.VerticalAlignment = VerticalAlignment.Top;
            _vbG.Margin = new Thickness(20, 29, 0, 0);

            _vbB = new ValueBox();  
            _vbB.Height = 22;
            _vbB.HorizontalAlignment = HorizontalAlignment.Stretch;
            _vbB.VerticalAlignment = VerticalAlignment.Top;
            _vbB.Margin = new Thickness(20, 55, 0, 0);

            _ColorInfoUnit.Children.Add(_vbR);
            _ColorInfoUnit.Children.Add(_vbG);
            _ColorInfoUnit.Children.Add(_vbB);              
        }
        /// <summary>
        /// Adds two CopyBoxes for RGB and HEX.
        /// </summary>
        private void AddCopyBoxes()
        {
            _cbRGB = new CopyBox();           
            _cbRGB.Height = 22;
            _cbRGB.HorizontalAlignment = HorizontalAlignment.Stretch;
            _cbRGB.VerticalAlignment = VerticalAlignment.Bottom;
            _cbRGB.Margin = new Thickness(5.5, 0, 0, 30);
            _cbRGB.Text = "255,255,255";

            _cbHEX = new CopyBox();           
            _cbHEX.Height = 22;
            _cbHEX.HorizontalAlignment = HorizontalAlignment.Stretch;
            _cbHEX.VerticalAlignment = VerticalAlignment.Bottom;
            _cbHEX.Margin = new Thickness(5.5, 0, 0, 0);
            _cbHEX.Text = "#FFFFFF";

            _ColorInfoUnit.Children.Add(_cbRGB);
            _ColorInfoUnit.Children.Add(_cbHEX);
            
        }
        /// <summary>
        /// Creates the Last and the Current color border.
        /// </summary>
        private void AddColorBar()
        {
            _currentColor = new Border();
            _currentColor.HorizontalAlignment = HorizontalAlignment.Stretch;
            _currentColor.VerticalAlignment = VerticalAlignment.Bottom;
            _currentColor.Margin = new Thickness(0, 0, 22, 0);
            _currentColor.BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            _currentColor.BorderThickness = new Thickness(1, 0, 0, 1);
            _currentColor.CornerRadius = new CornerRadius(3);
            _currentColor.Background = Brushes.White;
            _currentColor.Height = 24;

            _lastColor = new Border();
            _lastColor.HorizontalAlignment = HorizontalAlignment.Right;
            _lastColor.VerticalAlignment = VerticalAlignment.Bottom;
            _lastColor.Margin = new Thickness(0, 0, 0, 0);
            _lastColor.Width = 22;
            _lastColor.CornerRadius = new CornerRadius(3);
            _lastColor.BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            _lastColor.BorderThickness = new Thickness(1, 0, 1, 1);
            _lastColor.Background = Brushes.White;
            _lastColor.Height = 24;

            _ColorBarUnit.Children.Add(_currentColor);
            _ColorBarUnit.Children.Add(_lastColor);
        }
        /// <summary>
        /// Creates the gradient for the color selector.
        /// </summary>
        private void AddColorSelectorGradient()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.StartPoint = new Point(0.5, 0);
            linearGradientBrush.EndPoint = new Point(0.5, 1);
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0,     0), 0.020));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255,   0), 0.167));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255,   0, 255,   0), 0.334));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255,   0, 255, 255), 0.501));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255,   0,   0, 255), 0.668));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255,   0, 255), 0.835));
            linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255,   0,   0), 0.975));
            _colorSelector.Background = linearGradientBrush;            
        }
        /// <summary>
        /// Sets the value of the ValueBoxes.
        /// </summary>
        /// <param name="color"></param>
        private static void SetColorInfos(Color color)
        {
            _vbR.Value = color.R;
            _vbG.Value = color.G;
            _vbB.Value = color.B;
            _cbRGB.Text = color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString();
            string HexColor = string.Format("0x{0:X8}", color.ToString());
            _cbHEX.Text = "#" + HexColor.Remove(0, 5);            
        }
        /// <summary>
        /// Release the mouse when cliped.
        /// </summary>
        public void ReleaseMouse()
        {
            AdvancedMouse.CLPcursor.Release();
        }
        #endregion

        #region Events
        
        #region ColorSelector
        private void _ColorSelectorUnit_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Sets the new position of the color selector stylus.
                _stylus.Margin = new Thickness(0, e.GetPosition(_ColorSelectorUnit).Y - 4, 0, 0);
                //Sets the background of the base color.
                _BaseColor.Background = new SolidColorBrush(AdvancedMouse.MouseControling.PixelUnderMouse());
                //Gets the pixel color from the point mousePos.
                Color color = AdvancedMouse.MouseControling.PixelColor(_mousePos);               
                //Sets the current color property
                SetValue(IsCurrentColorProperty, color);
                //Sets the _currentColor.Background (_currentColor has been created of the methode AddColorBar ad is an border).
                _currentColor.Background = new SolidColorBrush(color);
            }
        }     

        private void _ColorSelectorUnit_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdvancedMouse.CLPcursor.OnUIElement(_colorSelectorCliping);
            _ColorSelectorUnit.Cursor = Cursors.None;
            //Gets a point which represents the center of the PickerStylus -> verry important because this is used on
            //ColorSelectorUnit_MouseMove to calculate the color.
            _mousePos = _PickerStylus.PointToScreen(new Point(7.5,7.5));
            //Sets the mouse in the centere of stylus.
            AdvancedMouse.MouseControling.SetOnUIElement(new Point(_stylus.Width / 2, (_stylus.Height / 2) - 1), _stylus);
        }

        private void _ColorSelectorUnit_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Release the mouse cliping.
            AdvancedMouse.CLPcursor.Release();
            //Sets the cursor to arrow.
            _ColorSelectorUnit.Cursor = Cursors.Arrow;
            //Sets the new position of the stylus.
            _stylus.Margin = new Thickness(0, e.GetPosition(_ColorSelectorUnit).Y - 4, 0, 0);
        }
        
        private void _ColorSelectorUnit_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {                
                //the same as on _ColorSelectorUnit_PreviewMouseLeftButtonDown
                AdvancedMouse.CLPcursor.OnUIElement(_colorSelectorCliping);                
                _ColorSelectorUnit.Cursor = Cursors.None;                
                _mousePos = _PickerStylus.PointToScreen(new Point(7.5, 7.5));
                AdvancedMouse.MouseControling.SetOnUIElement(new Point(_stylus.Width / 2, (_stylus.Height / 2) - 1), _stylus);
            }
        }
        #endregion
        
        #region ColorPicker
        private void _ColorPickerUnit_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {             
                //Sets the position of the PickerStylus.
                _PickerStylus.Margin = new Thickness(e.GetPosition(_colorPickerCliping).X - 7.5, e.GetPosition(_colorPickerCliping).Y - 7.5, 0, 0);
                //Sets the IsCurrentColorProperty to the color under the mouse.
                SetValue(IsCurrentColorProperty, AdvancedMouse.MouseControling.PixelUnderMouse());
            }
        }

        private void _ColorPickerUnit_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Clips the cursor to the _ColorPickerCliping which is an Canvas.
            AdvancedMouse.CLPcursor.OnUIElement(_colorPickerCliping);            
            //Sets the visibility of the _PickerStylus.
            _PickerStylus.Visibility = Visibility.Collapsed;
            //Sets the cursor of the _ColorPickerUnit to Pen.
            _ColorPickerUnit.Cursor = Cursors.Pen;                       
            //Sets the mouse position to the centere of the _PickerStylus.
            AdvancedMouse.MouseControling.SetOnUIElement(new Point(_PickerStylus.Width / 2, (_PickerStylus.Height / 2) - 1), _PickerStylus);
            //Sets the IsLastColorProperty.
            SetValue(IsLastColorProperty, IsCurrentColor);
            //_lastColor.Background which is an border gets the _currentColorBackground
            _lastColor.Background = _currentColor.Background;
        }

        private void _ColorPickerUnit_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Release the mouse cliping.
            AdvancedMouse.CLPcursor.Release();
            //Sets the cursor to arrow
            _ColorPickerUnit.Cursor = Cursors.Arrow;
            //Sets the position of the _PickerStylus.
            _PickerStylus.Margin = new Thickness(e.GetPosition(_colorPickerCliping).X - 7.5, e.GetPosition(_colorPickerCliping).Y - 7.5, 0, 0);
            //Sets the visibility of the _PickerStylus
            _PickerStylus.Visibility = Visibility.Visible;         
        }

        private void _ColorPickerUnit_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Clips the mouse to the _colorPicerCliping.
                AdvancedMouse.CLPcursor.OnUIElement(_colorPickerCliping);
                //Visibility of _PickerStylus
                _PickerStylus.Visibility = Visibility.Collapsed;
                //Sets the cursor of _ColorPickerUnit.
                _ColorPickerUnit.Cursor = Cursors.Pen;
                //Sets the mouse position to the centere of _PickerStylus
                AdvancedMouse.MouseControling.SetOnUIElement(new Point(_PickerStylus.Width / 2, (_PickerStylus.Height / 2) - 1), _PickerStylus);
            }
        }

        #endregion
        private static void OnIsCurrentColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //Calls SetColorInfo which takes an color. 
            SetColorInfos((Color)e.NewValue);
            //Sets the _currentColor.Background.
            _currentColor.Background = new SolidColorBrush((Color)e.NewValue);
            //If you use this Control and add an Event to LastColorChanged it would be thrown from here.
            _vbB.RaiseEvent(new RoutedEventArgs(CurrentColorChangedEvent, (Color)e.NewValue));
        }
        private static void OnIsLastColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //If you use this Control and add an Event to LastColorChanged it would be thrown from here.
            _vbB.RaiseEvent(new RoutedEventArgs(LastColorChangedEvent, (Color)e.NewValue));
        }
        #endregion       
    }
}