using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace XDRPC;

// Token: 0x0200001E RID: 30
// (Invoke) Token: 0x060000DA RID: 218
public delegate void RPCEventHandler(object source);

// Token: 0x0200001F RID: 31
// (Invoke) Token: 0x060000DE RID: 222
public delegate void RPCEventHandler<T>(object source, T info);

// Token: 0x02000003 RID: 3
public enum ArgumentType
{
    // Token: 0x04000004 RID: 4
    ByValue,
    // Token: 0x04000005 RID: 5
    ByRef,
    // Token: 0x04000006 RID: 6
    Out
}

// Token: 0x02000004 RID: 4
public enum CountType
{
    // Token: 0x04000008 RID: 8
    Byte,
    // Token: 0x04000009 RID: 9
    Element
}

// Token: 0x02000007 RID: 7
public interface XDRPCArgumentInfo
{
    // Token: 0x06000010 RID: 16
    ulong GetArgumentValue(ulong bufferAddress);

    // Token: 0x06000011 RID: 17
    ulong[] GetArgumentValues(ulong bufferAddress);

    // Token: 0x06000012 RID: 18
    void SetReferenceAddress(ulong bufferAddress);

    // Token: 0x06000013 RID: 19
    int GetArgumentCount();

    // Token: 0x06000014 RID: 20
    int GetRequiredBufferSize();

    // Token: 0x06000015 RID: 21
    int GetRequiredReferenceSize();

    // Token: 0x06000016 RID: 22
    byte[] PackBufferData();

    // Token: 0x06000017 RID: 23
    byte[] PackReferenceData();

    // Token: 0x06000018 RID: 24
    void UnpackBufferData(byte[] data);

    // Token: 0x06000019 RID: 25
    void UnpackReferenceData(byte[] data);

    // Token: 0x0600001A RID: 26
    bool IsFloatingPointValue();

    // Token: 0x17000002 RID: 2
    // (get) Token: 0x0600001B RID: 27
    ArgumentType PassBy { get; }
}

// Token: 0x0200000A RID: 10
public class XDRPCArgumentInfo<T> : XDRPCArgumentInfo where T : struct
{
    // Token: 0x0600003B RID: 59 RVA: 0x00002598 File Offset: 0x00001598
    public XDRPCArgumentInfo(T v) : this(v, ArgumentType.ByValue)
    {
    }

    // Token: 0x0600003C RID: 60 RVA: 0x000025A4 File Offset: 0x000015A4
    public XDRPCArgumentInfo(T v, ArgumentType t)
    {
        Type typeFromHandle = typeof(T);
        if (typeFromHandle.IsPrimitive)
        {
            this.Value = v;
            this.PassBy = t;
            return;
        }
        if (XDRPCMarshaler.IsValidStructType(typeFromHandle))
        {
            throw new XDRPCInvalidTypeException(typeFromHandle, string.Format("Type {0} is not supported by XDRPCArgumentInfo, use XDRPCStructArgumentInfo instead.", typeFromHandle.Name));
        }
        throw new XDRPCInvalidTypeException(typeFromHandle, string.Format("Type {0} is not supported by XDRPC.", typeFromHandle.Name));
    }

    // Token: 0x17000006 RID: 6
    // (get) Token: 0x0600003D RID: 61 RVA: 0x0000260E File Offset: 0x0000160E
    // (set) Token: 0x0600003E RID: 62 RVA: 0x00002616 File Offset: 0x00001616
    public T Value { get; private set; }

    // Token: 0x17000007 RID: 7
    // (get) Token: 0x0600003F RID: 63 RVA: 0x0000261F File Offset: 0x0000161F
    // (set) Token: 0x06000040 RID: 64 RVA: 0x00002627 File Offset: 0x00001627
    public ArgumentType PassBy { get; private set; }

    // Token: 0x06000041 RID: 65 RVA: 0x00002630 File Offset: 0x00001630
    public ulong GetArgumentValue(ulong bufferAddress)
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByValue:
                return MarshalingUtils.ConvertToUInt64(this.Value);
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                return bufferAddress;
            default:
                return 0UL;
        }
    }

    // Token: 0x06000042 RID: 66 RVA: 0x00002670 File Offset: 0x00001670
    public int GetRequiredBufferSize()
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                return MarshalingUtils.SizeOf(typeof(T));
            default:
                return 0;
        }
    }

    // Token: 0x06000043 RID: 67 RVA: 0x000026A8 File Offset: 0x000016A8
    public byte[] PackBufferData()
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            byte[] bytes = XDRPCArgumentInfo<T>.GetBytes(this.Value);
            if (BitConverter.IsLittleEndian)
            {
                MarshalingUtils.ReverseBytes(bytes);
            }
            return bytes;
        }
        return null;
    }

    // Token: 0x06000044 RID: 68 RVA: 0x000026E8 File Offset: 0x000016E8
    public void UnpackBufferData(byte[] data)
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            if (BitConverter.IsLittleEndian)
            {
                MarshalingUtils.ReverseBytes(data);
            }
            object value = XDRPCArgumentInfo<T>.GetValue(data);
            object obj;
            if ((obj = value) == null)
            {
                obj = default(T);
            }
            this.Value = (T)((object)obj);
        }
    }

    // Token: 0x06000045 RID: 69 RVA: 0x00002739 File Offset: 0x00001739
    public bool IsFloatingPointValue()
    {
        return this.PassBy == ArgumentType.ByValue && (typeof(T) == typeof(float) || typeof(T) == typeof(double));
    }

    // Token: 0x06000046 RID: 70 RVA: 0x00002774 File Offset: 0x00001774
    private static byte[] GetBytes(object o)
    {
        if (typeof(T) == typeof(bool))
        {
            return BitConverter.GetBytes(((bool)o) ? 1 : 0);
        }
        if (typeof(T) == typeof(byte))
        {
            return new byte[]
            {
                (byte)o
            };
        }
        if (typeof(T) == typeof(short))
        {
            return BitConverter.GetBytes((short)o);
        }
        if (typeof(T) == typeof(int))
        {
            return BitConverter.GetBytes((int)o);
        }
        if (typeof(T) == typeof(long))
        {
            return BitConverter.GetBytes((long)o);
        }
        if (typeof(T) == typeof(ushort))
        {
            return BitConverter.GetBytes((ushort)o);
        }
        if (typeof(T) == typeof(uint))
        {
            return BitConverter.GetBytes((uint)o);
        }
        if (typeof(T) == typeof(ulong))
        {
            return BitConverter.GetBytes((ulong)o);
        }
        if (typeof(T) == typeof(double))
        {
            return BitConverter.GetBytes((double)o);
        }
        if (typeof(T) == typeof(float))
        {
            return BitConverter.GetBytes((float)o);
        }
        return null;
    }

    // Token: 0x06000047 RID: 71 RVA: 0x000028E4 File Offset: 0x000018E4
    private static object GetValue(byte[] buffer)
    {
        if (typeof(T) == typeof(bool))
        {
            return BitConverter.ToUInt32(buffer, 0) != 0U;
        }
        if (typeof(T) == typeof(byte))
        {
            if (buffer.Length <= 0)
            {
                return null;
            }
            return buffer[0];
        }
        else
        {
            if (typeof(T) == typeof(short))
            {
                return BitConverter.ToInt16(buffer, 0);
            }
            if (typeof(T) == typeof(int))
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            if (typeof(T) == typeof(long))
            {
                return BitConverter.ToInt64(buffer, 0);
            }
            if (typeof(T) == typeof(ushort))
            {
                return BitConverter.ToUInt16(buffer, 0);
            }
            if (typeof(T) == typeof(uint))
            {
                return BitConverter.ToUInt32(buffer, 0);
            }
            if (typeof(T) == typeof(ulong))
            {
                return BitConverter.ToUInt64(buffer, 0);
            }
            if (typeof(T) == typeof(double))
            {
                return BitConverter.ToDouble(buffer, 0);
            }
            if (typeof(T) == typeof(float))
            {
                return BitConverter.ToSingle(buffer, 0);
            }
            return null;
        }
    }

    // Token: 0x06000048 RID: 72 RVA: 0x00002A5C File Offset: 0x00001A5C
    private static T ConvertFromUInt64(ulong v)
    {
        object obj = default(T);
        if (typeof(T) == typeof(bool))
        {
            obj = ((v & 65535UL) != 0UL);
        }
        else if (typeof(T) == typeof(byte))
        {
            obj = (byte)v;
        }
        else if (typeof(T) == typeof(short))
        {
            obj = (short)v;
        }
        else if (typeof(T) == typeof(int))
        {
            obj = (int)v;
        }
        else if (typeof(T) == typeof(long))
        {
            obj = (long)v;
        }
        else if (typeof(T) == typeof(ushort))
        {
            obj = (ushort)v;
        }
        else if (typeof(T) == typeof(uint))
        {
            obj = (uint)v;
        }
        else if (typeof(T) == typeof(ulong))
        {
            obj = v;
        }
        else if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
        {
            obj = BitConverter.Int64BitsToDouble((long)v);
        }
        return (T)((object)obj);
    }

    // Token: 0x06000049 RID: 73 RVA: 0x00002BD1 File Offset: 0x00001BD1
    public override string ToString()
    {
        return string.Format("[XDRPC]{0}: {1}", typeof(T).Name, this.Value);
    }

    // Token: 0x0600004A RID: 74 RVA: 0x00002BF7 File Offset: 0x00001BF7
    public int GetArgumentCount()
    {
        return 1;
    }

    // Token: 0x0600004B RID: 75 RVA: 0x00002BFC File Offset: 0x00001BFC
    public ulong[] GetArgumentValues(ulong bufferAddress)
    {
        if (this.PassBy == ArgumentType.ByValue)
        {
            return new ulong[]
            {
                this.GetArgumentValue(bufferAddress)
            };
        }
        return null;
    }

    // Token: 0x0600004C RID: 76 RVA: 0x00002C25 File Offset: 0x00001C25
    public void SetReferenceAddress(ulong bufferAddress)
    {
    }

    // Token: 0x0600004D RID: 77 RVA: 0x00002C27 File Offset: 0x00001C27
    public int GetRequiredReferenceSize()
    {
        return 0;
    }

    // Token: 0x0600004E RID: 78 RVA: 0x00002C2A File Offset: 0x00001C2A
    public byte[] PackReferenceData()
    {
        return null;
    }

    // Token: 0x0600004F RID: 79 RVA: 0x00002C2D File Offset: 0x00001C2D
    public void UnpackReferenceData(byte[] data)
    {
    }
}

