using System;
using System.Collections.Generic;
using System.Text;
using XDevkit;

namespace XDRPC;

// Token: 0x02000026 RID: 38
public static class XDRPCReferenceMonitor
{
    // Token: 0x06000193 RID: 403 RVA: 0x00006DFF File Offset: 0x00005DFF
    public static bool CheckLeakedXDRPCReferences()
    {
        return XDRPCReferenceMonitor.CheckLeakedXDRPCReferences(false);
    }

    // Token: 0x06000194 RID: 404 RVA: 0x00006E08 File Offset: 0x00005E08
    public static bool CheckLeakedXDRPCReferences(bool cleanup)
    {
        bool result = false;
        if (XDRPCReferenceMonitor._references.Count > 0)
        {
            result = true;
            List<XDRPCReference> list = new List<XDRPCReference>();
            StringBuilder stringBuilder = new StringBuilder("XboxReference leaks found...");
            stringBuilder.AppendLine();
            foreach (KeyValuePair<IXboxConsole, Dictionary<uint, XDRPCReference>> keyValuePair in XDRPCReferenceMonitor._references)
            {
                stringBuilder.AppendFormat("Leaks for console {0}:", keyValuePair.Key.Name);
                stringBuilder.AppendLine();
                foreach (KeyValuePair<uint, XDRPCReference> keyValuePair2 in keyValuePair.Value)
                {
                    stringBuilder.AppendFormat("{0,5} bytes at memory location 0x{1,8:X8}.", keyValuePair2.Value.BufferSize, keyValuePair2.Key);
                    stringBuilder.AppendLine();
                    if (cleanup)
                    {
                        list.Add(keyValuePair2.Value);
                    }
                }
            }
            foreach (XDRPCReference xdrpcreference in list)
            {
                try
                {
                    xdrpcreference.Free();
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendFormat("!!!Exception caught while cleaning up leak {0} on console {1}: {2}", xdrpcreference.Pointer, xdrpcreference.XboxConsole, ex.ToString());
                    stringBuilder.AppendLine();
                }
            }
            Console.WriteLine(stringBuilder.ToString());
        }
        return result;
    }

    // Token: 0x06000195 RID: 405 RVA: 0x00006FB0 File Offset: 0x00005FB0
    public static XDRPCReference AllocateRPC(this IXboxConsole console, int size)
    {
        return console.AllocateRPC(size, XDRPCMode.System);
    }

    // Token: 0x06000196 RID: 406 RVA: 0x00006FBC File Offset: 0x00005FBC
    public static XDRPCReference AllocateRPC(this IXboxConsole console, int size, XDRPCMode mode)
    {
        if (size == 0)
        {
            throw new XDRPCInvalidOperationException("Allocating size of 0 is not allowed.");
        }
        uint num = console.ExecuteRPC<uint>(mode, "xbdm.xex", 1, new object[]
        {
            size
        });
        if (num == 0U)
        {
            throw new XDRPCException("Failed allocation: null pointer was returned.");
        }
        return new XDRPCReference(console, num, size)
        {
            XDRPCManaged = true
        };
    }

    // Token: 0x06000197 RID: 407 RVA: 0x00007015 File Offset: 0x00006015
    public static XDRPCReference AllocateRPC(this IXboxConsole console, XDRPCArgumentInfo lpvBufArg)
    {
        return console.AllocateRPC(lpvBufArg, XDRPCMode.System);
    }

    // Token: 0x06000198 RID: 408 RVA: 0x0000701F File Offset: 0x0000601F
    public static XDRPCReference AllocateRPC(this IXboxConsole console, XDRPCArgumentInfo lpvBufArg, XDRPCMode mode)
    {
        if (lpvBufArg.PassBy == ArgumentType.ByValue)
        {
            throw new XDRPCInvalidOperationException("Allocating XDRPCArgumentInfo with ByValue argument type is not allowed.");
        }
        if (lpvBufArg.GetRequiredReferenceSize() > 0)
        {
            throw new XDRPCInvalidOperationException("Struct type containing references is not supported by the XDRPC allocation system. You will need to use the XDRPC allocation system to create the data for the references on the Xbox and change the struct to have uints filled with the XDRPCReference.Pointer values for that data instead of the references. See the How to Use XDRPC documentation for more info.");
        }
        return console.AllocateRPC(lpvBufArg.GetRequiredBufferSize(), mode);
    }

    // Token: 0x06000199 RID: 409 RVA: 0x00007055 File Offset: 0x00006055
    public static XDRPCReference AllocateRPC<T>(this IXboxConsole console) where T : struct
    {
        return console.AllocateRPC<T>(XDRPCMode.System);
    }

    // Token: 0x0600019A RID: 410 RVA: 0x00007060 File Offset: 0x00006060
    public static XDRPCReference AllocateRPC<T>(this IXboxConsole console, XDRPCMode mode) where T : struct
    {
        Type typeFromHandle = typeof(T);
        if (!XDRPCMarshaler.IsValidArgumentType(typeFromHandle))
        {
            throw new XDRPCInvalidTypeException(typeFromHandle, string.Format("Invalid type {0}: Cannot allocate type not supported by XDRPC.", typeFromHandle.Name));
        }
        XDRPCArgumentInfo lpvBufArg = XDRPCMarshaler.GenerateArgumentInfo(typeFromHandle, default(T), ArgumentType.ByRef);
        return console.AllocateRPC(lpvBufArg, mode);
    }

    // Token: 0x0600019B RID: 411 RVA: 0x000070B8 File Offset: 0x000060B8
    internal static void AddReference(XDRPCReference reference)
    {
        IXboxConsole xboxConsole = reference.XboxConsole;
        uint pointer = reference.Pointer;
        if (!XDRPCReferenceMonitor._references.ContainsKey(xboxConsole))
        {
            XDRPCReferenceMonitor._references.Add(xboxConsole, new Dictionary<uint, XDRPCReference>());
        }
        Dictionary<uint, XDRPCReference> dictionary = XDRPCReferenceMonitor._references[xboxConsole];
        if (!dictionary.ContainsKey(pointer))
        {
            dictionary.Add(pointer, reference);
            return;
        }
        throw new XDRPCInvalidOperationException(string.Format("Invalid operation: Pointer {0} is being referred to by two seperate XboxReferences.", pointer));
    }

    // Token: 0x0600019C RID: 412 RVA: 0x00007124 File Offset: 0x00006124
    internal static void ReleaseReference(XDRPCReference reference)
    {
        IXboxConsole xboxConsole = reference.XboxConsole;
        uint pointer = reference.Pointer;
        if (!XDRPCReferenceMonitor._references.ContainsKey(xboxConsole))
        {
            throw new XDRPCInvalidOperationException("Invalid operation: attempting to release reference that is not registered.");
        }
        Dictionary<uint, XDRPCReference> dictionary = XDRPCReferenceMonitor._references[xboxConsole];
        if (!dictionary.ContainsKey(pointer))
        {
            throw new XDRPCInvalidOperationException("Invalid operation: attempting to release reference that is not registered.");
        }
        dictionary.Remove(pointer);
        if (dictionary.Count == 0)
        {
            XDRPCReferenceMonitor._references.Remove(xboxConsole);
        }
    }

    // Token: 0x0400008E RID: 142
    private static Dictionary<IXboxConsole, Dictionary<uint, XDRPCReference>> _references = new Dictionary<IXboxConsole, Dictionary<uint, XDRPCReference>>();
}
