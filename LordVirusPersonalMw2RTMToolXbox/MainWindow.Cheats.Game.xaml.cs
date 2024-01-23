using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Fody;

using XDevkit;
using XDRPC;
using XDRPCPlusPlus;

namespace LordVirusPersonalMw2RTMToolXbox;

internal static class Mw2GameFunctions
{
    private const UInt32 _cbufAddText = 0x82224990;
    private const UInt32 _svGameSendServerCommand = 0x822548D8;
    private const UInt32 _dvarGetBool = 0x8229EEE8;

    internal static bool Cg_DvarGetBool(IXboxConsole xboxConsole, string commandString)
        => xboxConsole.ExecuteRPC<bool>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, _dvarGetBool),
            new object[] { commandString }
        );

    internal static void Cbuf_AddText(IXboxConsole xboxConsole, string commandString)
        => xboxConsole.ExecuteRPC<int>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, _cbufAddText),
            new object[] { 0, commandString }
        );

    internal static void Cg_GameSendServerCommand(IXboxConsole xboxConsole, params object[] parameters)
        => xboxConsole.ExecuteRPC<int>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, _svGameSendServerCommand),
            parameters
        );

    internal static void iPrintLn(IXboxConsole xboxConsole, string text, int client = -1)
        => Cg_GameSendServerCommand(xboxConsole, client, 0, $"f \"{text}\"");

    internal static void iPrintLnBold(IXboxConsole xboxConsole, string text, int client = -1)
        => Cg_GameSendServerCommand(xboxConsole, client, 0, $"c \"{text}\"");
}

[ConfigureAwait(false)]
internal sealed class G_Client
{
    private const int _maxNameCharCount = 35;

    private const byte _redBoxesOn = 0x55;
    private const byte _thermalRedBoxesOn = 0x99;
    private const byte _ufoModeOn = 0x02;
    private const byte _noClipOn = 0x99;
    private const byte _noRecoilOn = 0x04;

    private static readonly byte[] _godModeOn =  [ 0x00, 0xFF, 0xFF, 0xFF ];
    private static readonly byte[] _godModeOff = [ 0x00, 0x00, 0x00, 0x64 ];

    private readonly IXboxConsole _xboxConsole;
    public IXboxConsole XboxConsole
    {
        get => _xboxConsole;
    }

    private readonly uint _correctedNameAddress;
    private string _clientName = string.Empty;
    public string ClientName
    {
        get
        {
            ReadOnlySpan<byte> bytes =
                _xboxConsole.ReadBytes
                (
                    _correctedNameAddress,
                    _maxNameCharCount
                );

            bytes = bytes[..bytes.IndexOf((byte)0x00)];
            _clientName = Encoding.UTF8.GetString(bytes);

            return _clientName;
        }

        set
        {
            if (value.Length > _maxNameCharCount)
                value = value[.._maxNameCharCount];

            Span<byte> nameBytes = stackalloc byte[_maxNameCharCount];
            Encoding.ASCII.GetBytes(value, nameBytes);

            _xboxConsole
                .DebugTarget
                .SetMemory
                (
                    _correctedNameAddress,
                    (uint)nameBytes.Length,
                    nameBytes.ToArray(),
                    out _
                );
        }
    }

    private readonly int _clientIndex;
    public int ClientIndex => _clientIndex;

    public G_Client(IXboxConsole xbox, int clientIndex = 0)
    {
        _xboxConsole = xbox;
        _clientIndex = clientIndex;

        _correctedNameAddress =
            (uint)((uint)G_ClientStructOffsets.Array_BaseAddress +
            ((uint)G_ClientStructOffsets.StructSize * _clientIndex) +
            (uint)G_ClientStructOffsets.Name);

        Internal_ConfigureCheatImplmentations();
    }

