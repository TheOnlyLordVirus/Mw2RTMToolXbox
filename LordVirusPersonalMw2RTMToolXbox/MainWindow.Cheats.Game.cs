using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XDevkit;
using XDRPCPlusPlus;
using static LordVirusPersonalMw2RTMToolXbox.G_Client;

namespace LordVirusPersonalMw2RTMToolXbox;

interface IGameCheat
{
    bool GetValue();
    byte[] GetBytes();

    void Enable();
    void Disable();
    void Toggle();
}

internal struct G_ClientCheat : IGameCheat
{
    private IXboxConsole _xboxConsole;
    private G_ClientStructOffsets _cheatOffset;
    private bool _enabled = false;

    private readonly byte[] _readOnlyOnByte = new byte[] { 0x01 };
    private readonly byte[] _readOnlyOffByte = new byte[] { 0x00 };
    private readonly byte[] _onBytes;
    private readonly byte[] _offBytes;

    public G_ClientCheat(IXboxConsole xboxConsole, int clientNumber, G_ClientStructOffsets cheatAddress, byte onByte = 0x01, byte offByte = 0x00)
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatAddress;

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
        _cheatOffset = cheatAddress;

        _onBytes = onBytes;
        _offBytes = offBytes;
    }

    public G_ClientCheat(IXboxConsole xboxConsole, int clientNumber, G_ClientStructOffsets cheatAddress, byte[] onBytes, byte offByte)
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatAddress;

        _onBytes = onBytes;

        if (offByte == 0x00)
            _offBytes = _readOnlyOffByte;
        else
            _offBytes = new byte[1] { offByte };
    }

    public G_ClientCheat(IXboxConsole xboxConsole, int clientNumber, G_ClientStructOffsets cheatAddress, byte onByte, byte[] offBytes)
    {
        _xboxConsole = xboxConsole;
        _cheatOffset = cheatAddress;

        if (onByte == 0x01)
            _onBytes = _readOnlyOnByte;
        else
            _onBytes = new byte[1] { onByte };

        _offBytes = offBytes;
    }

    public void Disable()
    {
        try
        {

        }

        catch
        {

        }

        _enabled = false;
    }

    public void Enable()
    {
        try
        {


        }

        catch
        {

        }

        _enabled = true;
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

    }
}

internal sealed partial class G_Client
{
    internal enum G_ClientStructOffsets : uint
    {
        Array_BaseAddress = 0x830CBF80,
        StructSize = 0x3700,
        Redboxes = 0x13,
        Name = 0x3290,
        Godmode = 0x3229,
        Norecoil = 0x2BE,
        MovementFlag = 0x3290,
        PrimeAkimbo = 0x267,
        SecondaryAkimbo = 0x25D,
        AllPerks = 0x428,
        ModGun = 0x3243,
        Teleport = 0x24,
        InfAmmo1 = 0x2EC,
        InfAmmo2 = 0x2DC,
        InfAmmo3 = 0x354,
        InfAmmo4 = 0x36C,
        InfAmmo5 = 0x360,
        InfAmmo6 = 0x378,
        KillstreakBullet = 0x222
    }

