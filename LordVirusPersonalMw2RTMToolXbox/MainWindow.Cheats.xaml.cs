using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Fody;

using XDRPCPlusPlus;
using XDevkit;

using LordVirusMw2XboxLib;

namespace LordVirusPersonalMw2RTMToolXbox;

[ConfigureAwait(false)]
public sealed partial class MainWindow
{
    private IXboxManager? xboxManager = null;
    private IXboxConsole? devKit = null;

    private readonly Random _random = new Random();

    private CancellationTokenSource? nameChangerCancellationTokenSource = null;
    private CancellationTokenSource? levelCancellationTokenSource = null;
    private CancellationTokenSource? prestigeCancellationTokenSource = null;

    private readonly Task?[] _unlockAllTasks = new Task?[_maxClientCount];
    private readonly G_Client?[] _currentGameClients = new G_Client?[_maxClientCount];

    private G_Client? SelectedClient
    {
        get
        {
            if (ClientComboBox.SelectedValue is not G_ClientComboBoxItem g_ClientComboBox)
                return null;

            return g_ClientComboBox.Client;
        }
    }

    private void Internal_Connect()
    {
        try
        {
            xboxManager = new XboxManager();
            devKit = xboxManager.OpenConsole(xboxManager.DefaultConsole);

            LaserCheckBox.IsEnabled = true;
            RedBoxCheckBox.IsEnabled = true;
            ThermalCheckBox.IsEnabled = true;
            NoRecoilCheckBox.IsEnabled = true;
            ProModCheckBox.IsEnabled = true;
            CartoonCheckBox.IsEnabled = true;
            ChromeCheckBox.IsEnabled = true;
            UiDebugCheckBox.IsEnabled = true;
            FxCheckBox.IsEnabled = true;

            NameChangerTextBox.IsEnabled = true;
            ChangeNameButton.IsEnabled = true;
            RealTimeNameChangeCheckBox.IsEnabled = true;

            ClanNameChangerTextBox.IsEnabled = true;
            ChangeClanNameButton.IsEnabled = true;

            PrestigeIntegerUpDown.Value = 10;
            PrestigeIntegerUpDown.Maximum = _maxPrestige;
            PrestigeIntegerUpDown.Minimum = _minPrestige;
            PrestigeIntegerUpDown.IsEnabled = true;
            ChangePrestigeButton.IsEnabled = true;
            LoopPrestigeCheckBox.IsEnabled = true;

            LevelIntegerUpDown.Value = _maxLevel;
            LevelIntegerUpDown.Maximum = _maxLevel;
            LevelIntegerUpDown.Minimum = _minLevel;
            LevelIntegerUpDown.IsEnabled = true;
            ChangeLevelButton.IsEnabled = true;
            LoopLevelCheckBox.IsEnabled = true;

            CBuffAddTextBox.IsEnabled = true;
            CBuffAddTextButton.IsEnabled = true;

            SendGameServerCommandTextBox.IsEnabled = true;
            SendGameServerCommandButton.IsEnabled = true;

            UnlockAllButton.IsEnabled = true;
            EndGameButton.IsEnabled = true;

            ClientComboBox.IsEnabled = true;

            Internal_UpdateCurrentUserInfo();
            Mw2GameFunctions.Cbuf_AddText(devKit!, "loc_warningsUI 0; loc_warnings 0;");

            MessageBox.Show
            (
                "Successfuly Connected!",
                "Connection",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        catch (Exception ex)
        {
            nameChangerCancellationTokenSource?.Cancel();
            nameChangerCancellationTokenSource = null;

            prestigeCancellationTokenSource?.Cancel();
            prestigeCancellationTokenSource = null;

            levelCancellationTokenSource?.Cancel();
            levelCancellationTokenSource = null;

            MessageBox.Show
            (
                ex.Message,
                "Connect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void Internal_UpdateCurrentUserInfo()
    {
        Span<byte> initNameByteSpan = devKit?.ReadBytes(Mw2XboxLibConstants.NameAddress, _maxNameInputLength);
        initNameByteSpan = initNameByteSpan.TrimEnd((byte)0x00);

        Span<char> nameChars = stackalloc char[initNameByteSpan.Length];

        Encoding.ASCII.GetChars(initNameByteSpan, nameChars);

        var stringBuilder = new StringBuilder(nameChars.Length);

        bool setColorCodeNextLoop = false;
        foreach (char c in nameChars)
        {
            if (c == '^')
            {
                stringBuilder.Append(c);
                setColorCodeNextLoop = true;
                continue;
            }

            if (setColorCodeNextLoop && c.IsInt())
            {
                stringBuilder.Append('F');
                setColorCodeNextLoop = false;
                continue;
            }

            if (_buttonCharMap.Contains(c))
            {
                stringBuilder.Append("^B");
                continue;
            }

            stringBuilder.Append(c);
        }

        NameChangerTextBox.Text = stringBuilder.ToString();

        Span<byte> initClanByteSpan = devKit?.ReadBytes(Mw2XboxLibConstants.ClanAddress, 4);
        initClanByteSpan = initClanByteSpan.TrimEnd((byte)0x00);
        ClanNameChangerTextBox.Text = Encoding.ASCII.GetString(initClanByteSpan);
    }

    private void Internal_RefreshClients()
    {
        if (devKit is null)
            return;

        for (int clientIndex = 0; clientIndex < _maxClientCount; ++clientIndex) 
        {
            if (_currentGameClients[clientIndex] is null)
            {
                _currentGameClients[clientIndex] = new G_Client(devKit!, clientIndex);

                ClientComboBox.Items.Add(new G_ClientComboBoxItem()
                {
                    Content = _currentGameClients[clientIndex]?.ClientName,
                    Client = _currentGameClients[clientIndex]
                });

                continue;
            }

            if (ClientComboBox.Items[clientIndex] is not G_ClientComboBoxItem g_ClientComboBoxItem)
                continue;

            g_ClientComboBoxItem.Content = g_ClientComboBoxItem.Client?.ClientName ?? string.Empty;
        }
    }

    private void Internal_SetRealTimeNameChanging(bool toggleValue)
    {
        if(toggleValue)
        {
            nameChangerCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_AutoUpdateName(nameChangerCancellationTokenSource.Token);

            ChangeNameButton.IsEnabled = false;
            RainbowCheckBox.IsEnabled = true;
            ButtonCheckBox.IsEnabled = true;

            return;
        }

        if (nameChangerCancellationTokenSource is null)
            return;

        nameChangerCancellationTokenSource.Cancel();
        nameChangerCancellationTokenSource = null;

        ChangeNameButton.IsEnabled = true;

        RainbowCheckBox.IsEnabled = false;
        RainbowCheckBox.IsChecked = false;

        ButtonCheckBox.IsEnabled = false;
        ButtonCheckBox.IsChecked = false;

        NameChangerTextBox.MaxLength = _maxNameInputLength;
    }

    private void Internal_SetLevelLooping(bool toggleValue)
    {
        if (toggleValue)
        {
            levelCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopLevels(levelCancellationTokenSource.Token);

            LevelIntegerUpDown.IsEnabled = false;
            ChangeLevelButton.IsEnabled = false;

            return;
        }

        if (levelCancellationTokenSource is null)
            return;

        levelCancellationTokenSource.Cancel();
        levelCancellationTokenSource = null;

        LevelIntegerUpDown.IsEnabled = true;
        ChangeLevelButton.IsEnabled = true;
    }

    private void Internal_SetPrestigeLooping(bool toggleValue)
    {
        if (toggleValue)
        {
            prestigeCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopPrestiges(prestigeCancellationTokenSource.Token);

            PrestigeIntegerUpDown.IsEnabled = false;
            ChangePrestigeButton.IsEnabled = false;

            return;
        }

        if (prestigeCancellationTokenSource is null)
            return;

        prestigeCancellationTokenSource.Cancel();
        prestigeCancellationTokenSource = null;

        PrestigeIntegerUpDown.IsEnabled = true;
        ChangePrestigeButton.IsEnabled = true;
    }

    private ReadOnlySpan<char> Internal_BuildAutoUpdatingNameString()
    {
        byte tempMaxNameInputLength = _maxNameInputLength;

        ReadOnlySpan<char> newNameBuffer = NameChangerTextBox.Text;

        if (newNameBuffer.Length < _maxNameInputLength)
            goto ParseSpan;

        for (int i = 0; i < newNameBuffer.Length; ++i)
        {
            ++i;
            if (!(newNameBuffer[i - 1] == '^' &&
                (newNameBuffer[i] == 'B' ||
                    newNameBuffer[i] == 'b')))
                continue;

            tempMaxNameInputLength++;
        }

        // Slice to buffer length;
        newNameBuffer = newNameBuffer[..tempMaxNameInputLength];

        ParseSpan:
        if (RainbowCheckBox.IsChecked ?? false)
            Internal_ParseFlashingCodes(newNameBuffer, out newNameBuffer);

        if (ButtonCheckBox.IsChecked ?? false)
            Internal_ParseButtonCodes(newNameBuffer, out newNameBuffer);

        return newNameBuffer;
    }

    private void Internal_ParseFlashingCodes(ReadOnlySpan<char> name, out ReadOnlySpan<char> newName)
    {
        Span<char> tempName = stackalloc char[name.Length];
        name.CopyTo(tempName);

        int flashIndex = name.IndexOf("^F", StringComparison.OrdinalIgnoreCase);
        while (flashIndex > -1)
        {
            tempName[flashIndex] = '^'; flashIndex++;
            tempName[flashIndex] = (char)(_random.Next(6) + 48); flashIndex++;

            var newflashIndex = name[flashIndex..].IndexOf("^F", StringComparison.OrdinalIgnoreCase);

            if (newflashIndex == -1)
                break;

            flashIndex = newflashIndex + flashIndex;
        }

        newName = tempName.ToArray();
    }

    private void Internal_ParseButtonCodes(ReadOnlySpan<char> inputName, out ReadOnlySpan<char> newName)
    {
        Span<char> tempName = stackalloc char[inputName.Length];
        inputName.CopyTo(tempName);

        int buttonIndex = inputName.IndexOf("^B", StringComparison.OrdinalIgnoreCase);
        int removalCount = 0;
        while (buttonIndex > -1)
        {
            // Generate random button char.
            tempName[buttonIndex - removalCount] = _buttonCharMap[_random.Next(_buttonCharMap.Length)]; 

            Span<char> tempNameSlice = tempName[(buttonIndex - removalCount + 2)..].ToArray();
            tempNameSlice.TryCopyTo(tempName[(buttonIndex - removalCount + 1)..]);

            removalCount++;

            if (inputName.Length < buttonIndex + 2)
                break;

            int tempIndex = buttonIndex + 2;
            buttonIndex = inputName[tempIndex..].IndexOf("^B", StringComparison.OrdinalIgnoreCase);

            if (buttonIndex == -1)
                break;

            buttonIndex += tempIndex;
        }

        newName = tempName[..(tempName.Length - removalCount)].ToArray();
    }

    private async Task Internal_AutoUpdateName(CancellationToken cancellationToken)
    {
        do
        {
            if (devKit is null)
                break;

            Mw2GameFunctions.SetName(devKit!, Internal_BuildAutoUpdatingNameString());

            await Task
                .Delay(TimeSpan.FromMilliseconds(150), cancellationToken)
                .ConfigureAwait(true);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopLevels(CancellationToken cancellationToken)
    {
        int level = 1;

        do
        {
            if (devKit is null)
                break;

            Mw2GameFunctions.SetLevel(devKit!, level++);

            level %= (_maxLevel + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(70), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopPrestiges(CancellationToken cancellationToken)
    {
        int prestige = 1;

        do
        {
            if (devKit is null)
                break;

            Mw2GameFunctions.SetPrestige(devKit!, prestige++);

            prestige %= (_maxPrestige + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }
}