    // TODO: Get the in game check for the current client without RPC calling GetDvarBool.
    // TODO: Fix missing G_Client mods / addresses
    // TODO: Add magic bullets for clients.
    private void Internal_ConfigureCheatImplmentations()
    {
        _redboxes        = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.Redboxes, _clientIndex, onByte: _redBoxesOn, cheatName: "Red Boxes");
        _thermalRedboxes = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.Redboxes, _clientIndex, onByte: _thermalRedBoxesOn, cheatName: "Thermal Red Boxes");
        _godmode         = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.Godmode, _clientIndex, onBytes: _godModeOn, offBytes: _godModeOff, cheatName: "God Mode");
        _noRecoil        = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.NoRecoil, _clientIndex, onByte: _noRecoilOn, cheatName: "No Recoil");
        _noClip          = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.MovementFlag, _clientIndex, onByte: _noClipOn, cheatName: "No Clip");
        _ufoMode         = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.MovementFlag, _clientIndex, onByte: _ufoModeOn, cheatName: "Ufo Mode");

        //_primaryAkimbo = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.PrimeAkimbo);
        //_secondaryAkimbo = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.SecondaryAkimbo); // Freezes xbox

        _infiniteAmmo = new G_ClientInfiniteAmmo(_xboxConsole, _clientIndex);

#if DEBUG
        DebugCheat = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.DebugOffset, _clientIndex);
#endif
    }

#if DEBUG
    public IGameCheat? DebugCheat;
#endif

    private IGameCheat? _redboxes;
    public IGameCheat? Redboxes => _redboxes;

    private IGameCheat? _thermalRedboxes;
    public IGameCheat? ThermalRedboxes => _thermalRedboxes;

    private IGameCheat? _godmode;
    public IGameCheat? Godmode => _godmode;

    private IGameCheat? _noRecoil;
    public IGameCheat? NoRecoil => _noRecoil;

    private IGameCheat? _noClip;
    public IGameCheat? NoClip => _noClip;

    private IGameCheat? _ufoMode;
    public IGameCheat? UfoMode => _ufoMode;

    // TODO: different implmentation for these cheats.
    //private IGameCheat? _teleport;
    //public IGameCheat? Teleport;

    //private IGameCheat? _killstreakBullet;
    //public IGameCheat? KillstreakBullet => _killstreakBullet;

    //private IGameCheat? _primaryAkimbo;
    //public IGameCheat? PrimaryAkimbo => _primaryAkimbo;

    //private IGameCheat? _secondaryAkimbo;
    //public IGameCheat? SecondaryAkimbo => _secondaryAkimbo;

    //private IGameCheat? _allPerks;
    //public IGameCheat? AllPerks => _allPerks;

    //private IGameCheat? _modGun;
    //public IGameCheat? ModGun => _modGun;

    private IGameCheat? _infAmmo1;
    private IGameCheat? _infAmmo2;
    private IGameCheat? _infAmmo3;
    private IGameCheat? _infAmmo4;
    private IGameCheat? _infAmmo5;
    private IGameCheat? _infAmmo6;
    private G_ClientInfiniteAmmo? _infiniteAmmo;
    public IGameCheat? InfiniteAmmo => _infiniteAmmo;
}

internal interface IGameCheat
{
    bool GetValue();
    byte[] GetBytes();

    void Enable();
    void Disable();
    void Toggle();
}

internal class G_ClientCheat : IGameCheat
{
    private uint CorrectedCheatAddress => 
        (uint)G_ClientStructOffsets.Array_BaseAddress + 
        ((uint)G_ClientStructOffsets.StructSize * (uint)_clientNumber) + 
        (uint)_cheatAddress;

    private IXboxConsole _xboxConsole;
    private G_ClientStructOffsets _cheatAddress;

    private bool _enabled = false;

    private readonly string? _cheatName;
    private readonly int _clientNumber = 0;
    private readonly uint _byteCount = 1;

    private readonly byte[] _onBytes;
    private readonly byte[] _offBytes;

