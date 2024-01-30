using System;

namespace LordVirusMw2XboxLib;

#nullable enable

public struct G_ClientStructOffset
{
    private UInt32 InternalValue { get; set; }

    public const UInt32 Array_BaseAddress = 0x830CBF80;
    public const UInt32 StructSize = 0x3700;
    public const UInt32 Redboxes = 0x13;
    public const UInt32 PlayerOrgin = 0x1C;
    public const UInt32 Name = 0x3290;
    public const UInt32 Godmode = 0x3228;
    public const UInt32 NoRecoil = 0x2BE;
    public const UInt32 MovementFlag = 0x3423;
    public const UInt32 PrimaryAkimbo = 0x1C;
    public const UInt32 SecondaryAkimbo = 0x25D;
    public const UInt32 AllPerks = 0x428;
    public const UInt32 InfAmmo1 = 0x2EC;
    public const UInt32 InfAmmo2 = 0x2DC;
    public const UInt32 InfAmmo3 = 0x354;
    public const UInt32 InfAmmo4 = 0x36C;
    public const UInt32 InfAmmo5 = 0x360;
    public const UInt32 InfAmmo6 = 0x378;
    public const UInt32 KillstreakBullet = 0x222;
#if DEBUG
    public const UInt32 DebugOffset = 0x0000;
#endif

    public override readonly bool Equals(object? inputObject)
    {
        if (inputObject is not G_ClientStructOffset comparisonObject)
            return false;

        return comparisonObject.InternalValue == InternalValue;
    }

    public override readonly int GetHashCode() => InternalValue.GetHashCode();

    public static G_ClientStructOffset operator - 
        (G_ClientStructOffset left, G_ClientStructOffset right) 
            => left.InternalValue - right.InternalValue;

    public static G_ClientStructOffset operator + 
        (G_ClientStructOffset left, G_ClientStructOffset right)
        => left.InternalValue + right.InternalValue;

    public static G_ClientStructOffset operator * 
        (G_ClientStructOffset left, G_ClientStructOffset right)
        => left.InternalValue * right.InternalValue;

    public static G_ClientStructOffset operator / 
        (G_ClientStructOffset left, G_ClientStructOffset right)
        => left.InternalValue / right.InternalValue;

    public static G_ClientStructOffset operator ~
        (G_ClientStructOffset client)
            => ~client.InternalValue;

    public static G_ClientStructOffset operator |
        (G_ClientStructOffset left, G_ClientStructOffset right)
        => left.InternalValue | right.InternalValue;

    public static G_ClientStructOffset operator &
        (G_ClientStructOffset left, G_ClientStructOffset right)
            => left.InternalValue & right.InternalValue;

    public static G_ClientStructOffset operator ^
        (G_ClientStructOffset left, G_ClientStructOffset right)
            => left.InternalValue ^ right.InternalValue;

    public static bool operator ==
        (G_ClientStructOffset left, G_ClientStructOffset right)
            => left.InternalValue == right.InternalValue;

    public static bool operator !=
        (G_ClientStructOffset left, G_ClientStructOffset right)
            => left.InternalValue == right.InternalValue;

    public static implicit operator G_ClientStructOffset(Int32 otherType)
        => new G_ClientStructOffset
        {
            InternalValue = (UInt32)otherType
        };

    public static implicit operator G_ClientStructOffset(UInt32 otherType)
        =>  new G_ClientStructOffset
        {
            InternalValue = otherType
        };

    public static implicit operator G_ClientStructOffset(ulong otherType)
        => new G_ClientStructOffset
        {
            InternalValue = (UInt32)otherType
        };

    public static implicit operator G_ClientStructOffset(long otherType)
        => new G_ClientStructOffset
        {
            InternalValue = (UInt32)otherType
        };

    public static implicit operator Int32(G_ClientStructOffset otherType) 
        => (Int32)otherType.InternalValue;

    public static implicit operator UInt32(G_ClientStructOffset otherType) 
        => otherType.InternalValue;
}
