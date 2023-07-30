using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using XRPCLib;

namespace LordVirusPersonalMw2RTMToolXbox;

public sealed partial class MainWindow
{
    private Random _random = new Random();
    private CancellationTokenSource? NameChangerCancellationTokenSource;
    private XRPC _xRPC = new XRPC();

    private const UInt32 _nameAddress = 0x838BA824;
    private const UInt32 _clanAddress = 0x82687060;

    private const UInt32 _laserAddress = 0x82104093;
    private static readonly byte[] _laserOn = new byte[] { 0x01 };
    private static readonly byte[] _laserOff = new byte[] { 0x00 };

    private const UInt32 _redBoxAddress = 0x820F4234;
    private static readonly byte[] _redBoxOn = new byte[] { 0x60, 0x00 }; // NOP
    private static readonly byte[] _redBoxOff = new byte[] { 0x41, 0x9A };

    private static readonly char[,] _buttonCodes = new char[8, 2]
    {
        { '', '' }, 
        { '', '' }, 
        { '', '' }, 
        { '', '' },
        { '', '' },
        { '', '' },
        { '', '' },
        { '', '' }
    };

    private static readonly char[] _colorCharMap = new char[6]
    {
        '1',
        '2',
        '3',
        '4',
        '5',
        '6'
    };

    private void Internal_Connect()
    {
        try
        {
            _xRPC.Connect();

            NameChangerTextBox.IsEnabled = true;
            ChangeNameButton.IsEnabled = true;
            RealTimeNameChangeCheckBox.IsEnabled = true;
            LaserCheckBox.IsEnabled = true;
            RedBoxCheckBox.IsEnabled = true;
            ClanNameChangerTextBox.IsEnabled = true;
            ChangeClanNameButton.IsEnabled = true;

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

            MessageBox.Show
            (
                ex.Message,
                "Connect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void Internal_SetName(ReadOnlySpan<char> newName)
    {
        if (newName.Length > 32)
            newName = newName[..32];

        Span<byte> nameBytes = stackalloc byte[32];
        Encoding.ASCII.GetBytes(newName, nameBytes);

        _xRPC.SetMemory(_nameAddress, nameBytes.ToArray());
    }

    private void Internal_SetClanName(ReadOnlySpan<char> newClanName)
    {
        if (newClanName.Length > 4)
            newClanName = newClanName[..4];

        Span<byte> clanNameBytes = stackalloc byte[4];
        Encoding.ASCII.GetBytes(newClanName, clanNameBytes);

        _xRPC.SetMemory(_clanAddress, clanNameBytes.ToArray());
    }

    private void Internal_SetRealTimeNameChanging(bool toggleValue)
    {
        if(toggleValue)
        {
            NameChangerCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_AutoUpdateName(NameChangerCancellationTokenSource.Token)
                .ConfigureAwait(false);

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

        NameChangerTextBox.MaxLength = 32;
    }

    private ReadOnlySpan<char> Internal_BuildNameString()
    {
        byte maxNameInputLength = 32;

        if (RainbowCheckBox.IsChecked ?? false)
            maxNameInputLength -= 2;

        if (ButtonCheckBox.IsChecked ?? false)
            maxNameInputLength -= 2;

        ReadOnlySpan<char> inputChars = 
            (NameChangerTextBox.Text.Length > maxNameInputLength) ? 
                NameChangerTextBox.Text[..maxNameInputLength] : NameChangerTextBox.Text;

        if ((!RainbowCheckBox.IsChecked ?? false) && (!ButtonCheckBox.IsChecked ?? false))
            return inputChars.ToString();

        byte index = 0;
        Span<char> newName = stackalloc char[32];

        if (RainbowCheckBox.IsChecked ?? false)
        {
            newName[index] = '^'; index++;
            newName[index] = _colorCharMap[_random.Next(6)]; index++;
        }

        if (!(ButtonCheckBox.IsChecked ?? false))
        {
            inputChars.CopyTo(newName[index..]);

            return newName.ToString();
        }

        int buttonIndex = _random.Next(8);

        newName[index] = _buttonCodes[buttonIndex, 0]; index++;
        inputChars.CopyTo(newName[index..]); index += (byte)inputChars.Length;
        newName[index] = _buttonCodes[buttonIndex, 1];

        return newName.ToString();
    }

    private async Task Internal_AutoUpdateName(CancellationToken cancellationToken)
    {
        do
        {
            Internal_SetName(Internal_BuildNameString());

            TimeSpan delay = ((!RainbowCheckBox.IsChecked ?? false) && (!ButtonCheckBox.IsChecked ?? false)) ? 
                TimeSpan.FromSeconds(.1) : TimeSpan.FromSeconds(.2);
            
            await Task.Delay(delay, cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }
}
