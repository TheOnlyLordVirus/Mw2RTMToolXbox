using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using XDevkit;

using XDRPC;
using XDRPCPlusPlus;

namespace LordVirusMw2XboxLib;

#nullable enable

using Constants = Mw2XboxLibConstants;

internal static class Mw2GameFunctions
{
    public static bool TryConnectToMw2(IXboxManager? xboxManager, out IXboxConsole? connectedXbox)
    {
        bool result;

        try
        {
            if (xboxManager is null)
                xboxManager = new XboxManager();

            connectedXbox = xboxManager.OpenConsole(xboxManager.DefaultConsole);

            Cbuf_AddText(connectedXbox!, "loc_warningsUI 0; loc_warnings 0; cg_blood 0; cg_bloodLimit 1;");
            
            result = true;

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
            result = false;
            connectedXbox = null;

            MessageBox.Show
            (
                ex.Message,
                "Connect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        return result;
    }

    public static bool LocalClientInGame(IXboxConsole xboxConsole) => Cg_DvarGetBool(xboxConsole!, "cl_ingame");

    public static async Task UnlockAll(IXboxConsole xbox, int client = -1, CancellationToken cancellationToken = default)
    {
        if (!LocalClientInGame(xbox))
        {
            MessageBox.Show("You must be in game to unlock all!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Sv_GameSendServerCommand(xbox!, client, 0, "s loc_warningsUI 0; loc_warnings 0;");

        iPrintLnBold(xbox!, "^2Starting Challenges!", client);

        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks0);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks1);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks2);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks3);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks4);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks5);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks6);
        iPrintLn(xbox!, "25 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken)
            .ConfigureAwait(false);

        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks7);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks8);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks9);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks10);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks11);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks12);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks13);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks14);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks15);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks16);
        iPrintLn(xbox!, "50 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken)
            .ConfigureAwait(false);

        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks17);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks18);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks19);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks20);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks21);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks22);
        iPrintLn(xbox!, "75 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken)
            .ConfigureAwait(false);

        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks23);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks24);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks25);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks26);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks27);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks28);
        Sv_GameSendServerCommand(xbox!, client, 0, Constants.Unlocks29);
        iPrintLn(xbox!, "100 ^9Percent ^4Unlocked", client);
        await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken)
            .ConfigureAwait(false);

        iPrintLnBold(xbox!, "^2Completed Challenges!", client);
    }

    public static void EndGame(IXboxConsole xboxConsole)
    {
        int client = xboxConsole.ReadInt32(Constants.NonHostEndGame);

        Cbuf_AddText(xboxConsole!, $"cmd mr {client} -1 endround;");
    }

    public static bool Cg_DvarGetBool(IXboxConsole xboxConsole, string commandString)
        => xboxConsole.ExecuteRPC<bool>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, Constants.Dvar_GetBool),
            new object[] { commandString }
        );

    public static void Cbuf_AddText(IXboxConsole xboxConsole, string commandString)
        => xboxConsole.ExecuteRPC<int>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, Constants.Cbuf_AddText),
            new object[] { 0, commandString }
        );

    public static void Sv_GameSendServerCommand(IXboxConsole xboxConsole, params object[] parameters)
        => xboxConsole.ExecuteRPC<int>
        (
            new XDRPCExecutionOptions(XDRPCMode.Title, Constants.Sv_GameSendServerCommand),
            parameters
        );

#pragma warning disable IDE1006 // Naming style warning disable, We are naming this iPrintLn because thats what the game calls it.
    public static void iPrintLn(IXboxConsole xboxConsole, string text, int client = -1)
        => Sv_GameSendServerCommand(xboxConsole, client, 0, $"f \"{text}\"");

    public static void iPrintLnBold(IXboxConsole xboxConsole, string text, int client = -1)
        => Sv_GameSendServerCommand(xboxConsole, client, 0, $"c \"{text}\"");
#pragma warning restore IDE1006

    public static void SetPrestige(IXboxConsole xboxConsole, int prestige = 10)
    {
        if (prestige < Constants.MinPrestige)
            throw new ArgumentOutOfRangeException($"Prestige must be greater than {Constants.MinPrestige - 1} & less than prestige {Constants.MaxPrestige + 1}");

        if (prestige > Constants.MaxPrestige)
            throw new ArgumentOutOfRangeException($"Prestige must be greater than {Constants.MinPrestige - 1} & less than prestige {Constants.MaxPrestige + 1}");


        byte[] prestigeBytes = BitConverter
            .GetBytes(prestige);

        xboxConsole.DebugTarget
            .SetMemory
            (
                Constants.PrestigeAddress,
                (uint)prestigeBytes.Length,
                prestigeBytes,
                out _
            );
    }

    public static void SetLevel(IXboxConsole xboxConsole, int level = 70)
    {
        if (level < Constants.MinLevel)
            throw new ArgumentOutOfRangeException($"Level must be greater than {Constants.MinLevel - 1} & less than level {Constants.MaxLevel + 1}");

        if (level > Constants.MaxLevel)
            throw new ArgumentOutOfRangeException($"Level must be greater than {Constants.MinLevel - 1} & less than level {Constants.MaxLevel + 1}");

        byte[] levelBytes = BitConverter.GetBytes(Constants.LevelTable[level - 1]);

        xboxConsole.DebugTarget
            .SetMemory
            (
                Constants.LevelAddress,
                (uint)levelBytes.Length,
                levelBytes,
                out _
            );
    }

    public static void SetName(IXboxConsole xboxConsole, ReadOnlySpan<char> newName)
    {
        if (newName.Length > Constants.MaxNameCharLength)
            newName = newName[..Constants.MaxNameCharLength];

        Span<byte> nameBytes = stackalloc byte[Constants.MaxNameCharLength];
        Encoding.ASCII.GetBytes(newName, nameBytes);

        xboxConsole.DebugTarget
            .SetMemory
            (
                Constants.NameAddress,
                (uint)nameBytes.Length,
                nameBytes.ToArray(),
                out _
            );
    }

    public static void SetClanName(IXboxConsole xboxConsole, ReadOnlySpan<char> newClanName)
    {
        if (newClanName.Length > Constants.MaxClanCharLength)
            newClanName = newClanName[..Constants.MaxClanCharLength];

        Span<byte> clanNameBytes = stackalloc byte[Constants.MaxClanCharLength];
        Encoding.ASCII.GetBytes(newClanName, clanNameBytes);

        xboxConsole.DebugTarget
            .SetMemory
            (
                Constants.ClanAddress,
                (uint)clanNameBytes.Length,
                clanNameBytes.ToArray(),
                out _
            );
    }
}