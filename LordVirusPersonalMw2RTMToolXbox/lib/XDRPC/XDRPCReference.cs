using System;
using System.Text;

using XDevkit;

namespace XDRPC;

// Token: 0x0200001D RID: 29
public enum XDRPCPostMethodCall
{
    // Token: 0x04000044 RID: 68
    None,
    // Token: 0x04000045 RID: 69
    GetLastError
}

// Token: 0x02000025 RID: 37
public class XDRPCReference : IDisposable
{
    // Token: 0x06000179 RID: 377 RVA: 0x00006949 File Offset: 0x00005949
    public XDRPCReference(IXboxConsole console, uint pointer, int size)
    {
        this.XboxConsole = console;
        this.BufferSize = size;
        this.Pointer = pointer;
        this._xboxName = console.Name;
    }

    // Token: 0x0600017A RID: 378 RVA: 0x00006974 File Offset: 0x00005974
    ~XDRPCReference()
    {
        try
        {
            bool xdrpcmanaged = this.XDRPCManaged;
        }
        finally
        {
            this.Dispose();
        }
    }

    // Token: 0x1700002F RID: 47
    // (get) Token: 0x0600017B RID: 379 RVA: 0x000069A4 File Offset: 0x000059A4
    // (set) Token: 0x0600017C RID: 380 RVA: 0x000069AC File Offset: 0x000059AC
    public int BufferSize { get; internal set; }

    // Token: 0x17000030 RID: 48
    // (get) Token: 0x0600017D RID: 381 RVA: 0x000069B5 File Offset: 0x000059B5
    // (set) Token: 0x0600017E RID: 382 RVA: 0x000069BD File Offset: 0x000059BD
    public uint Pointer { get; internal set; }

    // Token: 0x17000031 RID: 49
    // (get) Token: 0x0600017F RID: 383 RVA: 0x000069C6 File Offset: 0x000059C6
    // (set) Token: 0x06000180 RID: 384 RVA: 0x000069CE File Offset: 0x000059CE
    public IXboxConsole XboxConsole { get; internal set; }

    // Token: 0x17000032 RID: 50
    // (get) Token: 0x06000181 RID: 385 RVA: 0x000069D7 File Offset: 0x000059D7
    // (set) Token: 0x06000182 RID: 386 RVA: 0x000069DF File Offset: 0x000059DF
    public bool XDRPCManaged
    {
        get
        {
            return this._xdrpcManaged;
        }
        set
        {
            if (this._xdrpcManaged != value)
            {
                this._xdrpcManaged = value;
                if (this._xdrpcManaged)
                {
                    XDRPCReferenceMonitor.AddReference(this);
                    return;
                }
                XDRPCReferenceMonitor.ReleaseReference(this);
            }
        }
    }

    // Token: 0x06000183 RID: 387 RVA: 0x00006A06 File Offset: 0x00005A06
    public void Dispose()
    {
        if (this.XDRPCManaged)
        {
            this.Free();
        }
        GC.SuppressFinalize(this);
    }

    // Token: 0x06000184 RID: 388 RVA: 0x00006A1C File Offset: 0x00005A1C
    public XDRPCArgumentInfo<uint> GetArgumentInfo()
    {
        return new XDRPCArgumentInfo<uint>(this.Pointer);
    }

    // Token: 0x06000185 RID: 389 RVA: 0x00006A2C File Offset: 0x00005A2C
    public void Set<T>(T data)
    {
        this.ValidatePointer("Set");
        Type typeFromHandle = typeof(T);
        this.ValidateType(typeFromHandle, "Set");
        int arrayElementCount = this.GetArrayElementCount(typeFromHandle);
        XDRPCArgumentInfo lpvBufArg = XDRPCMarshaler.GenerateArgumentInfo(typeFromHandle, data, ArgumentType.ByRef, arrayElementCount);
        this.Set(lpvBufArg);
    }

