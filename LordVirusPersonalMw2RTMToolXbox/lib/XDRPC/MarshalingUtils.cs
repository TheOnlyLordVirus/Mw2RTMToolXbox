using System;
using System.Collections.Generic;
using System.Reflection;

namespace XDRPC
{
    // Token: 0x02000002 RID: 2
    internal static class MarshalingUtils
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00001050
        internal static void ReverseBytes(byte[] buffer)
        {
            int i = 0;
            int num = buffer.Length - 1;
            while (i < num)
            {
                byte b = buffer[i];
                buffer[i] = buffer[num];
                buffer[num] = b;
                i++;
                num--;
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002084 File Offset: 0x00001084
        internal static void ReverseBytes(byte[] buffer, int groupSize)
        {
            if (buffer.Length % groupSize != 0)
            {
                throw new ArgumentException("Group size must be a multiple of the buffer length", "groupSize");
            }
            for (int i = 0; i < buffer.Length; i += groupSize)
            {
                int j = i;
                int num = i + groupSize - 1;
                while (j < num)
                {
                    byte b = buffer[j];
                    buffer[j] = buffer[num];
                    buffer[num] = b;
                    j++;
                    num--;
                }
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020DC File Offset: 0x000010DC
        internal static byte[] GetBigEndianBytes(ulong v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian)
            {
                MarshalingUtils.ReverseBytes(bytes);
            }
            return bytes;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002100 File Offset: 0x00001100
        internal static ulong GetUInt64FromBigEndianBytes(byte[] buffer, int start)
        {
            int num = MarshalingUtils.SizeOf(typeof(ulong));
            if (start + num > buffer.Length)
            {
                throw new XDRPCException("Error de-serializing UInt64 from buffer");
            }
            byte[] array = new byte[num];
            Array.Copy(buffer, start, array, 0, num);
            if (BitConverter.IsLittleEndian)
            {
                MarshalingUtils.ReverseBytes(array);
            }
            return BitConverter.ToUInt64(array, 0);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002158 File Offset: 0x00001158
        internal static void PushCallData(ulong value, byte[] buffer, ref int offset)
        {
            byte[] bigEndianBytes = MarshalingUtils.GetBigEndianBytes(value);
            Array.Copy(bigEndianBytes, 0, buffer, offset, bigEndianBytes.Length);
            offset += bigEndianBytes.Length;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002181 File Offset: 0x00001181
        internal static void PushBufferData(byte[] value, byte[] buffer, ref int offset)
        {
            Array.Copy(value, 0, buffer, offset, value.Length);
            offset += value.Length;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002198 File Offset: 0x00001198
        internal static ulong ConvertToUInt64(object o)
        {
            if (o is bool)
            {
                if (!(bool)o)
                {
                    return 0UL;
                }
                return 1UL;
            }
            else
            {
                if (o is byte)
                {
                    return (ulong)((byte)o);
                }
                if (o is short)
                {
                    return (ulong)((long)((short)o));
                }
                if (o is int)
                {
                    return (ulong)((long)((int)o));
                }
                if (o is long)
                {
                    return (ulong)((long)o);
                }
                if (o is ushort)
                {
                    return (ulong)((ushort)o);
                }
                if (o is uint)
                {
                    return (ulong)((uint)o);
                }
                if (o is ulong)
                {
                    return (ulong)o;
                }
                if (o is float)
                {
                    return (ulong)BitConverter.DoubleToInt64Bits((double)((float)o));
                }
                if (o is double)
                {
                    return (ulong)BitConverter.DoubleToInt64Bits((double)o);
                }
                return 0UL;
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002254 File Offset: 0x00001254
        internal static int SizeOf(Type type)
        {
            int result = 0;
            if (MarshalingUtils.ValueTypeSizeMap.ContainsKey(type))
            {
                result = MarshalingUtils.ValueTypeSizeMap[type];
            }
            else if (XDRPCMarshaler.IsValidStructType(type))
            {
                MarshalingUtils.GetStructSizes(type);
                result = MarshalingUtils.ValueTypeSizeMap[type];
            }
            return result;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x0000229C File Offset: 0x0000129C
        internal static int AlignmentOf(Type type)
        {
            int result = 0;
            if (MarshalingUtils.StructPrimitiveSizeMap.ContainsKey(type))
            {
                result = MarshalingUtils.StructPrimitiveSizeMap[type];
            }
            else if (XDRPCMarshaler.IsValidStructType(type))
            {
                MarshalingUtils.GetStructSizes(type);
                result = MarshalingUtils.StructPrimitiveSizeMap[type];
            }
            else if (type.IsPrimitive)
            {
                result = MarshalingUtils.SizeOf(type);
            }
            return result;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000022F4 File Offset: 0x000012F4
        private static void GetStructSizes(Type type)
        {
            if (XDRPCMarshaler.IsValidStructType(type))
            {
                Type type2 = typeof(XDRPCStructArgumentInfo<>).MakeGenericType(new Type[]
                {
                    type
                });
                ConstructorInfo constructor = type2.GetConstructor(new Type[]
                {
                    type,
                    typeof(ArgumentType)
                });
                object obj = constructor.Invoke(new object[]
                {
                    Activator.CreateInstance(type),
                    ArgumentType.ByRef
                });
                PropertyInfo property = type2.GetProperty("Size");
                PropertyInfo property2 = type2.GetProperty("PrimitiveSize");
                MarshalingUtils.ValueTypeSizeMap.Add(type, (int)property.GetValue(obj, null));
                MarshalingUtils.StructPrimitiveSizeMap.Add(type, (int)property2.GetValue(obj, null));
            }
        }

        // Token: 0x04000001 RID: 1
        private static Dictionary<Type, int> ValueTypeSizeMap = new Dictionary<Type, int>
        {
            {
                typeof(bool),
                4
            },
            {
                typeof(byte),
                1
            },
            {
                typeof(short),
                2
            },
            {
                typeof(int),
                4
            },
            {
                typeof(long),
                8
            },
            {
                typeof(ushort),
                2
            },
            {
                typeof(uint),
                4
            },
            {
                typeof(ulong),
                8
            },
            {
                typeof(float),
                4
            },
            {
                typeof(double),
                8
            }
        };

        // Token: 0x04000002 RID: 2
        private static Dictionary<Type, int> StructPrimitiveSizeMap = new Dictionary<Type, int>();
    }
}