    private struct ClientState : 
        IEnumerator<IGameCheat>,
        IEnumerator
    {
        public IGameCheat? G_Client_Redboxes;
        public IGameCheat? G_Client_ThermalRedboxes;
        public IGameCheat? G_Client_Name;
        public IGameCheat? G_Client_Godmode;
        public IGameCheat? G_Client_Norecoil;
        public IGameCheat? G_Client_MovementFlag;
        public IGameCheat? G_Client_PrimeAkimbo;
        public IGameCheat? G_Client_SecondaryAkimbo;
        public IGameCheat? G_Client_AllPerks;
        public IGameCheat? G_Client_ModGun;
        public IGameCheat? G_Client_Teleport;
        public IGameCheat? G_Client_InfAmmo1;
        public IGameCheat? G_Client_InfAmmo2;
        public IGameCheat? G_Client_InfAmmo3;
        public IGameCheat? G_Client_InfAmmo4;
        public IGameCheat? G_Client_InfAmmo5;
        public IGameCheat? G_Client_InfAmmo6;
        public IGameCheat? G_Client_KillstreakBullet;

        private List<IGameCheat?> _gameCheats = new List<IGameCheat?>();

        public ClientState()
        {
            _gameCheats.Add(G_Client_Redboxes);
            _gameCheats.Add(G_Client_ThermalRedboxes);
            _gameCheats.Add(G_Client_Name);
            _gameCheats.Add(G_Client_Godmode);
            _gameCheats.Add(G_Client_Norecoil);
            _gameCheats.Add(G_Client_MovementFlag);
            _gameCheats.Add(G_Client_PrimeAkimbo);
            _gameCheats.Add(G_Client_SecondaryAkimbo);
            _gameCheats.Add(G_Client_AllPerks);
            _gameCheats.Add(G_Client_ModGun);
            _gameCheats.Add(G_Client_Teleport);
            _gameCheats.Add(G_Client_InfAmmo1);
            _gameCheats.Add(G_Client_InfAmmo2);
            _gameCheats.Add(G_Client_InfAmmo3);
            _gameCheats.Add(G_Client_InfAmmo4);
            _gameCheats.Add(G_Client_InfAmmo5);
            _gameCheats.Add(G_Client_InfAmmo6);
            _gameCheats.Add(G_Client_KillstreakBullet);
        }

        private IGameCheat _currentEnumeratorCheat;
        public IGameCheat Current => _currentEnumeratorCheat;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _gameCheats.Clear();
            this.Dispose();
        }

        private int _cheatIndexer = 0;
        public bool MoveNext()
        {
            if (_cheatIndexer > _gameCheats.Count)
            {
                _cheatIndexer = 0;
                return false;
            }

            var currentCheat = _gameCheats[_cheatIndexer];
            do
            {
                if (currentCheat is null)
                    continue;

                currentCheat = _gameCheats[--_cheatIndexer];
            } while (currentCheat is null);

            _cheatIndexer++;
            return true;
        }

        public void Reset()
        {
            
        }
    }

    internal const byte _redBoxesOn = 0x55;
    internal const byte _thermalRedBoxesOn = 0x99;
    internal static readonly byte[] _godModeOn = new byte[] { 0x00, 0xFF, 0xFF, 0xFF };

    private ClientState CurrentClientState = new ClientState();

    private readonly IXboxConsole _xboxConsole;
    public IXboxConsole XboxConsole
    {
        get => _xboxConsole;
    }

    private string _clientName = string.Empty;
    public string ClientName
    {
        get
        {
            long address = 
                (uint)G_ClientStructOffsets.Array_BaseAddress + 
                    ((uint)G_ClientStructOffsets.StructSize * _clientIndex) + 
                        (uint)G_ClientStructOffsets.Name;

            ReadOnlySpan<byte> bytes = _xboxConsole.ReadBytes((uint)address, 35);

            bytes.Slice(bytes.IndexOf((byte)0x00));

            _clientName = Encoding.UTF8.GetString(bytes);

            return _clientName;
        }
    }

    private readonly int _clientIndex;

    public G_Client(IXboxConsole xbox, int clientIndex = 0)
    {
        _xboxConsole = xbox;
        _clientIndex = clientIndex;

        Internal_ConfigureCheatImplmentations();
    }

    private void Internal_ConfigureCheatImplmentations()
    {
        CurrentClientState.G_Client_Redboxes = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Redboxes, onByte: _redBoxesOn);
        CurrentClientState.G_Client_ThermalRedboxes = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Redboxes, onByte: _thermalRedBoxesOn);
        CurrentClientState.G_Client_Godmode = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Godmode, onBytes: _godModeOn, offByte: (byte)0x00);
        CurrentClientState.G_Client_Norecoil = new G_ClientCheat(_xboxConsole, _clientIndex, G_ClientStructOffsets.Norecoil, onBytes: _godModeOn, offByte: (byte)0x00);
    }

    public void UpdateClient()
    {

    }
}