    // Token: 0x06000186 RID: 390 RVA: 0x00006A7C File Offset: 0x00005A7C
    public void Set(string str, Encoding encoding)
    {
        this.ValidatePointer("Set");
        int stringBufferSize = this.GetStringBufferSize(encoding);
        XDRPCArgumentInfo lpvBufArg = new XDRPCStringArgumentInfo(str, encoding, ArgumentType.ByRef, stringBufferSize, CountType.Byte);
        this.Set(lpvBufArg);
    }

    // Token: 0x06000187 RID: 391 RVA: 0x00006AB0 File Offset: 0x00005AB0
    public T Get<T>()
    {
        this.ValidatePointer("Get");
        Type typeFromHandle = typeof(T);
        this.ValidateType(typeFromHandle, "Get");
        int arrayElementCount = this.GetArrayElementCount(typeFromHandle);
        XDRPCArgumentInfo xdrpcargumentInfo = XDRPCMarshaler.GenerateArgumentInfo(typeFromHandle, default(T), ArgumentType.Out, arrayElementCount);
        this.Get(xdrpcargumentInfo);
        return (T)((object)XDRPCMarshaler.GetArgumentInfoValue(typeFromHandle, xdrpcargumentInfo));
    }

    // Token: 0x06000188 RID: 392 RVA: 0x00006B14 File Offset: 0x00005B14
    public string Get(Encoding encoding)
    {
        this.ValidatePointer("Get");
        int stringBufferSize = this.GetStringBufferSize(encoding);
        XDRPCStringArgumentInfo xdrpcstringArgumentInfo = new XDRPCStringArgumentInfo(string.Empty, encoding, ArgumentType.Out, stringBufferSize, CountType.Byte);
        this.Get(xdrpcstringArgumentInfo);
        return xdrpcstringArgumentInfo.Value;
    }

    // Token: 0x06000189 RID: 393 RVA: 0x00006B50 File Offset: 0x00005B50
    public void Free()
    {
        if (!this.XDRPCManaged)
        {
            throw new XDRPCInvalidOperationException("Invalid free operation: cannot free XboxReference that doesn't have explict user ownership.");
        }
        this.XboxConsole.ExecuteRPC<int>(XDRPCMode.System, "xbdm.xex", 9, new object[]
        {
            this.Pointer
        });
        this.XDRPCManaged = false;
        this.Pointer = 0U;
        this.XboxConsole = null;
        this.BufferSize = 0;
    }

    // Token: 0x0600018A RID: 394 RVA: 0x00006BB6 File Offset: 0x00005BB6
    private void ValidatePointer(string msg)
    {
        if (this.Pointer == 0U)
        {
            throw new XDRPCInvalidOperationException(string.Format("Invalid {0} operation: Attempting to use XboxReference with a null pointer.", msg));
        }
    }

    // Token: 0x0600018B RID: 395 RVA: 0x00006BD1 File Offset: 0x00005BD1
    private void ValidateType(Type t, string msg)
    {
        if (!XDRPCMarshaler.IsValidArgumentType(t))
        {
            throw new XDRPCInvalidTypeException(t, string.Format("Invalid {0} operation: Type {1} is not supported by XDRPC.", msg, t.Name));
        }
    }

    // Token: 0x0600018C RID: 396 RVA: 0x00006BF4 File Offset: 0x00005BF4
    private int ValidateSize(XDRPCArgumentInfo lpvBufArg, string msgType)
    {
        int requiredBufferSize = lpvBufArg.GetRequiredBufferSize();
        if (requiredBufferSize > this.BufferSize)
        {
            throw new XDRPCInvalidOperationException(string.Format("Invalid {0} operation: Allocated buffer's size ({1}) is smaller than object's size ({2}).", msgType, this.BufferSize, requiredBufferSize));
        }
        if (lpvBufArg.GetRequiredReferenceSize() > 0)
        {
            throw new XDRPCInvalidOperationException(string.Format("Invalid {0} operation: {1}", msgType, "Struct type containing references is not supported by the XDRPC allocation system. You will need to use the XDRPC allocation system to create the data for the references on the Xbox and change the struct to have uints filled with the XDRPCReference.Pointer values for that data instead of the references. See the How to Use XDRPC documentation for more info."));
        }
        return requiredBufferSize;
    }

