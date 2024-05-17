using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using XDevkit;
using XDRPCPlusPlus;

namespace LordVirusMw2XboxLib;

#nullable enable

using Functions = Mw2GameFunctions;
using Constants = Mw2XboxLibConstants;

// TODO: Get the in game check for the current client without RPC calling GetDvarBool.
// TODO: Fix missing G_Client mods / addresses
// TODO: Add magic bullets for clients.
internal sealed record class G_Client (IXboxConsole XboxConsole, int ClientIndex = 0)
{
    private readonly G_ClientStructOffset _correctedNameAddress =
                (G_ClientStructOffset.Array_BaseAddress +
                    (G_ClientStructOffset.StructSize * ClientIndex) +
                        G_ClientStructOffset.Name);

    public Task UnlockAll(CancellationToken cancellationToken = default) =>
        Functions.UnlockAll(XboxConsole, ClientIndex, cancellationToken);

    public override string ToString() => ClientName;
    private string clientName = string.Empty;
    public string ClientName
    {
        get
        {
            ReadOnlySpan<byte> bytes =
                XboxConsole.ReadBytes
                (
                    _correctedNameAddress,
                    Constants.MaxNameCharLength
                );

            bytes = bytes[..bytes.IndexOf((byte)0x00)];
            clientName = Encoding.UTF8.GetString(bytes);

            return clientName;
        }

        set
        {
            if (value.Length > Constants.MaxNameCharLength)
                value = value[..Constants.MaxNameCharLength];

            Span<byte> nameBytes = stackalloc byte[Constants.MaxNameCharLength];
            Encoding.ASCII.GetBytes(value, nameBytes);

            XboxConsole
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

#if DEBUG
    private readonly IGameCheat? _debugCheat = null;
    public IGameCheat? DebugCheat
    {
        get => _debugCheat;
        init
        {
            //_debugCheat = new G_ClientCheat
            //(
            //    XboxConsole,
            //    G_ClientStructOffset.DebugOffset,
            //    ClientIndex,
            //    cheatName: "Debug Cheat"
            //);
        }
    }
#endif

    public readonly IGameCheat Godmode
        = new G_ClientCheat
            (
                XboxConsole,
                G_ClientStructOffset.Godmode,
                ClientIndex,
                onBytes: Constants.GodModeOn,
                offBytes: Constants.GodModeOff,
                cheatName: "God Mode"
            );

    public readonly IGameCheat Redboxes 
        = new G_ClientLoopingCheat
            (
                XboxConsole,
                G_ClientStructOffset.Redboxes,
                ClientIndex,
                onByte: Constants.RedBoxesOn,
                cheatName: "Red Boxes"
            );

    public readonly IGameCheat ThermalRedboxes
        = new G_ClientLoopingCheat
            (
                XboxConsole,
                G_ClientStructOffset.Redboxes,
                ClientIndex,
                onByte: Constants.ThermalRedBoxesOn,
                cheatName: "Thermal Red Boxes"
            );

    public readonly IGameCheat NoRecoil
        = new G_ClientLoopingCheat
            (
                XboxConsole,
                G_ClientStructOffset.NoRecoil,
                ClientIndex,
                onByte: Constants.NoRecoilOn,
                cheatName: "No Recoil"
            );

    public readonly IGameCheat NoClip
        = new G_ClientCheat
            (
                XboxConsole,
                G_ClientStructOffset.MovementFlag,
                ClientIndex,
                onByte: Constants.NoClipOn,
                cheatName: "No Clip"
            );

    public readonly IGameCheat UfoMode
        = new G_ClientCheat
            (
                XboxConsole,
                G_ClientStructOffset.MovementFlag,
                ClientIndex,
                onByte: Constants.UfoModeOn,
                cheatName: "Ufo Mode"
            );

    public readonly IGameCheat PrimaryAkimbo
       = new G_ClientCheat
            (
                XboxConsole,
                G_ClientStructOffset.PrimaryAkimbo,
                ClientIndex,
                cheatName: "Primary Akimbo"
            );

    public readonly IGameCheat SecondaryAkimbo 
        = new G_ClientCheat
            (
                XboxConsole,
                G_ClientStructOffset.SecondaryAkimbo,
                ClientIndex,
                cheatName: "Secondary Akimbo"
            );

    public readonly IGameCheat AllPerks 
        = new G_ClientLoopingCheat
            (
                XboxConsole,
                G_ClientStructOffset.AllPerks,
                ClientIndex,
                onBytes: Constants.AllPerksOn,
                offBytes: Constants.AllPerksOff,
                cheatName: "All Perks"
            );

    private readonly IGameCheat _infiniteAmmo = default!;
    public IGameCheat InfiniteAmmo 
    { 
        get => _infiniteAmmo;
        private init
        {
            _infiniteAmmo = new G_ClientLoopingCheat
            (
                XboxConsole,
                ClientIndex,
                Internal_BuildInfAmmoCheats(),
                cheatName: "Infinite Ammo"
            );
        }
    }

    private IGameCheat[] Internal_BuildInfAmmoCheats()
    {
        const byte offsetCount = 6;

        var infAmmoOffsets = new G_ClientStructOffset[offsetCount];

        byte index = 0;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo1; index++;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo2; index++;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo3; index++;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo4; index++;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo5; index++;
        infAmmoOffsets[index] = G_ClientStructOffset.InfAmmo6; index++;

        var infAmmoGameCheats = new IGameCheat[offsetCount];

        // Set looping max ammo capacity for all 6 Weapon slots.
        for (byte i = 0; i < offsetCount; ++i)
            infAmmoGameCheats[i] = new G_ClientCheat
                (
                    XboxConsole,
                    infAmmoOffsets[i],
                    ClientIndex,
                    onBytes: Constants.InfiniteAmmoOn,
                    offBytes: Constants.InfiniteAmmoOff
                );

        return infAmmoGameCheats;
    }
}