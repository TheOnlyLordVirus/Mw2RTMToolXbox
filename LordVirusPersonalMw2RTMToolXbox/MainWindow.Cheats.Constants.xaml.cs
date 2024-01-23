using System;

namespace LordVirusPersonalMw2RTMToolXbox;

//gclient_s
//{
//    playerState_s ps; // 0x0
//    clientSession_s sess; // 0x3190
//    int spectatorClient; // 0x341C
//    int mFlags; // 0x3420
//    char padding[4]; // 0x3424
//    int lastCmdTime; // 0x3428
//    int buttons; // 0x342C
//    int oldButtons; // 0x3430
//    int latched_buttons; // 0x3434
//    int buttonsSinceLastFrame; // 0x3438
//    vec3 oldOrigin; // 0x343C
//    float fGunPitch; // 0x3448
//    float fGunYaw; // 0x344C
//    char padding2[8]; // 0x3450
//    vec3 damage_from; // 0x3458
//    int damage_fromWorld; // 0x3464
//    int accurateCount; // 0x3468
//    int accuracy_shots; // 0x346C
//    int accuracy_hits; // 0x3470
//    int inactivityTime; // 0x3474
//    int inactivityWarning; // 0x3478
//    int lastVoiceTime; // 0x347C
//    int switchTeamTime; // 0x3480
//    float currentAimSpreadScale; // 0x3484
//    float prevLinkedInvQuat[4]; // 0x3488
//    bool link_rotationMovesEyePos; // 0x3498
//    bool link_doCollision; // 0x3499
//    bool link_useTagAnglesForViewAngles; // 0x349A
//    bool link_useBaseAnglesForViewClamp; // 0x349B
//    float linkAnglesFrac; // 0x349C
//    viewClampState link_viewClamp; // 0x34A0
//}

internal enum G_ClientStructOffsets : uint
{
    Array_BaseAddress = 0x830CBF80,
    StructSize = 0x3700,
    Redboxes = 0x13,
    Name = 0x3290,
    Godmode = 0x3228,
    NoRecoil = 0x2BE,
    MovementFlag = 0x3423,
    //PrimeAkimbo = 0x267,
    //SecondaryAkimbo = 0x25D,
    //AllPerks = 0x428,
    //ModGun = 0x3243,
    //Teleport = 0x24,
    InfAmmo1 = 0x2EC,
    InfAmmo2 = 0x2DC,
    InfAmmo3 = 0x354,
    InfAmmo4 = 0x36C,
    InfAmmo5 = 0x360,
    InfAmmo6 = 0x378,
    KillstreakBullet = 0x222,

#if DEBUG
    DebugOffset = 0x3228
#endif
}

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

    private static readonly int[] _levelTable =
    {
        0,
        500,
        1700,
        3600,
        6200,
        9500,
        13500,
        18200,
        23600,
        29700,
        36500,
        44300,
        53100,
        62900,
        73700,
        85500,
        98300,
        112100,
        126900,
        142700,
        159500,
        177300,
        196100,
        215900,
        236700,
        258500,
        281300,
        305100,
        329900,
        355700,
        382700,
        410900,
        440300,
        470900,
        502700,
        535700,
        569900,
        605300,
        641900,
        679700,
        718700,
        758900,
        800300,
        842900,
        886700,
        931700,
        977900,
        1025300,
        1073900,
        1123700,
        1175000,
        1227800,
        1282100,
        1337900,
        1395200,
        1454000,
        1514300,
        1576100,
        1639400,
        1704200,
        1770500,
        1838300,
        1907600,
        1978400,
        2050700,
        2124500,
        2199800,
        2276600,
        2354900,
        2434700,
        2516000
    };

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