// Token: 0x0200000C RID: 12
public class XDRPCArrayArgumentInfo<T> : XDRPCArgumentInfo where T : class
{
    // Token: 0x0600006E RID: 110 RVA: 0x00002F1F File Offset: 0x00001F1F
    public XDRPCArrayArgumentInfo(T v) : this(v, ArgumentType.ByValue, 0)
    {
    }

    // Token: 0x0600006F RID: 111 RVA: 0x00002F2A File Offset: 0x00001F2A
    public XDRPCArrayArgumentInfo(T v, ArgumentType t) : this(v, t, 0)
    {
    }

    // Token: 0x06000070 RID: 112 RVA: 0x00002F38 File Offset: 0x00001F38
    public XDRPCArrayArgumentInfo(T v, ArgumentType t, int maxArrayLength)
    {
        Type typeFromHandle = typeof(T);
        if (!typeFromHandle.IsArray)
        {
            throw new XDRPCInvalidTypeException(typeFromHandle, "Array type must be provided for XDRPCArrayArgumentInfo.");
        }
        if (!XDRPCMarshaler.IsValidArrayType(typeFromHandle))
        {
            throw new XDRPCInvalidTypeException(typeFromHandle, string.Format("Array type {0} is not supported by XDRPC.", typeFromHandle.Name));
        }
        this.Value = v;
        if (this.Value != null)
        {
            this._arrayLength = (this.Value as Array).Length;
        }
        else
        {
            this._arrayLength = 0;
        }
        if (t == ArgumentType.Out && maxArrayLength <= 0)
        {
            throw new XDRPCException("The max array length must be specified for an Out type array.");
        }
        this.PassBy = t;
        this._elementSize = MarshalingUtils.SizeOf(typeFromHandle.GetElementType());
        if (this.PassBy == ArgumentType.ByRef)
        {
            this.MaxArrayLength = Math.Max(this._arrayLength, maxArrayLength);
        }
        else if (this.PassBy == ArgumentType.Out)
        {
            this.MaxArrayLength = maxArrayLength;
        }
        else
        {
            this.MaxArrayLength = this._arrayLength;
        }
        this.ConstructArgumentInfoArray();
    }

    // Token: 0x1700000D RID: 13
    // (get) Token: 0x06000071 RID: 113 RVA: 0x0000302D File Offset: 0x0000202D
    // (set) Token: 0x06000072 RID: 114 RVA: 0x00003035 File Offset: 0x00002035
    public T Value { get; private set; }

    // Token: 0x1700000E RID: 14
    // (get) Token: 0x06000073 RID: 115 RVA: 0x0000303E File Offset: 0x0000203E
    // (set) Token: 0x06000074 RID: 116 RVA: 0x00003046 File Offset: 0x00002046
    public ArgumentType PassBy { get; private set; }

    // Token: 0x1700000F RID: 15
    // (get) Token: 0x06000075 RID: 117 RVA: 0x0000304F File Offset: 0x0000204F
    // (set) Token: 0x06000076 RID: 118 RVA: 0x00003057 File Offset: 0x00002057
    public int MaxArrayLength { get; private set; }

