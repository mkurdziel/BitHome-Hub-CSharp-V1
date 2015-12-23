using System.Windows.Controls;
using Trigger = SyNet.Events.Triggers.Trigger;

namespace SyNet.XamlPanels
{
  /// <summary>
  /// Interaction logic for MessageTriggerEditPanel.xaml
  /// </summary>
  public partial class MessageTriggerEditPanel : UserControl
  {
    private Trigger m_trigger;

    public MessageTriggerEditPanel(Trigger p_trigger)
    {
      m_trigger = p_trigger;

      //
      // Set to datacontext for binding
      //
      this.DataContext = m_trigger;

      InitializeComponent();

    }
  }
}
