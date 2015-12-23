using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Data;

namespace SyNet.GuiHelpers.Adorners
{
  /// <summary>
  /// A adorner class host a TextBox to enable editing and the TextBox lies in the AdornerLayer,
  /// when EditBox is in editable mode, arrange it with desired size; otherwise, arrange it with 
  /// size(0,0,0,0).
  /// </summary>
  internal sealed class EditBoxAdorner : Adorner
  {
    /// <summary>
    /// Inialize the EditBoxAdorner.
    /// </summary>
    public EditBoxAdorner(UIElement adornedElement, UIElement adorningElement)
      : base(adornedElement)
    {
      _textBox = adorningElement as TextBox;
      Debug.Assert(_textBox != null, "No TextBox!");

      _visualChildren = new VisualCollection(this);

      BuildTextBox();
    }    

    #region Public Methods

    /// <summary>
    /// Public method for EditBox to update staus of TextBox when IsEditing property is changed.
    /// </summary>
    /// <param name="isVisislbe"></param>
    public void UpdateVisibilty(bool isVisislbe)
    {
      _isVisible = isVisislbe;
      InvalidateMeasure();
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Override to measure elements.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
      _textBox.IsEnabled = _isVisible;
      //if in editable mode, measure the space the adorner element should cover.
      if (_isVisible)
      {
        AdornedElement.Measure(constraint);
        _textBox.Measure(constraint);

        //since the adorner is to cover the EditBox, it should return the AdornedElement.Width, 
        //the extra 15 is to make it more clear.
        return new Size(AdornedElement.DesiredSize.Width + _extraWidth, _textBox.DesiredSize.Height);
      }
      else  //if it is not in editable mode, no need to show anything.
        return new Size(0, 0);
    }

    /// <summary>
    /// override function to arrange elements.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
      if (_isVisible)
      {
        _textBox.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
      }
      else // if is not is editable mode, no need to show elements.
      {
        _textBox.Arrange(new Rect(0, 0, 0, 0));
      }
      return finalSize;
    }

    /// <summary>
    /// override property to return infomation about visual tree.
    /// </summary>
    protected override int VisualChildrenCount { get { return _visualChildren.Count; } }

    /// <summary>
    /// override function to return infomation about visual tree.
    /// </summary>
    protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Inialize necessary properties and hook necessary events on TextBox, then add it into tree.
    /// </summary>
    private void BuildTextBox()
    {
      _canvas = new Canvas();
      _canvas.Children.Add(_textBox);
      _visualChildren.Add(_canvas);

      //Bind Text onto AdornedElement.
      Binding binding = new Binding("Text");
      binding.Mode = BindingMode.TwoWay;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      binding.Source = this.AdornedElement;

      _textBox.SetBinding(TextBox.TextProperty, binding);

      //Update TextBox's focus status when layout finishs.
      _textBox.LayoutUpdated += new EventHandler(OnTextBoxLayoutUpdated);
    }

    /// <summary>
    /// When Layout finish, if in editable mode, update focus status on TextBox.
    /// </summary>
    private void OnTextBoxLayoutUpdated(object sender, EventArgs e)
    {
      if (_isVisible)
        _textBox.Focus();
    }

    #endregion

    #region Private Variables

    private VisualCollection _visualChildren; //visual children.
    private TextBox _textBox;  //The TextBox this adorner needs to cover.
    private bool _isVisible;   //whether is in editabl mode.
    private Canvas _canvas;    //canvas to contain the TextBox to enable it can expand out of cell.
    private const double _extraWidth = 15; //exstra space for TextBox to make the text in it clear.
 
    #endregion
  }
}