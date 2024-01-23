using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using XDRPCPlusPlus;
using XDevkit;

using Fody;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace LordVirusPersonalMw2RTMToolXbox;

[ConfigureAwait(false)]
public sealed partial class MainWindow
{
    internal sealed class G_ClientComboBoxItem : ComboBoxItem
    {
        public G_Client? Client;
    }

    public IXboxManager? XboxManager = null;
    public IXboxConsole? DevKit = null;

    private Random _random = new Random();

    private CancellationTokenSource? NameChangerCancellationTokenSource = null;
    private CancellationTokenSource? LevelCancellationTokenSource = null;
    private CancellationTokenSource? PrestigeCancellationTokenSource = null;

    private G_Client?[] CurrentGameClients = new G_Client?[_maxClientCount]; 

    private bool InGame
    {
        get
        {
            if (DevKit is null)
                return false;

            return Mw2GameFunctions.Cg_DvarGetBool(DevKit!, "cl_ingame");
        }
    }

    private void Internal_Connect()
    {
        try
        {
            XboxManager = new XboxManager();
            DevKit = XboxManager.OpenConsole(XboxManager.DefaultConsole);

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
            Mw2GameFunctions.Cg_DvarGetBool(DevKit!, "loc_warningsUI 0; loc_warnings 0; cg_blood 0; cg_bloodLimit 0;");

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
            NameChangerCancellationTokenSource?.Cancel();
            NameChangerCancellationTokenSource = null;

            PrestigeCancellationTokenSource?.Cancel();
            PrestigeCancellationTokenSource = null;

            LevelCancellationTokenSource?.Cancel();
            LevelCancellationTokenSource = null;

            MessageBox.Show
            (
                ex.Message,
                "Connect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    // TODO: Get this working!
    //private bool Internal_IsLocalClientInGame(int client = 0)
    //{
    //    return DevKit?.ExecuteRPC<bool>
    //    (
    //        new XDRPCExecutionOptions(XDRPCMode.Title, 0x82182990),
    //        new object[] { client }
    //    ) ?? false;
    //}

    private void Internal_UpdateCurrentUserInfo()
    {
        Span<byte> initNameByteSpan = DevKit?.ReadBytes(_nameAddress, _maxNameInputLength);
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

        Span<byte> initClanByteSpan = DevKit?.ReadBytes(_clanAddress, 4);
        initClanByteSpan = initClanByteSpan.TrimEnd((byte)0x00);
        ClanNameChangerTextBox.Text = Encoding.ASCII.GetString(initClanByteSpan);
    }

    private void Internal_RefreshClients()
    {
        if (DevKit is null)
            return;

        for (int clientIndex = 0; clientIndex < _maxClientCount; ++clientIndex) 
        {
            if (CurrentGameClients[clientIndex] is null)
            {
                CurrentGameClients[clientIndex] = new G_Client(DevKit!, clientIndex);

                ClientComboBox.Items.Add(new G_ClientComboBoxItem()
                {
                    Content = CurrentGameClients[clientIndex]?.ClientName,
                    Client = CurrentGameClients[clientIndex]
                });

                continue;
            }

            var item = (G_ClientComboBoxItem)ClientComboBox.Items[clientIndex];
            item.Content = item?.Client?.ClientName ?? string.Empty;
        }
    }

    private async Task Internal_UnlockAll(int client = -1)
    {
        if (DevKit is null)
            return;

        if (!InGame)
        {
            MessageBox.Show("You must be in game to unlock all!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "s loc_warningsUI 0; loc_warnings 0;");

        Mw2GameFunctions.iPrintLnBold(DevKit!, "^2Starting Challenges!", client);

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks0);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks1);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks2);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks3);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks4);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks5);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks6);
        Mw2GameFunctions.iPrintLn(DevKit!, "25 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4));

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks7);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks8);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks9);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks10);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks11);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks12);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks13);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks14);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks15);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks16);
        Mw2GameFunctions.iPrintLn(DevKit!, "50 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4));

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks17);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks18);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks19);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks20);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks21);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks22);
        Mw2GameFunctions.iPrintLn(DevKit!, "75 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4));

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks23);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks24);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks25);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks26);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks27);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks28);
        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, _unlocks29);
        Mw2GameFunctions.iPrintLn(DevKit!, "100 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        Mw2GameFunctions.iPrintLnBold(DevKit!, "^2Completed Challenges!", client);
    }

    private void Internal_SetPrestige(int prestige = 10)
    {
        if (prestige < _minLevel)
            return;

        if (prestige > _maxPrestige)
            return;

        byte[] prestigeBytes = BitConverter
            .GetBytes(prestige)
            .ToArray();

        DevKit?.DebugTarget
            .SetMemory
            (
                _prestigeAddress, 
                (uint)prestigeBytes.Length, 
                prestigeBytes, 
                out _
            );
    }

    private void Internal_SetLevel(int level = 70)
    {
        if (level < _minLevel)
            return;

        if (level > _maxLevel)
            return;

        byte[] levelBytes = BitConverter
            .GetBytes(_levelTable[level - 1])
            .ToArray();

        DevKit?.DebugTarget
            .SetMemory
            (
                _levelAddress, 
                (uint)levelBytes.Length,
                levelBytes,
                out _
            );
    }

    private void Internal_SetName(ReadOnlySpan<char> newName)
    {
        if (newName.Length > _maxNameInputLength)
            newName = newName[.._maxNameInputLength];

        Span<byte> nameBytes = stackalloc byte[_maxNameInputLength];
        Encoding.ASCII.GetBytes(newName, nameBytes);

        //if (InGame)
        //{
        //    Internal_CbufAddText($"userinfo \"\\clanabbrev\\^{_random.Next(6)}\\name\\{newName}\"");

        //    return;
        //}

        DevKit?.DebugTarget
            .SetMemory
            (
                _nameAddress,
                (uint)nameBytes.Length,
                nameBytes.ToArray(),
                out _
            );
    }

    private void Internal_SetClanName(ReadOnlySpan<char> newClanName)
    {
        if (newClanName.Length > 4)
            newClanName = newClanName[..4];

        Span<byte> clanNameBytes = stackalloc byte[4];
        Encoding.ASCII.GetBytes(newClanName, clanNameBytes);

        DevKit?.DebugTarget
            .SetMemory
            (
                _clanAddress, 
                (uint)clanNameBytes.Length,
                clanNameBytes.ToArray(),
                out _
            );
    }

    private void Internal_SetRealTimeNameChanging(bool toggleValue)
    {
        if(toggleValue)
        {
            NameChangerCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_AutoUpdateName(NameChangerCancellationTokenSource.Token);

            ChangeNameButton.IsEnabled = false;
            RainbowCheckBox.IsEnabled = true;
            ButtonCheckBox.IsEnabled = true;

            return;
        }

        if (NameChangerCancellationTokenSource is null)
            return;

        NameChangerCancellationTokenSource.Cancel();
        NameChangerCancellationTokenSource = null;

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
            LevelCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopLevels(LevelCancellationTokenSource.Token);

            LevelIntegerUpDown.IsEnabled = false;
            ChangeLevelButton.IsEnabled = false;

            return;
        }

        if (LevelCancellationTokenSource is null)
            return;

        LevelCancellationTokenSource.Cancel();
        LevelCancellationTokenSource = null;

        LevelIntegerUpDown.IsEnabled = true;
        ChangeLevelButton.IsEnabled = true;
    }

    private void Internal_SetPrestigeLooping(bool toggleValue)
    {
        if (toggleValue)
        {
            PrestigeCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopPrestiges(PrestigeCancellationTokenSource.Token);

            PrestigeIntegerUpDown.IsEnabled = false;
            ChangePrestigeButton.IsEnabled = false;

            return;
        }

        if (PrestigeCancellationTokenSource is null)
            return;

        PrestigeCancellationTokenSource.Cancel();
        PrestigeCancellationTokenSource = null;

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
            Internal_SetName(Internal_BuildAutoUpdatingNameString());

            TimeSpan delay = TimeSpan.FromMicroseconds(150);

            await Task
                .Delay(delay, cancellationToken)
                .ConfigureAwait(true);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopLevels(CancellationToken cancellationToken)
    {
        int level = 1;

        do
        {
            Internal_SetLevel(level++);

            level = level % (_maxLevel + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(70), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopPrestiges(CancellationToken cancellationToken)
    {
        int prestige = 1;

        do
        {
            Internal_SetPrestige(prestige++);

            prestige = prestige % (_maxPrestige + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }
}