    // Token: 0x0600018D RID: 397 RVA: 0x00006C53 File Offset: 0x00005C53
    private void ValidateReturn(int hr, string msg)
    {
        if (hr < 0)
        {
            throw new XDRPCException(string.Format("Failure during {0} operation: HRESULT returned is 0x{1:x}", msg, hr));
        }
    }

    // Token: 0x0600018E RID: 398 RVA: 0x00006C70 File Offset: 0x00005C70
    private void ValidateReturnSize(int actual, int expected, string msg)
    {
        if (actual != expected)
        {
            throw new XDRPCException(string.Format("Failure during {0} operation: Incorrect amount of bytes accessed. Expected {1} and got {2}.", msg, expected, actual));
        }
    }

    // Token: 0x0600018F RID: 399 RVA: 0x00006C94 File Offset: 0x00005C94
    private void Set(XDRPCArgumentInfo lpvBufArg)
    {
        int num = this.ValidateSize(lpvBufArg, "Set");
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(XDRPCMode.System, "xbdm.xex", 40);
        XDRPCArgumentInfo<uint> argumentInfo = this.GetArgumentInfo();
        XDRPCArgumentInfo<int> xdrpcargumentInfo = new XDRPCArgumentInfo<int>(num);
        XDRPCArgumentInfo<int> xdrpcargumentInfo2 = new XDRPCArgumentInfo<int>(0, ArgumentType.Out);
        int hr = this.XboxConsole.ExecuteRPC<int>(options, new XDRPCArgumentInfo[]
        {
            argumentInfo,
            xdrpcargumentInfo,
            lpvBufArg,
            xdrpcargumentInfo2
        });
        this.ValidateReturn(hr, "Set");
        this.ValidateReturnSize(xdrpcargumentInfo2.Value, num, "Set");
    }

    // Token: 0x06000190 RID: 400 RVA: 0x00006D20 File Offset: 0x00005D20
    private void Get(XDRPCArgumentInfo lpvBufArg)
    {
        int num = this.ValidateSize(lpvBufArg, "Get");
        XDRPCExecutionOptions options = new XDRPCExecutionOptions(XDRPCMode.System, "xbdm.xex", 10);
        XDRPCArgumentInfo<uint> argumentInfo = this.GetArgumentInfo();
        XDRPCArgumentInfo<int> xdrpcargumentInfo = new XDRPCArgumentInfo<int>(num);
        XDRPCArgumentInfo<int> xdrpcargumentInfo2 = new XDRPCArgumentInfo<int>(0, ArgumentType.Out);
        int hr = this.XboxConsole.ExecuteRPC<int>(options, new XDRPCArgumentInfo[]
        {
            argumentInfo,
            xdrpcargumentInfo,
            lpvBufArg,
            xdrpcargumentInfo2
        });
        this.ValidateReturn(hr, "Get");
        this.ValidateReturnSize(xdrpcargumentInfo2.Value, num, "Get");
    }

    // Token: 0x06000191 RID: 401 RVA: 0x00006DAC File Offset: 0x00005DAC
    private int GetArrayElementCount(Type type)
    {
        int result = 0;
        if (type.IsArray)
        {
            int num = MarshalingUtils.SizeOf(type.GetElementType());
            if (num != 0)
            {
                result = this.BufferSize / num;
            }
        }
        return result;
    }

    // Token: 0x06000192 RID: 402 RVA: 0x00006DDC File Offset: 0x00005DDC
    private int GetStringBufferSize(Encoding encoding)
    {
        int num = this.BufferSize;
        if (encoding == Encoding.Unicode)
        {
            num -= num % 2;
        }
        return num;
    }

    // Token: 0x04000088 RID: 136
    internal const string StructWithRefError = "Struct type containing references is not supported by the XDRPC allocation system. You will need to use the XDRPC allocation system to create the data for the references on the Xbox and change the struct to have uints filled with the XDRPCReference.Pointer values for that data instead of the references. See the How to Use XDRPC documentation for more info.";

    // Token: 0x04000089 RID: 137
    private bool _xdrpcManaged;

    // Token: 0x0400008A RID: 138
    private string _xboxName;
}