    public G_ClientCheat
    (
        IXboxConsole xboxConsole,
        G_ClientStructOffsets cheatAddress,
        int clientNumber, 
        byte onByte = 0x01, 
        byte offByte = 0x00,
        string? cheatName = null
    )
    {
        _xboxConsole = xboxConsole;
        _cheatAddress = cheatAddress;
        _clientNumber = clientNumber;
        _cheatName = cheatName;

        _onBytes = [ onByte ];
        _offBytes = [ offByte ];
    }

    public G_ClientCheat
    (
        IXboxConsole xboxConsole,
        G_ClientStructOffsets cheatAddress,
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
        _cheatAddress = cheatAddress;
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

        _enabled = true;
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

        _enabled = false;
    }

    public byte[] GetBytes()
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
        _enabled = !(GetValue());

        if (_enabled)
            Enable();
        else
            Disable();
    }
}

internal class G_ClientInfiniteAmmo : IGameCheat
{
    private CancellationTokenSource? _updaterCancellationTokenSource = new CancellationTokenSource();
    private bool _enabled = false;

    private static readonly byte[] _infiniteAmmoOn = [0x0F, 0x00, 0x00, 0x00];
    private static readonly byte[] _infiniteAmmoOff = [0x00, 0x00, 0x00, 0x64];

    private readonly int _clientIndex = 0;

    private readonly IXboxConsole _xboxConsole;
    private readonly IGameCheat _infAmmo1;
    private readonly IGameCheat _infAmmo2;
    private readonly IGameCheat _infAmmo3;
    private readonly IGameCheat _infAmmo4;
    private readonly IGameCheat _infAmmo5;
    private readonly IGameCheat _infAmmo6;

    public G_ClientInfiniteAmmo(IXboxConsole xboxConsole, int clientIndex)
    {
        _xboxConsole = xboxConsole;

        _clientIndex = clientIndex;

        // TODO: Label these for what weapon slot they are giving inf ammo too.
        _infAmmo1 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo1, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
        _infAmmo2 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo2, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
        _infAmmo3 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo3, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
        _infAmmo4 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo4, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
        _infAmmo5 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo5, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
        _infAmmo6 = new G_ClientCheat(_xboxConsole, G_ClientStructOffsets.InfAmmo6, _clientIndex, onBytes: _infiniteAmmoOn, offBytes: _infiniteAmmoOff);
    }

    public async Task UpdateInfAmmo(CancellationToken cancellationToken)
    {
        try
        {
            Mw2GameFunctions.iPrintLn(_xboxConsole, $"Infinite Ammo^7: ^2Enabled", _clientIndex);

            do
            {
                _infAmmo1.Enable();
                _infAmmo2.Enable();
                _infAmmo3.Enable();
                _infAmmo4.Enable();
                _infAmmo5.Enable();
                _infAmmo6.Enable();

                await Task
                    .Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        finally
        {
            _infAmmo1.Disable();
            _infAmmo2.Disable();
            _infAmmo3.Disable();
            _infAmmo4.Disable();
            _infAmmo5.Disable();
            _infAmmo6.Disable();

            Mw2GameFunctions.iPrintLn(_xboxConsole, $"Infinite Ammo^7: ^1Disabled", _clientIndex);
        }
    }

    public void Enable()
    {
        try
        {

            if (_updaterCancellationTokenSource is null)
                _updaterCancellationTokenSource = new CancellationTokenSource();

            _ = UpdateInfAmmo(_updaterCancellationTokenSource.Token);
        }

        catch
        {
            return;
        }

        _enabled = true;
    }

    public void Disable()
    {
        _updaterCancellationTokenSource?.Cancel();
        _updaterCancellationTokenSource = null;
        _enabled = false;
    }

    public byte[] GetBytes()
    {
        return Array.Empty<byte>();
    }

    public bool GetValue()
    {
        return _enabled;
    }

    public void Toggle()
    {
        _enabled = !_enabled;

        if (_enabled)
            Enable();
        else
            Disable();
    }
}
