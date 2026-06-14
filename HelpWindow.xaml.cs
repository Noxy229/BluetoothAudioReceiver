using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using BluetoothAudioReceiver.Services;

namespace BluetoothAudioReceiver;

public partial class HelpWindow : Window
{
    private readonly LocalizationService _loc = LocalizationService.Instance;

    public HelpWindow()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        Title = _loc["Help"];
        TitleTextBlock.Text = _loc["Help"];
        CloseButton.ToolTip = _loc["Close"];
        AutomationProperties.SetName(CloseButton, _loc["Close"]);

        QuickStartHeader.Text = _loc["QuickStart"];
        QuickStartStepsText.Text = _loc["QuickStartSteps"];
        KnownIssuesHeader.Text = _loc["KnownIssues"];
        NetworkSlowdownHeader.Text = _loc["NetworkSlowdown"];
        NetworkSlowdownDescText.Text = _loc["NetworkSlowdownDesc"];
        NetworkSolutionText.Text = $"{_loc["Solution"]}{Environment.NewLine}{_loc["NetworkSolution"]}";
        NoDevicesIssueHeader.Text = _loc["NoDevicesIssue"];
        NoDevicesSolutionText.Text = $"{_loc["Solution"]}{Environment.NewLine}{_loc["NoDevicesSolution"]}";
        ConnectionDropsHeader.Text = _loc["ConnectionDrops"];
        ConnectionDropsSolutionText.Text = $"{_loc["Solution"]}{Environment.NewLine}{_loc["ConnectionDropsSolution"]}";
        SystemRequirementsHeader.Text = _loc["SystemRequirements"];
        RequirementsText.Text = _loc["Requirements"];
        InfoHeader.Text = _loc["Info"];
        VersionInfoText.Text = $"{_loc["Version"]} 2.0.0{Environment.NewLine}{_loc["DevelopedWith"]}";
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