    // Token: 0x06000077 RID: 119 RVA: 0x00003060 File Offset: 0x00002060
    public ulong GetArgumentValue(ulong bufferAddress)
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByValue:
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                if (this.MaxArrayLength == 0)
                {
                    return 0UL;
                }
                return bufferAddress;
            default:
                return 0UL;
        }
    }

    // Token: 0x06000078 RID: 120 RVA: 0x00003097 File Offset: 0x00002097
    public int GetRequiredBufferSize()
    {
        return this.MaxArrayLength * this._elementSize;
    }

    // Token: 0x06000079 RID: 121 RVA: 0x000030A8 File Offset: 0x000020A8
    public byte[] PackBufferData()
    {
        byte[] array = null;
        if (this.MaxArrayLength > 0)
        {
            int num = 0;
            array = new byte[this._elementSize * this.MaxArrayLength];
            for (int i = 0; i < this.MaxArrayLength; i++)
            {
                MarshalingUtils.PushBufferData(this._arrayElementData[i].Info.PackBufferData(), array, ref num);
            }
        }
        return array;
    }

    // Token: 0x0600007A RID: 122 RVA: 0x00003108 File Offset: 0x00002108
    public void UnpackBufferData(byte[] data)
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            for (int i = 0; i < this.MaxArrayLength; i++)
            {
                byte[] array = new byte[this._elementSize];
                Array.Copy(data, i * this._elementSize, array, 0, this._elementSize);
                this._arrayElementData[i].Info.UnpackBufferData(array);
            }
            if (this._referenceSize == 0)
            {
                this.UpdateArrayValue();
            }
        }
    }

    // Token: 0x0600007B RID: 123 RVA: 0x0000317F File Offset: 0x0000217F
    public bool IsFloatingPointValue()
    {
        return false;
    }

    // Token: 0x0600007C RID: 124 RVA: 0x00003182 File Offset: 0x00002182
    public override string ToString()
    {
        return string.Format("[XDRPC]{0}: {1}", typeof(T).Name, this.Value);
    }

    // Token: 0x0600007D RID: 125 RVA: 0x000031A8 File Offset: 0x000021A8
    public int GetArgumentCount()
    {
        return 1;
    }

    // Token: 0x0600007E RID: 126 RVA: 0x000031AB File Offset: 0x000021AB
    public ulong[] GetArgumentValues(ulong bufferAddress)
    {
        return null;
    }

    // Token: 0x0600007F RID: 127 RVA: 0x000031B0 File Offset: 0x000021B0
    private void ConstructArgumentInfoArray()
    {
        Type elementType = typeof(T).GetElementType();
        Array array = this.Value as Array;
        this._arrayElementData = new XDRPCArrayArgumentInfo<T>.ArrayElementData[this.MaxArrayLength];
        int i = 0;
        int referenceSize = 0;
        while (i < this._arrayLength)
        {
            XDRPCArgumentInfo info = XDRPCMarshaler.GenerateArgumentInfo(elementType, array.GetValue(i), ArgumentType.ByRef);
            this._arrayElementData[i] = this.GenerateArrayBufferData(info, ref referenceSize);
            i++;
        }
        while (i < this.MaxArrayLength)
        {
            XDRPCArgumentInfo info2 = XDRPCMarshaler.GenerateArgumentInfo(elementType, Activator.CreateInstance(elementType), ArgumentType.ByRef);
            this._arrayElementData[i] = this.GenerateArrayBufferData(info2, ref referenceSize);
            i++;
        }
        if (this.MaxArrayLength > 0)
        {
            this._referenceSize = referenceSize;
            return;
        }
        this._referenceSize = 0;
    }

    // Token: 0x06000080 RID: 128 RVA: 0x00003280 File Offset: 0x00002280
    private XDRPCArrayArgumentInfo<T>.ArrayElementData GenerateArrayBufferData(XDRPCArgumentInfo info, ref int referenceOffset)
    {
        XDRPCArrayArgumentInfo<T>.ArrayElementData result = default(XDRPCArrayArgumentInfo<T>.ArrayElementData);
        result.Info = info;
        result.ReferenceOffset = referenceOffset;
        result.ReferenceSize = info.GetRequiredReferenceSize();
        referenceOffset += result.ReferenceSize;
        XDRPCExecutionState.AlignBufferOffset(ref referenceOffset);
        return result;
    }

    // Token: 0x06000081 RID: 129 RVA: 0x000032C8 File Offset: 0x000022C8
    public void SetReferenceAddress(ulong bufferAddress)
    {
        for (int i = 0; i < this.MaxArrayLength; i++)
        {
            this._arrayElementData[i].Info.SetReferenceAddress(bufferAddress + (ulong)((long)this._arrayElementData[i].ReferenceOffset));
        }
    }

    // Token: 0x06000082 RID: 130 RVA: 0x00003310 File Offset: 0x00002310
    public int GetRequiredReferenceSize()
    {
        return this._referenceSize;
    }

    // Token: 0x06000083 RID: 131 RVA: 0x00003318 File Offset: 0x00002318
    public byte[] PackReferenceData()
    {
        byte[] array = null;
        if (this._referenceSize > 0)
        {
            array = new byte[this._referenceSize];
            for (int i = 0; i < this.MaxArrayLength; i++)
            {
                byte[] array2 = this._arrayElementData[i].Info.PackReferenceData();
                if (array2 != null)
                {
                    int referenceOffset = this._arrayElementData[i].ReferenceOffset;
                    MarshalingUtils.PushBufferData(array2, array, ref referenceOffset);
                }
            }
        }
        return array;
    }

    // Token: 0x06000084 RID: 132 RVA: 0x00003384 File Offset: 0x00002384
    public void UnpackReferenceData(byte[] data)
    {
        for (int i = 0; i < this.MaxArrayLength; i++)
        {
            if (this._arrayElementData[i].ReferenceSize > 0)
            {
                byte[] array = new byte[this._arrayElementData[i].ReferenceSize];
                Array.Copy(data, this._arrayElementData[i].ReferenceOffset, array, 0, array.Length);
                this._arrayElementData[i].Info.UnpackReferenceData(array);
            }
        }
        this.UpdateArrayValue();
    }

    // Token: 0x06000085 RID: 133 RVA: 0x00003408 File Offset: 0x00002408
    private void UpdateArrayValue()
    {
        if (this.MaxArrayLength > 0)
        {
            Type elementType = typeof(T).GetElementType();
            Array array = Array.CreateInstance(elementType, this.MaxArrayLength);
            for (int i = 0; i < this.MaxArrayLength; i++)
            {
                array.SetValue(XDRPCMarshaler.GetArgumentInfoValue(elementType, this._arrayElementData[i].Info), i);
            }
            this.Value = (array as T);
            this._arrayLength = array.Length;
        }
    }

    // Token: 0x04000014 RID: 20
    private int _elementSize;

    // Token: 0x04000015 RID: 21
    private int _arrayLength;

    // Token: 0x04000016 RID: 22
    private int _referenceSize;

    // Token: 0x04000017 RID: 23
    private XDRPCArrayArgumentInfo<T>.ArrayElementData[] _arrayElementData;

    // Token: 0x0200000D RID: 13
    private struct ArrayElementData
    {
        // Token: 0x0400001B RID: 27
        public int ReferenceOffset;

        // Token: 0x0400001C RID: 28
        public int ReferenceSize;

        // Token: 0x0400001D RID: 29
        public XDRPCArgumentInfo Info;
    }
}

// Token: 0x02000009 RID: 9
public class XDRPCSizeOfArgumentInfo : XDRPCArgumentInfo
{
    // Token: 0x17000004 RID: 4
    // (get) Token: 0x0600002A RID: 42 RVA: 0x00002504 File Offset: 0x00001504
    // (set) Token: 0x0600002B RID: 43 RVA: 0x0000250C File Offset: 0x0000150C
    public XDRPCArgumentInfo TargetArgument { get; set; }

    // Token: 0x0600002C RID: 44 RVA: 0x00002515 File Offset: 0x00001515
    public XDRPCSizeOfArgumentInfo()
    {
        this.TargetArgument = null;
    }

    // Token: 0x0600002D RID: 45 RVA: 0x00002524 File Offset: 0x00001524
    public XDRPCSizeOfArgumentInfo(XDRPCArgumentInfo target)
    {
        this.TargetArgument = target;
    }

    // Token: 0x0600002E RID: 46 RVA: 0x00002533 File Offset: 0x00001533
    public ulong GetArgumentValue(ulong bufferAddress)
    {
        if (this.TargetArgument == null)
        {
            return 0UL;
        }
        return (ulong)((long)this.TargetArgument.GetRequiredBufferSize());
    }

    // Token: 0x0600002F RID: 47 RVA: 0x0000254C File Offset: 0x0000154C
    public int GetRequiredBufferSize()
    {
        return 0;
    }

    // Token: 0x06000030 RID: 48 RVA: 0x0000254F File Offset: 0x0000154F
    public byte[] PackBufferData()
    {
        return null;
    }

    // Token: 0x06000031 RID: 49 RVA: 0x00002552 File Offset: 0x00001552
    public void UnpackBufferData(byte[] data)
    {
    }

    // Token: 0x06000032 RID: 50 RVA: 0x00002554 File Offset: 0x00001554
    public override string ToString()
    {
        return string.Format("[XDRPC]SizeOf({0})", this.TargetArgument);
    }

    // Token: 0x06000033 RID: 51 RVA: 0x00002566 File Offset: 0x00001566
    public bool IsFloatingPointValue()
    {
        return false;
    }

    // Token: 0x06000034 RID: 52 RVA: 0x00002569 File Offset: 0x00001569
    public int GetArgumentCount()
    {
        return 1;
    }

    // Token: 0x06000035 RID: 53 RVA: 0x0000256C File Offset: 0x0000156C
    public ulong[] GetArgumentValues(ulong bufferAddress)
    {
        return new ulong[]
        {
            this.GetArgumentValue(bufferAddress)
        };
    }

