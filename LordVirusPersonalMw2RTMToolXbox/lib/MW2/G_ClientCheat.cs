using System;
using System.Linq;

using XDevkit;
using XDRPCPlusPlus;

namespace LordVirusMw2XboxLib;

#nullable enable

internal sealed class G_ClientCheat : IGameCheat
{
    private readonly IXboxConsole _xboxConsole;
    private readonly G_ClientStructOffset _cheatOffset;

    private readonly string? _cheatName;
    private readonly int _clientNumber = 0;
    private readonly uint _byteCount = 1;

    private readonly byte[] _onBytes;
    private readonly byte[] _offBytes;

    private uint CorrectedCheatAddress => 
        G_ClientStructOffset.Array_BaseAddress + 
            (G_ClientStructOffset.StructSize * (uint)_clientNumber) + 
                _cheatOffset;

    private bool enabled = false;

    public G_ClientCheat
    (
        IXboxConsole xboxConsole,
        G_ClientStructOffset cheatOffset,
        int clientNumber,
        byte onByte = 0x01,
        byte offByte = 0x00,
        string? cheatName = null
    )
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatOffset;
        _clientNumber = clientNumber;
        _cheatName = cheatName;

        _onBytes = [onByte];
        _offBytes = [offByte];
    }

    public G_ClientCheat
    (
        IXboxConsole xboxConsole,
        G_ClientStructOffset cheatOffset,
        int clientNumber,
        byte[] onBytes,
        byte[] offBytes,
        string? cheatName = null
    )
    {
        if (onBytes.Length != offBytes.Length)
            throw new ArgumentOutOfRangeException("Error: onBytes and offBytes must have the same number of bytes.");

        _byteCount = (uint)onBytes.Length;

        _xboxConsole = xboxConsole;
        _cheatOffset = cheatOffset;
        _clientNumber = clientNumber;
        _cheatName = cheatName;

        _onBytes = onBytes;
        _offBytes = offBytes;
    }

    public void Enable()
    {
        try
        {
            _xboxConsole
                .WriteBytes
                (
                    CorrectedCheatAddress,
                    _onBytes
                );

            if (_cheatName is not null)
                Mw2GameFunctions.iPrintLn(_xboxConsole, $"{_cheatName}^7: ^2Enabled", _clientNumber);
        }

        catch
        {
            return;
        }

        enabled = true;
    }

    public void Disable()
    {
        try
        {
            _xboxConsole
                .WriteBytes
                (
                    CorrectedCheatAddress,
                    _offBytes
                );

            if (_cheatName is not null)
                Mw2GameFunctions.iPrintLn(_xboxConsole, $"{_cheatName}^7: ^1Disabled", _clientNumber);
        }

        catch
        {
            return;
        }

        enabled = false;
    }

    private byte[] GetBytes()
    {
        return _xboxConsole
                .ReadBytes
                (
                    CorrectedCheatAddress,
                    _byteCount
                );
    }

    public bool GetValue()
    {
        try
        {
            return Enumerable.SequenceEqual
                (
                    GetBytes(),
                    _onBytes
                );
        }

        catch { return false; }
    }

    public void Toggle()
    {
        enabled = !(GetValue());

        if (enabled)
            Enable();
        else
            Disable();
    }
}
