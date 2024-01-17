using LordVirusPersonalMw2RTMToolXbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using XDevkit;

namespace XDRPC;

// Token: 0x0200001C RID: 28
public enum XDRPCMode
{
    // Token: 0x04000041 RID: 65
    System,
    // Token: 0x04000042 RID: 66
    Title
}

// Token: 0x02000024 RID: 36
public static class XDRPCMarshaler
{
    // Token: 0x06000159 RID: 345 RVA: 0x00005F54 File Offset: 0x00004F54
    private static XDRPCArgumentInfo[] GenerateArgumentInfoArray(params object[] args)
    {
        XDRPCArgumentInfo[] array = new XDRPCArgumentInfo[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            object obj = args[i];
            if (obj == null)
            {
                array[i] = new XDRPCNullArgumentInfo();
            }
            else
            {
                Type type = obj.GetType();
                if (typeof(XDRPCArgumentInfo).IsAssignableFrom(type))
                {
                    array[i] = (XDRPCArgumentInfo)obj;
                }
                else
                {
                    if (!XDRPCMarshaler.IsValidArgumentType(type))
                    {
                        throw new XDRPCInvalidArgumentTypeException(type, i);
                    }
                    array[i] = XDRPCMarshaler.GenerateArgumentInfo(type, obj);
                }
            }
        }
        return array;
    }

