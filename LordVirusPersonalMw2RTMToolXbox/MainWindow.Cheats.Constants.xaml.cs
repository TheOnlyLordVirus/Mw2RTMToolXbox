using System;

namespace LordVirusPersonalMw2RTMToolXbox;

public sealed partial class MainWindow
{
    private const byte _trueByte = 0x01;
    private const byte _falseByte = 0x00;

    private const UInt32 _laserAddress = 0x82104093;

    private const UInt32 _redBoxAddress = 0x820F4234;
    private static readonly byte[] _redBoxOn = [0x60, 0x00, 0x00, 0x00]; // NOP
    private static readonly byte[] _redBoxOff = [0x41, 0x9A, 0x00, 0x0C];

    private const UInt32 _noRecoilAddress = 0x820E31F8;
    private static readonly byte[] _noRecoilOn = [0x4E, 0x80, 0x00, 0x20];
    private static readonly byte[] _noRecoilOff = [0x7D, 0x88, 0x02, 0xA6];

    private const UInt32 _thermalAddress = 0x82127C98;
    private static readonly byte[] _thermalOn = [0x38, 0x60, 0x00, 0x01, 0x4E, 0x80, 0x00, 0x20];
    private static readonly byte[] _thermalOff = [0x7D, 0x88, 0x02, 0xA6, 0x91, 0x81, 0xFF, 0xF8];

    private const byte _maxNameInputLength = 34;

    private const int _maxPrestige = 11;
    private const int _minPrestige = 0;

    private const int _maxLevel = 70;
    private const int _minLevel = 1;

    private const int _maxClientCount = 18;

    private static readonly char[] _buttonCharMap =
    {
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        '',
        ''
    };
}