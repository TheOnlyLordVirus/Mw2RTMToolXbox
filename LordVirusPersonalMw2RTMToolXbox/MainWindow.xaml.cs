using System.Windows;
using System.Windows.Input;

using XDRPCPlusPlus;

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

    }

    private void RainbowCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {

    }

    private void ButtonCheckBox_Checked(object sender, RoutedEventArgs e)
    {
    }

    private void ButtonCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
    }

    private void SpecialsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
    }

    private void SpecialsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
    }

    private void LaserCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        DevKit?.WriteByte(_laserAddress, _trueByte);
    }

    private void LaserCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        DevKit?.WriteByte(_laserAddress, _falseByte);
    }

    private void RedBoxCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _redBoxAddress, 
                (uint)_redBoxOn.Length,
                _redBoxOn,
                out _
            );
    }

    private void RedBoxCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _redBoxAddress, 
                (uint)_redBoxOff.Length, 
                _redBoxOff, 
                out _
            );
    }

    private void ThermalCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _thermalAddress, 
                (uint)_thermalOn.Length, 
                _thermalOn, 
            out _
            );
    }

    private void ThermalCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _thermalAddress, 
                (uint)_thermalOff.Length, 
                _thermalOff, 
                out _
            );
    }

    private void NoRecoilCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _noRecoilAddress,
                (uint)_noRecoilOn.Length,
                _noRecoilOn,
                out _
            );
    }

    private void NoRecoilCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        DevKit?.DebugTarget
            .SetMemory
            (
                _noRecoilAddress,
                (uint)_noRecoilOff.Length,
                _noRecoilOff,
                out _
            );
    }

    private void ProModCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("cg_fov 100;");
    }

    private void ProModCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("reset cg_fov;");
    }

    private void CartoonCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("r_fullbright 1;");
    }

    private void CartoonCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("r_fullbright 0;");
    }

    private void ChromeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("r_specularmap 2;");
    }

    private void ChromeCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("r_specularmap 0;");
    }

    private void UiDebugCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("ui_debugmode 1;");
    }

    private void UiDebugCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("ui_debugmode 0;");
    }

    private void FxCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("fx_enable 1;");
    }

    private void FxCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText("fx_enable 0;");
    }

    private void ChangePrestigeButton_Click(object sender, RoutedEventArgs e)
    {
        if (PrestigeIntegerUpDown.Value is null)
            return;

        Internal_SetPrestige((int)PrestigeIntegerUpDown.Value);
    }

    private void LoopPrestigeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_SetPrestigeLooping(true);
    }

    private void LoopPrestigeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Internal_SetPrestigeLooping(false);
    }

    private void ChangeLevelButton_Click(object sender, RoutedEventArgs e)
    {
        if (LevelIntegerUpDown.Value is null)
            return;

        Internal_SetLevel((int)LevelIntegerUpDown.Value);
    }

    private void LoopLevelCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Internal_SetLevelLooping(true);
    }

    private void LoopLevelCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Internal_SetLevelLooping(false);
    }

    private void CBuffAddTextButton_Click(object sender, RoutedEventArgs e)
    {
        Internal_CbufAddText(CBuffAddTextBox.Text);
    }

    private void SendGameServerCommandButton_Click(object sender, RoutedEventArgs e)
    {
        int client = -1; // TODO: Get current client from the drop box.

        Internal_GameSendServerCommand(client, 0, SendGameServerCommandTextBox);
    }

    private void EndGameButton_Click(object sender, RoutedEventArgs e)
    {
        int? number = DevKit?.ReadInt32(_nonHostEndGame);

        if (number is null)
            return;

        Internal_CbufAddText($"cmd mr {number} -1 endround;");
    }

    private void UnlockAllButton_Click(object sender, RoutedEventArgs e)
    {
        _ = Internal_UnlockAll();
    }
}