    // Token: 0x17000005 RID: 5
    // (get) Token: 0x06000036 RID: 54 RVA: 0x0000258B File Offset: 0x0000158B
    public ArgumentType PassBy
    {
        get
        {
            return ArgumentType.ByValue;
        }
    }

    // Token: 0x06000037 RID: 55 RVA: 0x0000258E File Offset: 0x0000158E
    public void SetReferenceAddress(ulong bufferAddress)
    {
    }

    // Token: 0x06000038 RID: 56 RVA: 0x00002590 File Offset: 0x00001590
    public int GetRequiredReferenceSize()
    {
        return 0;
    }

    // Token: 0x06000039 RID: 57 RVA: 0x00002593 File Offset: 0x00001593
    public byte[] PackReferenceData()
    {
        return null;
    }

    // Token: 0x0600003A RID: 58 RVA: 0x00002596 File Offset: 0x00001596
    public void UnpackReferenceData(byte[] data)
    {
    }
}

// Token: 0x0200000B RID: 11
public class XDRPCStringArgumentInfo : XDRPCArgumentInfo
{
    // Token: 0x06000050 RID: 80 RVA: 0x00002C2F File Offset: 0x00001C2F
    public XDRPCStringArgumentInfo(string v) : this(v, Encoding.ASCII)
    {
    }

    // Token: 0x06000051 RID: 81 RVA: 0x00002C3D File Offset: 0x00001C3D
    public XDRPCStringArgumentInfo(string v, Encoding encoding) : this(v, encoding, ArgumentType.ByValue)
    {
    }

    // Token: 0x06000052 RID: 82 RVA: 0x00002C48 File Offset: 0x00001C48
    public XDRPCStringArgumentInfo(string v, Encoding encoding, ArgumentType t) : this(v, encoding, t, 0)
    {
    }

    // Token: 0x06000053 RID: 83 RVA: 0x00002C54 File Offset: 0x00001C54
    public XDRPCStringArgumentInfo(string v, Encoding encoding, ArgumentType t, int cchMax) : this(v, encoding, t, cchMax, XDRPCStringArgumentInfo.GetDefaultCountTypeForEncoding(encoding))
    {
    }

    // Token: 0x06000054 RID: 84 RVA: 0x00002C67 File Offset: 0x00001C67
    public XDRPCStringArgumentInfo(string v, Encoding encoding, ArgumentType t, int cMax, CountType ct)
    {
        this.Value = v;
        this.PassBy = t;
        this.Encoding = encoding;
        this.CchMax = cMax;
        this.MaxType = ct;
        this.CalculateBufferSize();
    }

    // Token: 0x17000008 RID: 8
    // (get) Token: 0x06000055 RID: 85 RVA: 0x00002C9A File Offset: 0x00001C9A
    // (set) Token: 0x06000056 RID: 86 RVA: 0x00002CA2 File Offset: 0x00001CA2
    public Encoding Encoding { get; private set; }

    // Token: 0x17000009 RID: 9
    // (get) Token: 0x06000057 RID: 87 RVA: 0x00002CAB File Offset: 0x00001CAB
    // (set) Token: 0x06000058 RID: 88 RVA: 0x00002CB3 File Offset: 0x00001CB3
    public string Value { get; private set; }

    // Token: 0x1700000A RID: 10
    // (get) Token: 0x06000059 RID: 89 RVA: 0x00002CBC File Offset: 0x00001CBC
    // (set) Token: 0x0600005A RID: 90 RVA: 0x00002CC4 File Offset: 0x00001CC4
    public ArgumentType PassBy { get; private set; }

    // Token: 0x1700000B RID: 11
    // (get) Token: 0x0600005B RID: 91 RVA: 0x00002CCD File Offset: 0x00001CCD
    // (set) Token: 0x0600005C RID: 92 RVA: 0x00002CD5 File Offset: 0x00001CD5
    public CountType MaxType { get; private set; }

    // Token: 0x1700000C RID: 12
    // (get) Token: 0x0600005D RID: 93 RVA: 0x00002CDE File Offset: 0x00001CDE
    // (set) Token: 0x0600005E RID: 94 RVA: 0x00002CE6 File Offset: 0x00001CE6
    public int CchMax { get; private set; }

    // Token: 0x0600005F RID: 95 RVA: 0x00002CF0 File Offset: 0x00001CF0
    public ulong GetArgumentValue(ulong bufferAddress)
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByValue:
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                if (this._maxBufferSize == 0)
                {
                    return 0UL;
                }
                return bufferAddress;
            default:
                return 0UL;
        }
    }

    // Token: 0x06000060 RID: 96 RVA: 0x00002D27 File Offset: 0x00001D27
    public int GetRequiredBufferSize()
    {
        return this._maxBufferSize;
    }

    // Token: 0x06000061 RID: 97 RVA: 0x00002D30 File Offset: 0x00001D30
    public byte[] PackBufferData()
    {
        byte[] array = null;
        if (this._maxBufferSize > 0)
        {
            if (this.Value != null)
            {
                array = this.Encoding.GetBytes(this.Value);
                if (BitConverter.IsLittleEndian && this.Encoding == Encoding.Unicode)
                {
                    MarshalingUtils.ReverseBytes(array, 2);
                }
            }
            else
            {
                array = new byte[this._maxBufferSize];
            }
        }
        return array;
    }

    // Token: 0x06000062 RID: 98 RVA: 0x00002D8C File Offset: 0x00001D8C
    public void UnpackBufferData(byte[] data)
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            if (BitConverter.IsLittleEndian && this.Encoding == Encoding.Unicode)
            {
                MarshalingUtils.ReverseBytes(data, 2);
            }
            this.Value = this.Encoding.GetString(data, 0, this.GetReturnedStringLength(data));
        }
    }

    // Token: 0x06000063 RID: 99 RVA: 0x00002DE0 File Offset: 0x00001DE0
    private int GetReturnedStringLength(byte[] data)
    {
        Encoding encoding = this.Encoding;
        char[] chars = new char[1];
        byte[] bytes = encoding.GetBytes(chars);
        for (int i = 0; i < data.Length; i += bytes.Length)
        {
            bool flag = true;
            for (int j = 0; j < bytes.Length; j++)
            {
                if (data[i + j] != bytes[j])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return i;
            }
        }
        return data.Length;
    }

    // Token: 0x06000064 RID: 100 RVA: 0x00002E3C File Offset: 0x00001E3C
    private void CalculateBufferSize()
    {
        int num = 0;
        if (this.Value != null)
        {
            num = this.Encoding.GetByteCount(this.Value + "\0");
        }
        int num2 = (this.MaxType == CountType.Element) ? this.Encoding.GetMaxByteCount(this.CchMax) : this.CchMax;
        if (this.PassBy == ArgumentType.ByRef)
        {
            num = Math.Max(num, num2);
        }
        else if (this.PassBy == ArgumentType.Out)
        {
            num = num2;
        }
        this._maxBufferSize = num;
        if (this.MaxType == CountType.Byte)
        {
            this.CchMax = this._maxBufferSize;
            return;
        }
        int byteCount = this.Encoding.GetByteCount("\0");
        this.CchMax = this._maxBufferSize / byteCount - byteCount;
    }

    // Token: 0x06000065 RID: 101 RVA: 0x00002EED File Offset: 0x00001EED
    internal static CountType GetDefaultCountTypeForEncoding(Encoding encoding)
    {
        if (encoding != Encoding.UTF8)
        {
            return CountType.Element;
        }
        return CountType.Byte;
    }

    // Token: 0x06000066 RID: 102 RVA: 0x00002EFA File Offset: 0x00001EFA
    public override string ToString()
    {
        return string.Format("[XDRPC]String: {0}", this.Value);
    }

    // Token: 0x06000067 RID: 103 RVA: 0x00002F0C File Offset: 0x00001F0C
    public bool IsFloatingPointValue()
    {
        return false;
    }

    // Token: 0x06000068 RID: 104 RVA: 0x00002F0F File Offset: 0x00001F0F
    public int GetArgumentCount()
    {
        return 1;
    }

    // Token: 0x06000069 RID: 105 RVA: 0x00002F12 File Offset: 0x00001F12
    public ulong[] GetArgumentValues(ulong bufferAddress)
    {
        return null;
    }

    // Token: 0x0600006A RID: 106 RVA: 0x00002F15 File Offset: 0x00001F15
    public void SetReferenceAddress(ulong bufferAddress)
    {
    }

    // Token: 0x0600006B RID: 107 RVA: 0x00002F17 File Offset: 0x00001F17
    public int GetRequiredReferenceSize()
    {
        return 0;
    }

    // Token: 0x0600006C RID: 108 RVA: 0x00002F1A File Offset: 0x00001F1A
    public byte[] PackReferenceData()
    {
        return null;
    }

    // Token: 0x0600006D RID: 109 RVA: 0x00002F1D File Offset: 0x00001F1D
    public void UnpackReferenceData(byte[] data)
    {
    }

    // Token: 0x0400000E RID: 14
    private int _maxBufferSize;
}

