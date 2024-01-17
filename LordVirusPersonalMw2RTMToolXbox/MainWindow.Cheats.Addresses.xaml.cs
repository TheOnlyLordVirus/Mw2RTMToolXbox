using System;

namespace LordVirusPersonalMw2RTMToolXbox;

public sealed partial class MainWindow
{
    private const UInt32 _cbufAddText = 0x82224990;
    private const UInt32 _svGameSendServerCommand = 0x822548D8;
    private const UInt32 _dvarGetBool = 0x8229EEE8;
    private const UInt32 _nonHostEndGame = 0x826237E0;

    private const UInt32 _prestigeAddress = 0x831A0DD4;
    private const UInt32 _levelAddress = 0x831A0DCC;

    private const UInt32 _nameAddress = 0x838BA824;
    private const UInt32 _clanAddress = 0x82687060;

    private const byte _trueByte = 0x01;
    private const byte _falseByte = 0x00;

    private const UInt32 _laserAddress = 0x82104093;

    private const UInt32 _redBoxAddress = 0x820F4234;
    private static readonly byte[] _redBoxOn = new byte[] { 0x60, 0x00, 0x00, 0x00 }; // NOP
    private static readonly byte[] _redBoxOff = new byte[] { 0x41, 0x9A, 0x00, 0x0C };

    private const UInt32 _noRecoilAddress = 0x820E31F8;
    private static readonly byte[] _noRecoilOn = new byte[] { 0x4E, 0x80, 0x00, 0x20 };
    private static readonly byte[] _noRecoilOff = new byte[] { 0x7D, 0x88, 0x02, 0xA6 };

    private const UInt32 _thermalAddress = 0x82127C98;
    private static readonly byte[] _thermalOn = new byte[] { 0x38, 0x60, 0x00, 0x01, 0x4E, 0x80, 0x00, 0x20 };
    private static readonly byte[] _thermalOff = new byte[] { 0x7D, 0x88, 0x02, 0xA6, 0x91, 0x81, 0xFF, 0xF8 };
}
