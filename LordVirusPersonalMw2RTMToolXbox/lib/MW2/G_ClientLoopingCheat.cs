using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using XDevkit;
using XDRPCPlusPlus;

namespace LordVirusMw2XboxLib;

#nullable enable

internal sealed class G_ClientLoopingCheat : IGameCheat
{    
    private readonly IXboxConsole _xboxConsole;
    private readonly G_ClientStructOffset _cheatOffset;

    private readonly string? _cheatName;
    private readonly int _clientNumber = 0;
    private readonly uint _byteCount = 1;
     
    private readonly bool _usingBytes = true;
    private readonly byte[]? _onBytes;
    private readonly byte[]? _offBytes;

    private readonly IEnumerable<IGameCheat>? _gameCheats;

    private CancellationTokenSource? updaterCancellationTokenSource = new CancellationTokenSource();

    private uint CorrectedCheatAddress =>
        G_ClientStructOffset.Array_BaseAddress +
            (G_ClientStructOffset.StructSize * (uint)_clientNumber) +
                _cheatOffset;

    private bool enabled = false;

    public G_ClientLoopingCheat(
        IXboxConsole xboxConsole,
        G_ClientStructOffset cheatOffset,
        int clientIndex,
        byte onByte = 0x01,
        byte offByte = 0x00,
        string? cheatName = null)
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatOffset;
        _clientNumber = clientIndex;

        _onBytes = [onByte];
        _offBytes = [offByte];

        _cheatName = cheatName;
    }

    public G_ClientLoopingCheat(
        IXboxConsole xboxConsole,
        G_ClientStructOffset cheatOffset,
        int clientIndex,
        byte[] onBytes,
        byte[] offBytes,
        string? cheatName = null)
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatOffset;
        _clientNumber = clientIndex;

        if (onBytes.Length != offBytes.Length)
            throw new ArgumentOutOfRangeException("Error: onBytes and offBytes must have the same number of bytes.");

        _onBytes = onBytes;
        _offBytes = offBytes;

        _cheatName = cheatName;
    }

    public G_ClientLoopingCheat(
        IXboxConsole xboxConsole,
        int clientIndex,
        IEnumerable<IGameCheat> gameCheats,
        string? cheatName = null)
    {
        _xboxConsole = xboxConsole;
        _clientNumber = clientIndex;

        _onBytes = null;
        _offBytes = null;

        _usingBytes = false;
        _gameCheats = gameCheats;

        _cheatName = cheatName;
    }

    public G_ClientLoopingCheat(
        IXboxConsole xboxConsole,
        int clientIndex,
        IGameCheat gameCheats,
        string? cheatName = null)
    {
        _xboxConsole = xboxConsole;
        _clientNumber = clientIndex;

        _onBytes = null;
        _offBytes = null;

        _usingBytes = false;
        _gameCheats = [gameCheats];

        _cheatName = cheatName;
    }

    public async Task SetLoop(CancellationToken cancellationToken)
    {
        try
        {
            if (_cheatName is not null)
                Mw2GameFunctions.iPrintLn(_xboxConsole, $"{_cheatName}^7: ^2Enabled", _clientNumber);

            do
            {
                if (_usingBytes)
                    _xboxConsole
                        .WriteBytes
                        (
                            CorrectedCheatAddress,
                            _onBytes!
                        );

                else if (_gameCheats is not null)
                    foreach (var cheat in _gameCheats)
                        cheat.Enable();

                await Task
                    .Delay(TimeSpan.FromSeconds(3), cancellationToken)
                    .ConfigureAwait(false);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        finally
        {
            if (_usingBytes)
                _xboxConsole
                    .WriteBytes
                    (
                        CorrectedCheatAddress,
                        _offBytes!
                    );

            else if (_gameCheats is not null)
                foreach (var cheat in _gameCheats)
                    cheat.Disable();

            enabled = false;

            if (_cheatName is not null)
                Mw2GameFunctions.iPrintLn(_xboxConsole, $"{_cheatName}^7: ^1Disabled", _clientNumber);
        }
    }

    public void Enable()
    {
        try
        {
            if (updaterCancellationTokenSource is null)
                updaterCancellationTokenSource = new CancellationTokenSource();

            _ = SetLoop(updaterCancellationTokenSource.Token)
                .ConfigureAwait(false);
        }

        catch
        {
            return;
        }

        enabled = true;
    }

    public void Disable()
    {
        updaterCancellationTokenSource?.Cancel();
        updaterCancellationTokenSource = null;
        enabled = false;
    }

    public bool GetValue()
    {
        return enabled;
    }

    public void Toggle()
    {
        enabled = !enabled;

        if (enabled)
            Enable();
        else
            Disable();
    }
}