// Token: 0x0200000E RID: 14
public class XDRPCStructArgumentInfo<T> : XDRPCArgumentInfo where T : struct
{
    // Token: 0x06000086 RID: 134 RVA: 0x00003487 File Offset: 0x00002487
    public XDRPCStructArgumentInfo(T value) : this(value, ArgumentType.ByRef)
    {
    }

    // Token: 0x06000087 RID: 135 RVA: 0x00003494 File Offset: 0x00002494
    public XDRPCStructArgumentInfo(T value, ArgumentType t)
    {
        this.Value = value;
        this.PassBy = t;
        this.PrimitiveSize = this.PopulateStructBufferRecursive(typeof(T), this.Value, this._structBufferDataList, 0, string.Empty);
        this._structBufferSize = this.GetCurrentOffset(this._structBufferDataList, 1);
        this._referenceBufferSize = this.GetCurrentOffset(this._referenceBufferDataList, 8);
    }

    // Token: 0x17000010 RID: 16
    // (get) Token: 0x06000088 RID: 136 RVA: 0x0000351E File Offset: 0x0000251E
    // (set) Token: 0x06000089 RID: 137 RVA: 0x00003526 File Offset: 0x00002526
    public T Value { get; private set; }

    // Token: 0x17000011 RID: 17
    // (get) Token: 0x0600008A RID: 138 RVA: 0x0000352F File Offset: 0x0000252F
    // (set) Token: 0x0600008B RID: 139 RVA: 0x00003537 File Offset: 0x00002537
    public ArgumentType PassBy { get; private set; }

    // Token: 0x17000012 RID: 18
    // (get) Token: 0x0600008C RID: 140 RVA: 0x00003540 File Offset: 0x00002540
    public int Size
    {
        get
        {
            return this._structBufferSize;
        }
    }

    // Token: 0x17000013 RID: 19
    // (get) Token: 0x0600008D RID: 141 RVA: 0x00003548 File Offset: 0x00002548
    // (set) Token: 0x0600008E RID: 142 RVA: 0x00003550 File Offset: 0x00002550
    public int PrimitiveSize { get; private set; }

    // Token: 0x0600008F RID: 143 RVA: 0x00003559 File Offset: 0x00002559
    public override string ToString()
    {
        return string.Format("[XDRPC]{0}: {1}", typeof(T).Name, this.Value);
    }

