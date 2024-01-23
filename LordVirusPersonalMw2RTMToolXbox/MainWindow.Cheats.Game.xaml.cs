using System;
using System.Text;
using System.Collections.Generic;

using XDevkit;
using XDRPCPlusPlus;

namespace LordVirusPersonalMw2RTMToolXbox;

internal sealed partial class G_Client
{
    // TODO: different implmentation for these cheats.
    //private IGameCheat? _name;
    //public IGameCheat? Name => _name;

    //private IGameCheat? _teleport;
    //public IGameCheat? Teleport;

    //private IGameCheat? _killstreakBullet;
    //public IGameCheat? KillstreakBullet => _killstreakBullet;

    // TODO: Implement this for Noclip, Ufo, and any other mFlag.
    //private IGameCheat? _movementFlag;
    //public IGameCheat? MovementFlag => _movementFlag;

    private List<IGameCheat?> _gameCheats = new List<IGameCheat?>();

    private const uint _maxNameCharCount = 35;

    private const byte _redBoxesOn = 0x55;
    private const byte _thermalRedBoxesOn = 0x99;

    private const byte _noRecoilOn = 0x04;

    private readonly byte[] _godModeOn = new byte[] { 0x00, 0xFF, 0xFF, 0xFF };
    private readonly byte[] _godModeOff = new byte[] { 0x00, 0x00, 0x00, 0x64 };

    //private readonly byte[] _infiniteAmmoOn = new byte[] { 0x00, 0xFF, 0xFF, 0xFF };
    //private readonly byte[] _infiniteAmmoOff = new byte[] { 0x00, 0x00, 0x00, 0x64 };

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
    }

    private readonly int _clientIndex;

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

    private void Internal_ConfigureCheatImplmentations()
    {
        _redboxes = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Redboxes, onByte: _redBoxesOn);
        _thermalRedboxes = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Redboxes, onByte: _thermalRedBoxesOn);

        _godmode = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Godmode, onBytes: _godModeOn, offBytes: _godModeOff);

        _noRecoil = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.NoRecoil, onByte: _noRecoilOn);

        _infAmmo1 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo1);
        _infAmmo2 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo2);
        _infAmmo3 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo3);
        _infAmmo4 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo4);
        _infAmmo5 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo5);
        _infAmmo6 = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.InfAmmo6);

        _infiniteAmmo = new G_ClientInfiniteAmmo
        (
            _infAmmo1!,
            _infAmmo2!,
            _infAmmo3!,
            _infAmmo4!,
            _infAmmo5!,
            _infAmmo6!
        );

        _gameCheats.Add(_redboxes);
        _gameCheats.Add(_thermalRedboxes);
        _gameCheats.Add(_godmode);
        _gameCheats.Add(_noRecoil);

#if DEBUG
        DebugCheat = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.DebugOffset, onBytes: _godModeOn, offBytes: _godModeOff);
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

interface IGameCheat
{
    bool GetValue();
    byte[] GetBytes();

    void Enable();
    void Disable();
    void Toggle();
}

internal class G_ClientCheat : IGameCheat
{
    private uint CorrectedCheatAddress => (uint)G_ClientStructOffsets.Array_BaseAddress + ((uint)G_ClientStructOffsets.StructSize * (uint)_clientNumber) + (uint)_cheatAddress;

    private IXboxConsole _xboxConsole;
    private G_ClientStructOffsets _cheatAddress;

    private bool _enabled = false;
    private readonly int _clientNumber = 0;
    private readonly uint _byteCount = 1;
    private readonly byte[] _readOnlyOnByte = new byte[] { 0x01 };
    private readonly byte[] _readOnlyOffByte = new byte[] { 0x00 };
    private readonly byte[] _onBytes;
    private readonly byte[] _offBytes;

    public G_ClientCheat(IXboxConsole xboxConsole, int clientNumber, G_ClientStructOffsets cheatAddress, byte onByte = 0x01, byte offByte = 0x00)
    {
        _xboxConsole = xboxConsole;
        _cheatAddress = cheatAddress;
        _clientNumber = clientNumber;

        if (onByte == 0x01)
            _onBytes = _readOnlyOnByte;
        else
            _onBytes = new byte[1] { onByte };

        if (offByte == 0x00)
            _offBytes = _readOnlyOffByte;
        else
            _offBytes = new byte[1] { offByte };
    }

    public G_ClientCheat(IXboxConsole xboxConsole, int clientNumber, G_ClientStructOffsets cheatAddress, byte[] onBytes, byte[] offBytes)
    {
        _xboxConsole = xboxConsole;
        _cheatAddress = cheatAddress;
        _clientNumber = clientNumber;

        if (onBytes.Length != offBytes.Length)
            throw new ArgumentOutOfRangeException("Error: onBytes and offBytes must have the same number of bytes.");

        _byteCount = (uint)onBytes.Length;

        _onBytes = onBytes;
        _offBytes = offBytes;
    }

    public void Enable()
    {
        try
        {
            _xboxConsole.WriteBytes(CorrectedCheatAddress, _onBytes);
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
            _xboxConsole.WriteBytes(CorrectedCheatAddress, _offBytes);
        }

        catch
        {
            return;
        }

        _enabled = false;
    }

    public byte[] GetBytes()
    {
        return _xboxConsole.ReadBytes(CorrectedCheatAddress, _byteCount);
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

internal class G_ClientInfiniteAmmo : IGameCheat
{
    private bool _enabled = false;

    private readonly IGameCheat _infAmmo1;
    private readonly IGameCheat _infAmmo2;
    private readonly IGameCheat _infAmmo3;
    private readonly IGameCheat _infAmmo4;
    private readonly IGameCheat _infAmmo5;
    private readonly IGameCheat _infAmmo6;

    public G_ClientInfiniteAmmo(
        IGameCheat infAmmo1,
        IGameCheat infAmmo2,
        IGameCheat infAmmo3,
        IGameCheat infAmmo4,
        IGameCheat infAmmo5,
        IGameCheat infAmmo6)
    {
        _infAmmo1 = infAmmo1;
        _infAmmo2 = infAmmo2;
        _infAmmo3 = infAmmo3;
        _infAmmo4 = infAmmo4;
        _infAmmo5 = infAmmo5;
        _infAmmo6 = infAmmo6;
    }

    public void Enable()
    {
        try
        {
            _infAmmo1.Enable();
            _infAmmo2.Enable();
            _infAmmo3.Enable();
            _infAmmo4.Enable();
            _infAmmo5.Enable();
            _infAmmo6.Enable();
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
            _infAmmo1.Disable();
            _infAmmo2.Disable();
            _infAmmo3.Disable();
            _infAmmo4.Disable();
            _infAmmo5.Disable();
            _infAmmo6.Disable();
        }

        catch
        {
            return;
        }

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
