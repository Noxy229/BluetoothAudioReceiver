using System.Windows;
using System.Windows.Input;

namespace BluetoothAudioReceiver;

public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        DataContext = new { AppVersion = typeof(HelpWindow).Assembly.GetName().Version?.ToString(3) };
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