    // Token: 0x0600015A RID: 346 RVA: 0x00005FCC File Offset: 0x00004FCC
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCMode mode, string module, int ordinal, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(mode, module, ordinal);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x0600015B RID: 347 RVA: 0x00005FF4 File Offset: 0x00004FF4
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, string module, int ordinal, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, module, ordinal);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x0600015C RID: 348 RVA: 0x0000601C File Offset: 0x0000501C
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, string module, int ordinal, out ulong postMethodCallReturn, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, module, ordinal);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, out postMethodCallReturn, args2);
    }

    // Token: 0x0600015D RID: 349 RVA: 0x00006044 File Offset: 0x00005044
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCMode mode, string module, string functionName, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(mode, module, functionName);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x0600015E RID: 350 RVA: 0x0000606C File Offset: 0x0000506C
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, string module, string functionName, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, module, functionName);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x0600015F RID: 351 RVA: 0x00006094 File Offset: 0x00005094
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, string module, string functionName, out ulong postMethodCallReturn, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, module, functionName);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, out postMethodCallReturn, args2);
    }

    // Token: 0x06000160 RID: 352 RVA: 0x000060BC File Offset: 0x000050BC
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCMode mode, uint functionAddress, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(mode, functionAddress);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x06000161 RID: 353 RVA: 0x000060E0 File Offset: 0x000050E0
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, uint functionAddress, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, functionAddress);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x06000162 RID: 354 RVA: 0x00006104 File Offset: 0x00005104
    public static T ExecuteRPC<T>(this IXboxConsole console, string threadName, uint functionAddress, out ulong postMethodCallReturn, params object[] args) where T : struct
    {
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(threadName, functionAddress);
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, out postMethodCallReturn, args2);
    }

    // Token: 0x06000163 RID: 355 RVA: 0x0000612C File Offset: 0x0000512C
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options) where T : struct
    {
        XDRPCArgumentInfo[] args = new XDRPCArgumentInfo[0];
        return console.ExecuteRPC<T>(options, args);
    }

    // Token: 0x06000164 RID: 356 RVA: 0x00006148 File Offset: 0x00005148
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options, params object[] args) where T : struct
    {
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, args2);
    }

    // Token: 0x06000165 RID: 357 RVA: 0x00006164 File Offset: 0x00005164
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options, params XDRPCArgumentInfo[] args) where T : struct
    {
        ulong num;
        return console.ExecuteRPC<T>(options, out num, args);
    }

    // Token: 0x06000166 RID: 358 RVA: 0x0000617C File Offset: 0x0000517C
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options, out ulong postMethodCallReturn) where T : struct
    {
        XDRPCArgumentInfo[] args = new XDRPCArgumentInfo[0];
        return console.ExecuteRPC<T>(options, out postMethodCallReturn, args);
    }

    // Token: 0x06000167 RID: 359 RVA: 0x0000619C File Offset: 0x0000519C
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options, out ulong postMethodCallReturn, params object[] args) where T : struct
    {
        XDRPCArgumentInfo[] args2 = XDRPCMarshaler.GenerateArgumentInfoArray(args);
        return console.ExecuteRPC<T>(options, out postMethodCallReturn, args2);
    }

    // Token: 0x06000168 RID: 360 RVA: 0x000061BC File Offset: 0x000051BC
    public static T ExecuteRPC<T>(this IXboxConsole console, XDRPCExecutionOptions options, out ulong postMethodCallReturn, params XDRPCArgumentInfo[] args) where T : struct
    {
        Type typeFromHandle = typeof(T);
        postMethodCallReturn = 0UL;
        if (!XDRPCMarshaler.IsValidReturnType(typeFromHandle))
        {
            if (typeFromHandle.IsValueType)
            {
                XDRPCStructArgumentInfo<T> xdrpcstructArgumentInfo = new XDRPCStructArgumentInfo<T>(default(T), ArgumentType.Out);
                XDRPCArgumentInfo[] array = new XDRPCArgumentInfo[args.Length + 1];
                array[0] = xdrpcstructArgumentInfo;
                Array.Copy(args, 0, array, 1, args.Length);
                console.ExecuteRPC<T>(options, array);
                return xdrpcstructArgumentInfo.Value;
            }
            throw new XDRPCInvalidReturnTypeException(typeFromHandle);
        }
        else
        {
            XDRPCExecutionState.XDRPCCallFlags flags = XDRPCExecutionState.XDRPCCallFlags.IntegerReturn;
            if (typeFromHandle == typeof(float) || typeFromHandle == typeof(double))
            {
                flags = XDRPCExecutionState.XDRPCCallFlags.FloatingPointReturn;
            }
            XDRPCExecutionState xdrpcexecutionState = new XDRPCExecutionState(console, options, args, flags);
            xdrpcexecutionState.NoDevkit = XDRPCMarshaler.NoDevkit;
            xdrpcexecutionState.Invoke();
            if (options.PostMethodCall != XDRPCPostMethodCall.None)
            {
                postMethodCallReturn = xdrpcexecutionState.PostMethodCallReturnValue;
            }
            object obj = XDRPCMarshaler.UnpackReturnType(typeFromHandle, xdrpcexecutionState.ReturnValue);
            if (obj == null)
            {
                return default(T);
            }
            return (T)((object)obj);
        }
    }

    // Token: 0x06000169 RID: 361 RVA: 0x000062A0 File Offset: 0x000052A0
    public static bool SupportsRPC(this IXboxConsole console)
    {
        bool result = true;
        try
        {
            console.ExecuteRPC<int>(XDRPCMode.System, "xbdm.xex", 117, new object[]
            {
                0
            });
        }
        catch (XDRPCException)
        {
            result = false;
        }
        return result;
    }

    // Token: 0x0600016A RID: 362 RVA: 0x000062E8 File Offset: 0x000052E8
    internal static bool IsValidReturnType(Type t)
    {
        return XDRPCMarshaler.ValidReturnTypes.Contains(t);
    }

    // Token: 0x0600016B RID: 363 RVA: 0x000062F5 File Offset: 0x000052F5
    internal static bool IsValidArgumentType(Type t)
    {
        return XDRPCMarshaler.IsValidValueType(t) || XDRPCMarshaler.IsValidArrayType(t) || typeof(string) == t;
    }

    // Token: 0x0600016C RID: 364 RVA: 0x00006318 File Offset: 0x00005318
    internal static bool IsValidArrayType(Type t)
    {
        bool result = false;
        if (t.IsArray)
        {
            result = (t.GetArrayRank() == 1 && XDRPCMarshaler.IsValidValueType(t.GetElementType()));
        }
        return result;
    }

    // Token: 0x0600016D RID: 365 RVA: 0x00006348 File Offset: 0x00005348
    internal static bool IsValidValueType(Type t)
    {
        return t.IsValueType && t != typeof(char);
    }

    // Token: 0x0600016E RID: 366 RVA: 0x00006364 File Offset: 0x00005364
    internal static bool IsValidStructType(Type t)
    {
        return !t.IsPrimitive && t.IsValueType;
    }

    // Token: 0x0600016F RID: 367 RVA: 0x00006376 File Offset: 0x00005376
    internal static XDRPCArgumentInfo GenerateArgumentInfo(Type t, object o)
    {
        return XDRPCMarshaler.GenerateArgumentInfo(t, o, ArgumentType.ByValue);
    }

    // Token: 0x06000170 RID: 368 RVA: 0x00006380 File Offset: 0x00005380
    internal static XDRPCArgumentInfo GenerateArgumentInfo(Type t, object o, ArgumentType at)
    {
        return XDRPCMarshaler.GenerateArgumentInfo(t, o, at, 0);
    }

    // Token: 0x06000171 RID: 369 RVA: 0x0000638B File Offset: 0x0000538B
    internal static XDRPCArgumentInfo GenerateArgumentInfo(Type t, object o, ArgumentType at, int size)
    {
        return XDRPCMarshaler.GenerateArgumentInfo(t, o, at, size, Encoding.ASCII);
    }

    // Token: 0x06000172 RID: 370 RVA: 0x0000639C File Offset: 0x0000539C
    internal static XDRPCArgumentInfo GenerateArgumentInfo(Type t, object o, ArgumentType at, int size, Encoding encoding)
    {
        XDRPCArgumentInfo result = null;
        if (t.IsPrimitive || t.IsValueType)
        {
            Type type;
            if (t.IsPrimitive)
            {
                type = typeof(XDRPCArgumentInfo<>).MakeGenericType(new Type[]
                {
                    t
                });
            }
            else
            {
                type = typeof(XDRPCStructArgumentInfo<>).MakeGenericType(new Type[]
                {
                    t
                });
            }
            ConstructorInfo constructor = type.GetConstructor(new Type[]
            {
                t,
                typeof(ArgumentType)
            });
            result = (XDRPCArgumentInfo)constructor.Invoke(new object[]
            {
                o,
                at
            });
        }
        else if (XDRPCMarshaler.IsValidArrayType(t))
        {
            Type type = typeof(XDRPCArrayArgumentInfo<>).MakeGenericType(new Type[]
            {
                t
            });
            ConstructorInfo constructor2 = type.GetConstructor(new Type[]
            {
                t,
                typeof(ArgumentType),
                typeof(int)
            });
            result = (XDRPCArgumentInfo)constructor2.Invoke(new object[]
            {
                o,
                at,
                size
            });
        }
        else if (typeof(string) == t)
        {
            result = new XDRPCStringArgumentInfo((string)o, encoding, at, size, XDRPCStringArgumentInfo.GetDefaultCountTypeForEncoding(encoding));
        }
        return result;
    }

    // Token: 0x06000173 RID: 371 RVA: 0x00006508 File Offset: 0x00005508
    internal static object GetArgumentInfoValue(Type t, XDRPCArgumentInfo argInfo)
    {
        object result = null;
        if (typeof(string) == t)
        {
            result = ((XDRPCStringArgumentInfo)argInfo).Value;
        }
        else if (t.IsPrimitive || t.IsValueType || XDRPCMarshaler.IsValidArrayType(t))
        {
            Type type;
            if (t.IsPrimitive)
            {
                type = typeof(XDRPCArgumentInfo<>).MakeGenericType(new Type[]
                {
                    t
                });
            }
            else if (t.IsValueType)
            {
                type = typeof(XDRPCStructArgumentInfo<>).MakeGenericType(new Type[]
                {
                    t
                });
            }
            else
            {
                type = typeof(XDRPCArrayArgumentInfo<>).MakeGenericType(new Type[]
                {
                    t
                });
            }
            PropertyInfo property = type.GetProperty("Value");
            result = property.GetValue(argInfo, null);
        }
        return result;
    }

    // Token: 0x06000174 RID: 372 RVA: 0x000065D4 File Offset: 0x000055D4
    private static object UnpackReturnType(Type t, ulong v)
    {
        if (t == typeof(bool))
        {
            return (v & 65535UL) != 0UL;
        }
        if (t == typeof(byte))
        {
            return (byte)v;
        }
        if (t == typeof(short))
        {
            return (short)v;
        }
        if (t == typeof(int))
        {
            return (int)v;
        }
        if (t == typeof(long))
        {
            return (long)v;
        }
        if (t == typeof(ushort))
        {
            return (ushort)v;
        }
        if (t == typeof(uint))
        {
            return (uint)v;
        }
        if (t == typeof(ulong))
        {
            return v;
        }
        if (t == typeof(float))
        {
            return (float)BitConverter.Int64BitsToDouble((long)v);
        }
        if (t == typeof(double))
        {
            return BitConverter.Int64BitsToDouble((long)v);
        }
        return null;
    }

    // Token: 0x06000175 RID: 373 RVA: 0x000066F0 File Offset: 0x000056F0
    public static TDelegate CreateDelegate<TDelegate>(this IXboxConsole console, XDRPCMode mode, string module, int ordinal)
    {
        if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
        {
            throw new XDRPCInvalidArgumentTypeException(typeof(TDelegate), -1);
        }
        Type typeFromHandle = typeof(TDelegate);
        MethodInfo method = typeFromHandle.GetMethod("Invoke");
        ParameterExpression[] array = (from paramInfo in method.GetParameters()
                                       select Expression.Parameter(paramInfo.ParameterType, paramInfo.Name)).ToArray<ParameterExpression>();
        Expression[] initializers = (from paramInfo in array
                                     select Expression.Convert(paramInfo, typeof(object))).ToArray<UnaryExpression>();
        Type type = method.ReturnType;
        if (method.ReturnType == typeof(void))
        {
            type = typeof(uint);
        }
        MethodInfo method2 = typeof(XDRPCMarshaler).GetMethod("ExecuteRPC", new Type[]
        {
            typeof(IXboxConsole),
            typeof(XDRPCMode),
            typeof(string),
            typeof(int),
            typeof(object[])
        }).MakeGenericMethod(new Type[]
        {
            type
        });
        Expression[] arguments = new Expression[]
        {
            Expression.Constant(console),
            Expression.Constant(mode),
            Expression.Constant(module),
            Expression.Constant(ordinal),
            Expression.NewArrayInit(typeof(object), initializers)
        };
        Expression body = Expression.Call(method2, arguments);
        return Expression.Lambda<TDelegate>(body, array).Compile();
    }

    // Token: 0x04000085 RID: 133
    public const int MaxArgumentsSupported = 8;

    // Token: 0x04000086 RID: 134
    public static bool NoDevkit = false;

    // Token: 0x04000087 RID: 135
    private static HashSet<Type> ValidReturnTypes = new HashSet<Type>
    {
        typeof(bool),
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double)
    };
}