    // Token: 0x06000090 RID: 144 RVA: 0x00003580 File Offset: 0x00002580
    public ulong GetArgumentValue(ulong bufferAddress)
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByValue:
                return this.GetArgumentValues(0UL)[0];
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                return bufferAddress;
            default:
                return 0UL;
        }
    }

    // Token: 0x06000091 RID: 145 RVA: 0x000035B8 File Offset: 0x000025B8
    public ulong[] GetArgumentValues(ulong bufferAddress)
    {
        if (this.PassBy == ArgumentType.ByValue)
        {
            int argumentCount = this.GetArgumentCount();
            byte[] array = this.PackBufferData(this._structBufferDataList, this._structBufferSize);
            ulong[] array2 = new ulong[argumentCount];
            for (int i = 0; i < argumentCount; i++)
            {
                byte[] array3 = new byte[8];
                int num = i * 8;
                int num2 = array.Length - num;
                if (num2 > array3.Length)
                {
                    num2 = array3.Length;
                }
                Array.Copy(array, num, array3, 0, num2);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array3);
                }
                array2[i] = BitConverter.ToUInt64(array3, 0);
            }
            return array2;
        }
        return null;
    }

    // Token: 0x06000092 RID: 146 RVA: 0x00003646 File Offset: 0x00002646
    public int GetArgumentCount()
    {
        if (this.PassBy == ArgumentType.ByValue)
        {
            return (this._structBufferSize + 8 - 1) / 8;
        }
        return 1;
    }

    // Token: 0x06000093 RID: 147 RVA: 0x00003660 File Offset: 0x00002660
    public int GetRequiredBufferSize()
    {
        switch (this.PassBy)
        {
            case ArgumentType.ByValue:
                return 0;
            case ArgumentType.ByRef:
            case ArgumentType.Out:
                return this._structBufferSize;
            default:
                return 0;
        }
    }

    // Token: 0x06000094 RID: 148 RVA: 0x00003692 File Offset: 0x00002692
    public byte[] PackBufferData()
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            return this.PackBufferData(this._structBufferDataList, this._structBufferSize);
        }
        return null;
    }

    // Token: 0x06000095 RID: 149 RVA: 0x000036BC File Offset: 0x000026BC
    public void UnpackBufferData(byte[] incomingData)
    {
        if (this.PassBy == ArgumentType.ByRef || this.PassBy == ArgumentType.Out)
        {
            this.UnpackBufferData(this._structBufferDataList, incomingData);
            if (this._referenceBufferSize == 0)
            {
                List<XDRPCStructArgumentInfo<T>.StructBufferData>.Enumerator enumerator = this._structBufferDataList.GetEnumerator();
                enumerator.MoveNext();
                this.Value = (T)((object)this.UnpackValuesRecursive(typeof(T), this.Value, ref enumerator));
            }
        }
    }

    // Token: 0x06000096 RID: 150 RVA: 0x0000372C File Offset: 0x0000272C
    public bool IsFloatingPointValue()
    {
        return false;
    }

    // Token: 0x06000097 RID: 151 RVA: 0x0000372F File Offset: 0x0000272F
    public void SetReferenceAddress(ulong bufferAddress)
    {
        this.PopulateReferencePointers(this._structBufferDataList, bufferAddress);
        this.PopulateReferencePointers(this._referenceBufferDataList, bufferAddress);
    }

    // Token: 0x06000098 RID: 152 RVA: 0x0000374B File Offset: 0x0000274B
    public int GetRequiredReferenceSize()
    {
        return this._referenceBufferSize;
    }

    // Token: 0x06000099 RID: 153 RVA: 0x00003753 File Offset: 0x00002753
    public byte[] PackReferenceData()
    {
        return this.PackBufferData(this._referenceBufferDataList, this._referenceBufferSize);
    }

    // Token: 0x0600009A RID: 154 RVA: 0x00003768 File Offset: 0x00002768
    public void UnpackReferenceData(byte[] incomingData)
    {
        this.UnpackBufferData(this._referenceBufferDataList, incomingData);
        List<XDRPCStructArgumentInfo<T>.StructBufferData>.Enumerator enumerator = this._structBufferDataList.GetEnumerator();
        enumerator.MoveNext();
        this.Value = (T)((object)this.UnpackValuesRecursive(typeof(T), this.Value, ref enumerator));
    }

    // Token: 0x0600009B RID: 155 RVA: 0x000037C0 File Offset: 0x000027C0
    private byte[] PackBufferData(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int bufferSize)
    {
        byte[] array = null;
        if (bufferSize > 0)
        {
            array = new byte[bufferSize];
            foreach (XDRPCStructArgumentInfo<T>.StructBufferData structBufferData in bufferDataList)
            {
                if (!structBufferData.Ignore)
                {
                    byte[] array2 = structBufferData.Info.PackBufferData();
                    int length = (array2.Length < structBufferData.BufferSize) ? array2.Length : structBufferData.BufferSize;
                    Array.Copy(array2, 0, array, structBufferData.BufferOffset, length);
                }
            }
        }
        return array;
    }

    // Token: 0x0600009C RID: 156 RVA: 0x00003858 File Offset: 0x00002858
    private void UnpackBufferData(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, byte[] incomingData)
    {
        foreach (XDRPCStructArgumentInfo<T>.StructBufferData structBufferData in bufferDataList)
        {
            byte[] array = new byte[structBufferData.BufferSize];
            Array.Copy(incomingData, structBufferData.BufferOffset, array, 0, structBufferData.BufferSize);
            structBufferData.Info.UnpackBufferData(array);
        }
    }

    // Token: 0x0600009D RID: 157 RVA: 0x000038D0 File Offset: 0x000028D0
    private int PopulateStructBufferRecursive(Type structType, object structToMarshal, List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int depth, string unionWritingField)
    {
        if (depth > 3)
        {
            throw new XDRPCInvalidTypeException(structType, string.Format("Struct of type {0} has too many nested structs (more than {1} deep) which is not supported.", typeof(T).Name, 3));
        }
        if (!XDRPCStructArgumentInfo<T>.builtInStructAllowlist.Contains(structType))
        {
            object[] customAttributes = structType.GetCustomAttributes(typeof(XDRPCStructAttribute), false);
            if (customAttributes == null || customAttributes.Length == 0)
            {
                throw new XDRPCInvalidTypeException(structType, string.Format("Struct of type {0} doesn't have the XDRPCStruct attribute.", structType.Name));
            }
        }
        FieldInfo[] fields = structType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        int packAttribute = (structType.StructLayoutAttribute.Value == LayoutKind.Sequential) ? 8 : structType.StructLayoutAttribute.Pack;
        int num = 0;
        int num2 = 0;
        bool flag = false;
        foreach (FieldInfo fieldInfo in fields)
        {
            Type fieldType = fieldInfo.FieldType;
            MarshalAsAttribute marshalAsAttribute = null;
            XDRPCStructArgumentInfo<T>.StructBufferData structBufferData = default(XDRPCStructArgumentInfo<T>.StructBufferData);
            object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            if (customAttributes2 != null && customAttributes2.Length != 0)
            {
                marshalAsAttribute = (MarshalAsAttribute)customAttributes2[0];
            }
            if (fieldType == typeof(string) || fieldType.IsArray)
            {
                if (marshalAsAttribute == null)
                {
                    throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} of type {1} in struct type {2} doesn't have the required MarshalAsAttribute.", fieldInfo.Name, fieldType.Name, structType.Name));
                }
                if (marshalAsAttribute.Value != UnmanagedType.ByValArray && marshalAsAttribute.Value != UnmanagedType.LPArray)
                {
                    throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} of type {1} in struct type {2} with its MarshalAs attribute not using the required UnmanagedType.ByValArray or UnmanagedType.LPArray.", fieldInfo.Name, fieldType.Name, structType.Name));
                }
                bool flag2 = marshalAsAttribute.Value == UnmanagedType.LPArray;
                int num3 = marshalAsAttribute.SizeConst;
                int num5;
                int num6;
                if (fieldType.IsArray)
                {
                    if (fieldType.GetArrayRank() > 1)
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is a multidimensional array which is not supported by XDRPCStructArgumentInfo.", fieldInfo.Name, structType.Name));
                    }
                    Type elementType = fieldType.GetElementType();
                    if (!XDRPCMarshaler.IsValidValueType(elementType))
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is an array of type {2} which is not supported by XDRPCStructArgumentInfo.", fieldInfo.Name, structType.Name, elementType.Name));
                    }
                    int num4 = MarshalingUtils.SizeOf(elementType);
                    if (num4 == 0)
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is an array of type {2} which is not supported by XDRPCStructArgumentInfo.", fieldInfo.Name, structType.Name, elementType.Name));
                    }
                    num5 = num4;
                    if (XDRPCMarshaler.IsValidStructType(elementType))
                    {
                        num5 = MarshalingUtils.AlignmentOf(elementType);
                    }
                    Array array2 = (Array)fieldInfo.GetValue(structToMarshal);
                    if (flag2)
                    {
                        if (array2 != null && num3 < array2.Length)
                        {
                            num3 = array2.Length;
                        }
                    }
                    else if (num3 == 0)
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is an array being passed by value that doesn't have the required SizeConst part of the attribute defined.", fieldInfo.Name, structType.Name));
                    }
                    num6 = num4 * num3;
                    structBufferData.Info = XDRPCMarshaler.GenerateArgumentInfo(fieldType, array2, ArgumentType.ByRef, num3);
                }
                else
                {
                    if (!XDRPCStructArgumentInfo<T>.EncodingMap.ContainsKey(marshalAsAttribute.ArraySubType))
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is a string with a MarshalAs attribute without a string UnmanagedType for its ArraySubType.", fieldInfo.Name, structType.Name));
                    }
                    Encoding encoding = XDRPCStructArgumentInfo<T>.EncodingMap[marshalAsAttribute.ArraySubType];
                    int num4 = (encoding == Encoding.Unicode) ? 2 : 1;
                    num5 = num4;
                    num6 = num3 * num4;
                    string text = (string)fieldInfo.GetValue(structToMarshal);
                    if (flag2)
                    {
                        int num7 = 0;
                        if (text != null)
                        {
                            num7 = encoding.GetByteCount(text + "\0");
                        }
                        if (num6 < num7)
                        {
                            num6 = num7;
                        }
                    }
                    else if (num6 == 0)
                    {
                        throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is a string being passed by value that doesn't have the required SizeConst part of the attribute defined.", fieldInfo.Name, structType.Name));
                    }
                    structBufferData.Info = new XDRPCStringArgumentInfo(text, encoding, ArgumentType.ByRef, num6, CountType.Byte);
                }
                int requiredReferenceSize = structBufferData.Info.GetRequiredReferenceSize();
                if (requiredReferenceSize > 0)
                {
                    XDRPCStructArgumentInfo<T>.StructBufferData item = default(XDRPCStructArgumentInfo<T>.StructBufferData);
                    item.BufferOffset = this.GetCurrentOffset(this._referenceBufferDataList, 8);
                    item.BufferSize = requiredReferenceSize;
                    item.ReferenceIndex = -1;
                    item.Info = null;
                    structBufferData.ReferenceIndex = this._referenceBufferDataList.Count;
                    this._referenceBufferDataList.Add(item);
                }
                else
                {
                    structBufferData.ReferenceIndex = -1;
                }
                structBufferData.BufferSize = num6;
                if (flag2)
                {
                    if (num6 > 0)
                    {
                        XDRPCStructArgumentInfo<T>.StructBufferData item2 = structBufferData;
                        item2.BufferOffset = this.GetCurrentOffset(this._referenceBufferDataList, num5);
                        this.FillPointerBufferData(out structBufferData, bufferDataList, packAttribute);
                        this._referenceBufferDataList.Add(item2);
                    }
                    else
                    {
                        this.FillPointerBufferData(out structBufferData, bufferDataList, packAttribute);
                        structBufferData.Info = new XDRPCArgumentInfo<uint>(0U, ArgumentType.ByRef);
                        structBufferData.ReferenceIndex = -1;
                    }
                    num5 = structBufferData.BufferSize;
                }
                else
                {
                    int packAlignment = this.CalculatePackAlignment(packAttribute, num5);
                    structBufferData.BufferOffset = this.GetCurrentOffset(bufferDataList, packAlignment);
                }
                num = Math.Max(num, num5);
                num2 = Math.Max(num2, structBufferData.BufferSize);
                if (this.FillUnionBufferData(ref structBufferData, fieldInfo, unionWritingField))
                {
                    flag = true;
                }
                bufferDataList.Add(structBufferData);
            }
            else if (fieldType.IsPrimitive)
            {
                int num8 = MarshalingUtils.SizeOf(fieldType);
                if (num8 == 0)
                {
                    throw new XDRPCInvalidTypeException(fieldType, string.Format("Field {0} in struct type {1} is primitive type {2} which is not supported by XDRPCStructArgumentInfo.", fieldInfo.Name, structType.Name, fieldType.Name));
                }
                structBufferData.Info = XDRPCMarshaler.GenerateArgumentInfo(fieldType, fieldInfo.GetValue(structToMarshal), ArgumentType.ByRef);
                if (marshalAsAttribute != null && marshalAsAttribute.Value == UnmanagedType.LPStruct)
                {
                    XDRPCStructArgumentInfo<T>.StructBufferData item3 = default(XDRPCStructArgumentInfo<T>.StructBufferData);
                    item3.Info = structBufferData.Info;
                    item3.BufferOffset = this.GetCurrentOffset(this._referenceBufferDataList, num8);
                    item3.BufferSize = num8;
                    item3.ReferenceIndex = -1;
                    this.FillPointerBufferData(out structBufferData, bufferDataList, packAttribute);
                    this._referenceBufferDataList.Add(item3);
                }
                else
                {
                    int packAlignment2 = this.CalculatePackAlignment(packAttribute, num8);
                    structBufferData.BufferOffset = this.GetCurrentOffset(bufferDataList, packAlignment2);
                    structBufferData.BufferSize = num8;
                    structBufferData.ReferenceIndex = -1;
                }
                num = Math.Max(num, structBufferData.BufferSize);
                num2 = Math.Max(num2, structBufferData.BufferSize);
                if (this.FillUnionBufferData(ref structBufferData, fieldInfo, unionWritingField))
                {
                    flag = true;
                }
                bufferDataList.Add(structBufferData);
            }
            else
            {
                if (!fieldType.IsValueType)
                {
                    throw new XDRPCInvalidTypeException(fieldType, string.Format("Type {0} found in struct type {1} is not supported by XDRPCStructArgumentInfo.", fieldType.Name, structType.Name));
                }
                List<XDRPCStructArgumentInfo<T>.StructBufferData> list = new List<XDRPCStructArgumentInfo<T>.StructBufferData>();
                string unionWritingField2 = string.Empty;
                customAttributes2 = fieldInfo.GetCustomAttributes(typeof(XDRPCUnionAttribute), false);
                if (customAttributes2 != null && customAttributes2.Length != 0)
                {
                    unionWritingField2 = ((XDRPCUnionAttribute)customAttributes2[0]).Value;
                }
                int num9 = this.PopulateStructBufferRecursive(fieldType, fieldInfo.GetValue(structToMarshal), list, depth + 1, unionWritingField2);
                if (marshalAsAttribute != null && marshalAsAttribute.Value == UnmanagedType.LPStruct)
                {
                    this.FillPointerBufferData(out structBufferData, bufferDataList, packAttribute);
                    if (this.FillUnionBufferData(ref structBufferData, fieldInfo, unionWritingField))
                    {
                        flag = true;
                    }
                    bufferDataList.Add(structBufferData);
                    int currentOffset = this.GetCurrentOffset(this._referenceBufferDataList, 8);
                    for (int j = 0; j < list.Count; j++)
                    {
                        XDRPCStructArgumentInfo<T>.StructBufferData item4 = list[j];
                        item4.BufferOffset += currentOffset;
                        if (item4.NextOffset != 0)
                        {
                            item4.NextOffset += currentOffset;
                        }
                        this._referenceBufferDataList.Add(item4);
                    }
                    num = structBufferData.BufferSize;
                }
                else
                {
                    structBufferData.BufferOffset = this.GetCurrentOffset(bufferDataList, this.CalculatePackAlignment(packAttribute, num9));
                    if (this.FillUnionBufferData(ref structBufferData, fieldInfo, unionWritingField))
                    {
                        flag = true;
                    }
                    for (int k = 0; k < list.Count; k++)
                    {
                        XDRPCStructArgumentInfo<T>.StructBufferData item5 = list[k];
                        item5.BufferOffset += structBufferData.BufferOffset;
                        if (item5.NextOffset != 0)
                        {
                            structBufferData.BufferSize = item5.NextOffset;
                            item5.NextOffset += structBufferData.BufferOffset;
                        }
                        if (structBufferData.Ignore)
                        {
                            item5.Ignore = true;
                        }
                        bufferDataList.Add(item5);
                    }
                }
                num = Math.Max(num, num9);
                num2 = Math.Max(num2, structBufferData.BufferSize);
            }
        }
        if (!string.IsNullOrEmpty(unionWritingField))
        {
            if (!flag)
            {
                throw new XDRPCInvalidTypeException(structType, string.Format("Struct of type {0} has the XDRPCUnion attribute with the non-existant field {1} specified, please provide a correct field name.", structType.Name, unionWritingField));
            }
            this.SetNextOffset(bufferDataList, num2);
        }
        int num10 = this.CalculatePackAlignment(packAttribute, num);
        this.CalculateNextOffset(bufferDataList, num10);
        return num10;
    }

    // Token: 0x0600009E RID: 158 RVA: 0x000040E4 File Offset: 0x000030E4
    private int GetCurrentOffset(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int packAlignment)
    {
        int num = 0;
        if (bufferDataList.Count > 0)
        {
            XDRPCStructArgumentInfo<T>.StructBufferData structBufferData = bufferDataList[bufferDataList.Count - 1];
            num = structBufferData.NextOffset;
            if (num == 0)
            {
                num = structBufferData.BufferOffset + structBufferData.BufferSize;
            }
            num = this.AlignOffset(num, packAlignment);
        }
        return num;
    }

    // Token: 0x0600009F RID: 159 RVA: 0x00004130 File Offset: 0x00003130
    private void SetNextOffset(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int nextOffset)
    {
        if (bufferDataList.Count > 0)
        {
            int index = bufferDataList.Count - 1;
            XDRPCStructArgumentInfo<T>.StructBufferData value = bufferDataList[index];
            value.NextOffset = nextOffset;
            bufferDataList[index] = value;
        }
    }

    // Token: 0x060000A0 RID: 160 RVA: 0x00004168 File Offset: 0x00003168
    private void CalculateNextOffset(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int packAlignment)
    {
        if (bufferDataList.Count > 0)
        {
            int index = bufferDataList.Count - 1;
            XDRPCStructArgumentInfo<T>.StructBufferData value = bufferDataList[index];
            int num = value.NextOffset;
            if (num == 0)
            {
                num = value.BufferOffset + value.BufferSize;
            }
            value.NextOffset = this.AlignOffset(num, packAlignment);
            bufferDataList[index] = value;
        }
    }

    // Token: 0x060000A1 RID: 161 RVA: 0x000041C1 File Offset: 0x000031C1
    private int CalculatePackAlignment(int packAttribute, int primitiveSize)
    {
        return Math.Min(packAttribute, primitiveSize);
    }

    // Token: 0x060000A2 RID: 162 RVA: 0x000041CC File Offset: 0x000031CC
    private int AlignOffset(int currOffset, int packAlignment)
    {
        int num = currOffset % packAlignment;
        if (num != 0)
        {
            currOffset += packAlignment - num;
        }
        return currOffset;
    }

    // Token: 0x060000A3 RID: 163 RVA: 0x000041E8 File Offset: 0x000031E8
    private void FillPointerBufferData(out XDRPCStructArgumentInfo<T>.StructBufferData data, List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, int packAttribute)
    {
        data = default(XDRPCStructArgumentInfo<T>.StructBufferData);
        data.BufferSize = MarshalingUtils.SizeOf(typeof(uint));
        int packAlignment = this.CalculatePackAlignment(packAttribute, data.BufferSize);
        data.BufferOffset = this.GetCurrentOffset(bufferDataList, packAlignment);
        data.ReferenceIndex = this._referenceBufferDataList.Count;
        data.Info = null;
    }

    // Token: 0x060000A4 RID: 164 RVA: 0x00004248 File Offset: 0x00003248
    private bool FillUnionBufferData(ref XDRPCStructArgumentInfo<T>.StructBufferData data, FieldInfo fieldInfo, string unionWritingField)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(unionWritingField))
        {
            data.BufferOffset = 0;
            if (unionWritingField != fieldInfo.Name)
            {
                data.Ignore = true;
            }
            else
            {
                result = true;
            }
        }
        return result;
    }

    // Token: 0x060000A5 RID: 165 RVA: 0x00004280 File Offset: 0x00003280
    private void PopulateReferencePointers(List<XDRPCStructArgumentInfo<T>.StructBufferData> bufferDataList, ulong bufferAddress)
    {
        for (int i = 0; i < bufferDataList.Count; i++)
        {
            XDRPCStructArgumentInfo<T>.StructBufferData value = bufferDataList[i];
            if (value.Info == null)
            {
                if (value.ReferenceIndex != -1)
                {
                    uint v = (uint)((long)this._referenceBufferDataList[value.ReferenceIndex].BufferOffset + (long)bufferAddress);
                    value.Info = new XDRPCArgumentInfo<uint>(v, ArgumentType.ByRef);
                    bufferDataList[i] = value;
                }
            }
            else if (value.Info.GetRequiredReferenceSize() > 0)
            {
                XDRPCStructArgumentInfo<T>.StructBufferData value2 = this._referenceBufferDataList[value.ReferenceIndex];
                ulong referenceAddress = (ulong)((long)value2.BufferOffset + (long)bufferAddress);
                value.Info.SetReferenceAddress(referenceAddress);
                value2.Info = new XDRPCArrayArgumentInfo<byte[]>(value.Info.PackReferenceData(), ArgumentType.ByRef);
                this._referenceBufferDataList[value.ReferenceIndex] = value2;
            }
        }
    }

    // Token: 0x060000A6 RID: 166 RVA: 0x00004360 File Offset: 0x00003360
    private object UnpackValuesRecursive(Type structType, object structToMarshal, ref List<XDRPCStructArgumentInfo<T>.StructBufferData>.Enumerator bufferEnum)
    {
        FieldInfo[] fields = structType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo fieldInfo in fields)
        {
            Type fieldType = fieldInfo.FieldType;
            XDRPCStructArgumentInfo<T>.StructBufferData structBufferData = bufferEnum.Current;
            if (fieldType == typeof(string) || fieldType.IsArray || fieldType.IsPrimitive)
            {
                if (structBufferData.ReferenceIndex != -1)
                {
                    if (structBufferData.Info.GetRequiredReferenceSize() == 0)
                    {
                        structBufferData = this._referenceBufferDataList[structBufferData.ReferenceIndex];
                    }
                    if (structBufferData.Info.GetRequiredReferenceSize() > 0)
                    {
                        XDRPCArrayArgumentInfo<byte[]> xdrpcarrayArgumentInfo = (XDRPCArrayArgumentInfo<byte[]>)this._referenceBufferDataList[structBufferData.ReferenceIndex].Info;
                        structBufferData.Info.UnpackReferenceData(xdrpcarrayArgumentInfo.Value);
                    }
                }
                object value = this.ValidateArgInfoNullValue(fieldType, structBufferData.Info);
                fieldInfo.SetValue(structToMarshal, value);
                bufferEnum.MoveNext();
            }
            else if (fieldType.IsValueType)
            {
                object value2;
                if (structBufferData.ReferenceIndex != -1)
                {
                    List<XDRPCStructArgumentInfo<T>.StructBufferData>.Enumerator enumerator = this._referenceBufferDataList.GetEnumerator();
                    do
                    {
                        enumerator.MoveNext();
                    }
                    while (enumerator.Current.Info != this._referenceBufferDataList[structBufferData.ReferenceIndex].Info);
                    value2 = this.UnpackValuesRecursive(fieldType, fieldInfo.GetValue(structToMarshal), ref enumerator);
                    bufferEnum.MoveNext();
                }
                else
                {
                    value2 = this.UnpackValuesRecursive(fieldType, fieldInfo.GetValue(structToMarshal), ref bufferEnum);
                }
                fieldInfo.SetValue(structToMarshal, value2);
            }
        }
        return structToMarshal;
    }

    // Token: 0x060000A7 RID: 167 RVA: 0x000044DC File Offset: 0x000034DC
    private object ValidateArgInfoNullValue(Type fieldType, XDRPCArgumentInfo argInfo)
    {
        if (argInfo.GetType().IsGenericType)
        {
            Type type = argInfo.GetType().GetGenericArguments()[0];
            if (!type.Equals(fieldType) && type.Equals(typeof(uint)) && ((XDRPCArgumentInfo<uint>)argInfo).Value == 0U)
            {
                return null;
            }
        }
        return XDRPCMarshaler.GetArgumentInfoValue(fieldType, argInfo);
    }

    // Token: 0x0400001E RID: 30
    private const int MaxRecursionDepth = 3;

    // Token: 0x0400001F RID: 31
    private const int DefaultPackingValue = 8;

    // Token: 0x04000020 RID: 32
    private const int NoReferenceIndex = -1;

    // Token: 0x04000021 RID: 33
    private const BindingFlags StructFieldFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // Token: 0x04000022 RID: 34
    private int _structBufferSize;

    // Token: 0x04000023 RID: 35
    private int _referenceBufferSize;

    // Token: 0x04000024 RID: 36
    private List<XDRPCStructArgumentInfo<T>.StructBufferData> _structBufferDataList = new List<XDRPCStructArgumentInfo<T>.StructBufferData>();

    // Token: 0x04000025 RID: 37
    private List<XDRPCStructArgumentInfo<T>.StructBufferData> _referenceBufferDataList = new List<XDRPCStructArgumentInfo<T>.StructBufferData>();

    // Token: 0x04000026 RID: 38
    private static Dictionary<UnmanagedType, Encoding> EncodingMap = new Dictionary<UnmanagedType, Encoding>
    {
        {
            UnmanagedType.LPStr,
            Encoding.ASCII
        },
        {
            UnmanagedType.LPWStr,
            Encoding.Unicode
        },
        {
            UnmanagedType.LPTStr,
            Encoding.UTF8
        }
    };

    // Token: 0x04000027 RID: 39
    private static Type[] builtInStructAllowlist = new Type[]
    {
        typeof(Guid)
    };

    // Token: 0x0200000F RID: 15
    private struct StructBufferData
    {
        // Token: 0x0400002B RID: 43
        public int BufferOffset;

        // Token: 0x0400002C RID: 44
        public int BufferSize;

        // Token: 0x0400002D RID: 45
        public int NextOffset;

        // Token: 0x0400002E RID: 46
        public XDRPCArgumentInfo Info;

        // Token: 0x0400002F RID: 47
        public int ReferenceIndex;

        // Token: 0x04000030 RID: 48
        public bool Ignore;
    }
}

// Token: 0x02000005 RID: 5
[AttributeUsage(AttributeTargets.Struct)]
public class XDRPCStructAttribute : Attribute
{
}