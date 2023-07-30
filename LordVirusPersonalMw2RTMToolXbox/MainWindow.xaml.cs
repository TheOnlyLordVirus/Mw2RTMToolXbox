using System.Windows;
using System.Windows.Input;

namespace LordVirusPersonalMw2RTMToolXbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    #region Shitty Window Manipulation
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private void CloseForm_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MinimizeForm_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = System.Windows.WindowState.Minimized;
    }
    #endregion

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        Internal_Connect();
    }

    private void ChangeClanNameButton_Click(object sender, RoutedEventArgs e)
    {
        Internal_SetClanName(ClanNameChangerTextBox.Text);
    }

    private void ChangeNameButton_Click(object sender, RoutedEventArgs e)
    {
        Internal_SetName(NameChangerTextBox.Text);
    }

    private void RealTimeNameChangeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_SetRealTimeNameChanging(true);
    }

    private void RealTimeNameChangeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Internal_SetRealTimeNameChanging(false);
    }

    private void RainbowCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        NameChangerTextBox.MaxLength -= 2;
    }

    private void RainbowCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        NameChangerTextBox.MaxLength += 2;
    }

    private void ButtonCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        NameChangerTextBox.MaxLength -= 2;
    }

    private void ButtonCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        NameChangerTextBox.MaxLength += 2;
    }

    private void LaserCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _xRPC.SetMemory(_laserAddress, _laserOn);
    }

    private void LaserCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        _xRPC.SetMemory(_laserAddress, _laserOff);
    }


    private void RedBoxCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _xRPC.SetMemory(_redBoxAddress, _redBoxOn);
    }

    private void RedBoxCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        _xRPC.SetMemory(_redBoxAddress, _redBoxOff);
    }
}
