using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using XDevkit;

namespace XDRPC;

// Token: 0x02000021 RID: 33
internal class XDRPCExecutionState
{
    // Token: 0x06000144 RID: 324 RVA: 0x000051C4 File Offset: 0x000041C4
    internal XDRPCExecutionState(IXboxConsole console, XDRPCExecutionOptions options, XDRPCArgumentInfo[] arguments, XDRPCExecutionState.XDRPCCallFlags flags)
    {
        this.Console = console;
        this.Options = options;
        this.ReturnValue = 0UL;
        this.PostMethodCallReturnValue = 0UL;
        this.callFlags = flags;
        List<XDRPCArgumentInfo> list = new List<XDRPCArgumentInfo>();
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].IsFloatingPointValue())
            {
                list.Add(arguments[i]);
                arguments[i] = new XDRPCNullArgumentInfo();
            }
        }
        this.IntegerArguments = arguments;
        this.FloatingPointArguments = list.ToArray();
        this.totalArgumentCount = 0;
        for (int j = 0; j < this.IntegerArguments.Length; j++)
        {
            this.totalArgumentCount += this.IntegerArguments[j].GetArgumentCount();
        }
        this.bufferData = null;
        this.totalBufferSize = 0;
        this.connectionId = 0U;
        this.bufferBaseAddress = 0U;
        this.callData = null;
    }

    // Token: 0x1700002C RID: 44
    // (get) Token: 0x06000145 RID: 325 RVA: 0x00005296 File Offset: 0x00004296
    // (set) Token: 0x06000146 RID: 326 RVA: 0x0000529E File Offset: 0x0000429E
    internal ulong ReturnValue { get; private set; }

    // Token: 0x1700002D RID: 45
    // (get) Token: 0x06000147 RID: 327 RVA: 0x000052A7 File Offset: 0x000042A7
    // (set) Token: 0x06000148 RID: 328 RVA: 0x000052AF File Offset: 0x000042AF
    internal ulong PostMethodCallReturnValue { get; private set; }

    // Token: 0x1700002E RID: 46
    // (get) Token: 0x06000149 RID: 329 RVA: 0x000052B8 File Offset: 0x000042B8
    // (set) Token: 0x0600014A RID: 330 RVA: 0x000052C0 File Offset: 0x000042C0
    internal bool NoDevkit { get; set; }

    // Token: 0x0600014B RID: 331 RVA: 0x000052CC File Offset: 0x000042CC
    public void Invoke()
    {
        this.ValidateCallData();
        this.CreateCallDataBufferLayout();
        this.OpenXBDMConnection();
        bool flag = true;
        try
        {
            this.Options.NotifyConnected(this.connectionId);
            this.SendRPCStartCommand();
            this.Options.NotifyRPCConnectionReady(this.bufferBaseAddress);
            this.CreateArgumentBufferData();
            this.Options.NotifyBufferPrepared(this.totalBufferSize);
            this.SendCallData();
            this.Options.NotifyDataSent();
            this.WaitForCallCompletion(this.Options.ExecutionTimeoutPeriod);
            this.Options.NotifyWaitComplete();
            this.ReceiveCallData();
            this.Options.NotifyDataReceived();
            flag = false;
        }
        finally
        {
            if (!this.NoDevkit)
            {
                if (flag)
                {
                    try
                    {
                        this.Console.CloseConnection(this.connectionId);
                        goto IL_C7;
                    }
                    catch (Exception)
                    {
                        goto IL_C7;
                    }
                }
                this.Console.CloseConnection(this.connectionId);
            }
        IL_C7:
            this.connectionId = 0U;
            this.Options.NotifyConnectionClosed();
        }
        this.UnpackCallData();
        this.Options.NotifyStatusReceived((uint)this.callResult);
        this.ValidateCallResult();
    }

    // Token: 0x0600014C RID: 332 RVA: 0x000053F0 File Offset: 0x000043F0
    private void ValidateCallData()
    {
        if (this.totalArgumentCount > 8)
        {
            throw new XDRPCTooManyArgumentsException(this.totalArgumentCount, 8);
        }
    }

    // Token: 0x0600014D RID: 333 RVA: 0x00005408 File Offset: 0x00004408
    private void CreateCallDataBufferLayout()
    {
        int num = XDRPCExecutionState.ArgumentSize * (8 + this.totalArgumentCount + this.FloatingPointArguments.Length);
        int num2 = 2;
        this.bufferData = new XDRPCExecutionState.ArgumentBufferData[this.IntegerArguments.Length + num2];
        this.referenceData = new XDRPCExecutionState.ArgumentBufferData[this.IntegerArguments.Length];
        int num3 = num;
        for (int i = 0; i < this.IntegerArguments.Length + num2; i++)
        {
            XDRPCExecutionState.ArgumentBufferData argumentBufferData = default(XDRPCExecutionState.ArgumentBufferData);
            argumentBufferData.BufferOffset = num3;
            if (i < num2)
            {
                argumentBufferData.BufferSize = this.Options.GetRequiredBufferSize(i);
            }
            else
            {
                argumentBufferData.BufferSize = this.IntegerArguments[i - num2].GetRequiredBufferSize();
            }
            num3 += argumentBufferData.BufferSize;
            XDRPCExecutionState.AlignBufferOffset(ref num3);
            this.bufferData[i] = argumentBufferData;
        }
        for (int j = 0; j < this.IntegerArguments.Length; j++)
        {
            XDRPCExecutionState.ArgumentBufferData argumentBufferData2 = default(XDRPCExecutionState.ArgumentBufferData);
            argumentBufferData2.BufferOffset = num3;
            argumentBufferData2.BufferSize = this.IntegerArguments[j].GetRequiredReferenceSize();
            num3 += argumentBufferData2.BufferSize;
            XDRPCExecutionState.AlignBufferOffset(ref num3);
            this.referenceData[j] = argumentBufferData2;
        }
        this.totalBufferSize = num3;
    }

    // Token: 0x0600014E RID: 334 RVA: 0x0000553C File Offset: 0x0000453C
    private void OpenXBDMConnection()
    {
        if (this.NoDevkit)
        {
            return;
        }
        int num = 0;
        bool flag = false;
        while (!flag)
        {
            try
            {
                this.connectionId = this.Console.OpenConnection(null);
                flag = true;
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != -2099642112)
                {
                    throw;
                }
                if (num >= 3)
                {
                    throw new XDRPCException("Unable to connect to console. Verify console is not crashed.");
                }
                num++;
                this.Options.NotifyConnectRetry(num);
                Thread.Sleep(XDRPCExecutionState.RetryPeriod);
            }
        }
    }

    // Token: 0x0600014F RID: 335 RVA: 0x000055C0 File Offset: 0x000045C0
    private void SendRPCStartCommand()
    {
        string text = string.Empty;
        string text2 = (this.Options.Mode == XDRPCMode.Title) ? "title" : "system";
        string text3 = string.Format("rpc {0} version={1} buf_size={2} processor={3} thread={4}", new object[]
        {
            text2,
            XDRPCExecutionState.XDRPCVersion.Major,
            this.totalBufferSize,
            this.Options.Processor,
            string.IsNullOrEmpty(this.Options.ThreadName) ? string.Empty : this.Options.ThreadName
        });
        if (this.NoDevkit)
        {
            text = "204- buf_addr=80000000";
        }
        else
        {
            try
            {
                this.Console.SendTextCommand(this.connectionId, text3, out text);
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == -2099642345)
                {
                    throw new XDRPCInvalidResponseException("Invalid argument.");
                }
                if (ex.ErrorCode == -2099642361)
                {
                    throw new XDRPCNotSupportedException("RPC command not registered in Xbox's XBDM.");
                }
                if (ex.ErrorCode == -2099642340)
                {
                    throw new XDRPCInvalidResponseException(string.Format("Requested buffer size ({0}) too large for XBDM.", this.totalBufferSize));
                }
                throw;
            }
        }
        Match match = Regex.Match(text, "(?+i)^(?<code>\\d+)\\-\\s+error=(?<base>.+)");
        if (match.Success && uint.Parse(match.Groups["code"].Value) == 200U)
        {
            throw new XDRPCInvalidResponseException(match.Groups["base"].Value);
        }
        match = Regex.Match(text, "(?+i)^(?<code>\\d+)\\-\\s+buf_addr=(?<base>[a-f\\d]+)");
        if (!match.Success)
        {
            throw new XDRPCInvalidResponseException(text);
        }
        uint num = uint.Parse(match.Groups["code"].Value);
        if (num != 204U)
        {
            throw new XDRPCNotSupportedException(text);
        }
        this.bufferBaseAddress = uint.Parse(match.Groups["base"].Value, NumberStyles.HexNumber);
    }

    // Token: 0x06000150 RID: 336 RVA: 0x000057BC File Offset: 0x000047BC
    private void CreateArgumentBufferData()
    {
        int num = 0;
        int num2 = 2;
        this.callData = new byte[this.totalBufferSize];
        MarshalingUtils.PushCallData(0UL, this.callData, ref num);
        MarshalingUtils.PushCallData(0UL, this.callData, ref num);
        MarshalingUtils.PushCallData((ulong)((long)this.Options.PostMethodCall), this.callData, ref num);
        MarshalingUtils.PushCallData((ulong)this.callFlags, this.callData, ref num);
        MarshalingUtils.PushCallData((ulong)((long)this.totalArgumentCount), this.callData, ref num);
        MarshalingUtils.PushCallData((ulong)((long)this.FloatingPointArguments.Length), this.callData, ref num);
        for (int i = 0; i < this.IntegerArguments.Length + num2; i++)
        {
            ulong bufferAddress = (ulong)this.bufferBaseAddress + (ulong)((long)this.bufferData[i].BufferOffset);
            if (i < num2)
            {
                ulong argumentValue = this.Options.GetArgumentValue(i, bufferAddress);
                MarshalingUtils.PushCallData(argumentValue, this.callData, ref num);
            }
            else
            {
                int num3 = i - num2;
                ulong referenceAddress = (ulong)this.bufferBaseAddress + (ulong)((long)this.referenceData[num3].BufferOffset);
                this.IntegerArguments[num3].SetReferenceAddress(referenceAddress);
                int argumentCount = this.IntegerArguments[num3].GetArgumentCount();
                if (argumentCount == 1)
                {
                    ulong argumentValue2 = this.IntegerArguments[num3].GetArgumentValue(bufferAddress);
                    MarshalingUtils.PushCallData(argumentValue2, this.callData, ref num);
                }
                else
                {
                    ulong[] argumentValues = this.IntegerArguments[num3].GetArgumentValues(bufferAddress);
                    for (int j = 0; j < argumentValues.Length; j++)
                    {
                        MarshalingUtils.PushCallData(argumentValues[j], this.callData, ref num);
                    }
                }
            }
        }
        for (int k = 0; k < this.FloatingPointArguments.Length; k++)
        {
            ulong bufferAddress2 = (ulong)this.bufferBaseAddress + (ulong)((long)this.bufferData[k].BufferOffset);
            ulong argumentValue3 = this.FloatingPointArguments[k].GetArgumentValue(bufferAddress2);
            MarshalingUtils.PushCallData(argumentValue3, this.callData, ref num);
        }
        for (int l = 0; l < this.IntegerArguments.Length + num2; l++)
        {
            if (this.bufferData[l].BufferSize > 0)
            {
                byte[] array;
                if (l < num2)
                {
                    array = this.Options.PackBufferData(l);
                }
                else
                {
                    array = this.IntegerArguments[l - num2].PackBufferData();
                }
                if (array != null)
                {
                    num = this.bufferData[l].BufferOffset;
                    MarshalingUtils.PushBufferData(array, this.callData, ref num);
                }
            }
        }
        for (int m = 0; m < this.IntegerArguments.Length; m++)
        {
            if (this.referenceData[m].BufferSize > 0)
            {
                byte[] array2 = this.IntegerArguments[m].PackReferenceData();
                if (array2 != null)
                {
                    num = this.referenceData[m].BufferOffset;
                    MarshalingUtils.PushBufferData(array2, this.callData, ref num);
                }
            }
        }
    }

    // Token: 0x06000151 RID: 337 RVA: 0x00005A84 File Offset: 0x00004A84
    private void SendCallData()
    {
        string text;
        if (this.NoDevkit)
        {
            text = "203- OK";
        }
        else
        {
            this.Console.SendBinary(this.connectionId, this.callData, (uint)this.callData.Length);
            int num = this.Console.ReceiveStatusResponse(this.connectionId, out text);
            if (num < 0)
            {
                throw new XDRPCInvocationFailedException("Receive Status Response failed {0:X8}", new object[]
                {
                    num
                });
            }
        }
        Match match = Regex.Match(text, "^(?<code>\\d+)\\-\\s+(?<msg>.*)$");
        if (!match.Success)
        {
            throw new XDRPCInvalidResponseException(text);
        }
        uint num2 = uint.Parse(match.Groups["code"].Value);
        if (num2 != 203U)
        {
            throw new XDRPCInvalidResponseException(text);
        }
    }

    // Token: 0x06000152 RID: 338 RVA: 0x00005B3C File Offset: 0x00004B3C
    private void WaitForCallCompletion(TimeSpan timeout)
    {
        if (this.NoDevkit)
        {
            return;
        }
        byte[] array = new byte[XDRPCExecutionState.ArgumentSize];
        DateTime now = DateTime.Now;
        ulong uint64FromBigEndianBytes;
        DateTime now2;
        for (; ; )
        {
            uint num;
            this.Console.ReceiveBinary(this.connectionId, array, (uint)array.Length, out num);
            if ((ulong)num != (ulong)((long)XDRPCExecutionState.ArgumentSize))
            {
                break;
            }
            uint64FromBigEndianBytes = MarshalingUtils.GetUInt64FromBigEndianBytes(array, 0);
            if (uint64FromBigEndianBytes == 997UL)
            {
                now2 = DateTime.Now;
                if (now2.Subtract(now) > timeout)
                {
                    goto Block_4;
                }
            }
            if (uint64FromBigEndianBytes != 997UL)
            {
                goto Block_5;
            }
        }
        throw new XDRPCInvalidResponseException("Unexpected binary chunk size received from console");
    Block_4:
        throw new XDRPCTimeoutException("Timed out while waiting for function to execute, try setting a longer executionTimeoutPeriod in XDRPCExecutionOptions if longer timeout period is needed", now, now2, timeout);
    Block_5:
        if (uint64FromBigEndianBytes != 0UL)
        {
            throw new XDRPCInvalidResponseException(string.Format("Received invalid status from console: {0}", uint64FromBigEndianBytes));
        }
    }

    // Token: 0x06000153 RID: 339 RVA: 0x00005BEC File Offset: 0x00004BEC
    private void ReceiveCallData()
    {
        if (this.NoDevkit)
        {
            return;
        }
        uint num = 0U;
        this.Console.ReceiveBinary(this.connectionId, this.callData, (uint)this.callData.Length, out num);
        if (num != (uint)this.callData.Length)
        {
            throw new XDRPCInvocationFailedException("Buffer returned is of unexpected size");
        }
    }

    // Token: 0x06000154 RID: 340 RVA: 0x00005C3C File Offset: 0x00004C3C
    private void UnpackCallData()
    {
        this.callResult = MarshalingUtils.GetUInt64FromBigEndianBytes(this.callData, 0);
        this.ReturnValue = MarshalingUtils.GetUInt64FromBigEndianBytes(this.callData, MarshalingUtils.SizeOf(typeof(ulong)));
        if (this.Options.PostMethodCall != XDRPCPostMethodCall.None)
        {
            this.PostMethodCallReturnValue = MarshalingUtils.GetUInt64FromBigEndianBytes(this.callData, 2 * MarshalingUtils.SizeOf(typeof(ulong)));
        }
        for (int i = 0; i < this.IntegerArguments.Length; i++)
        {
            int num = i + 2;
            if (this.bufferData[num].BufferSize > 0)
            {
                byte[] array = new byte[this.bufferData[num].BufferSize];
                Array.Copy(this.callData, this.bufferData[num].BufferOffset, array, 0, array.Length);
                this.IntegerArguments[i].UnpackBufferData(array);
            }
        }
        for (int j = 0; j < this.IntegerArguments.Length; j++)
        {
            if (this.referenceData[j].BufferSize > 0)
            {
                byte[] array2 = new byte[this.referenceData[j].BufferSize];
                Array.Copy(this.callData, this.referenceData[j].BufferOffset, array2, 0, array2.Length);
                this.IntegerArguments[j].UnpackReferenceData(array2);
            }
        }
    }

    // Token: 0x06000155 RID: 341 RVA: 0x00005D8C File Offset: 0x00004D8C
    private void ValidateCallResult()
    {
        if ((uint)this.callResult == 2147942526U)
        {
            throw new XDRPCModuleNotFoundException(this.Options.ModuleName);
        }
        if ((uint)this.callResult == 2147942527U)
        {
            if (this.Options.Ordinal != 0)
            {
                throw new XDRPCFunctionNotFoundException(this.Options.ModuleName, this.Options.Ordinal);
            }
            throw new XDRPCFunctionNotFoundException(this.Options.ModuleName, this.Options.FunctionName);
        }
        else
        {
            if ((uint)this.callResult != 0U)
            {
                throw new XDRPCInvocationFailedException("RPC call failed: {0:X8}", new object[]
                {
                    (uint)this.callResult
                });
            }
            return;
        }
    }

    // Token: 0x06000156 RID: 342 RVA: 0x00005E38 File Offset: 0x00004E38
    internal static void AlignBufferOffset(ref int currOffset)
    {
        int num = currOffset % XDRPCExecutionState.ArgumentSize;
        if (num != 0)
        {
            currOffset += XDRPCExecutionState.ArgumentSize - num;
        }
    }

    // Token: 0x06000157 RID: 343 RVA: 0x00005E60 File Offset: 0x00004E60
    private void DumpBuffer(byte[] buffer)
    {
        System.Console.WriteLine("===== Buffer =====");
        System.Console.WriteLine("Length = {0}", buffer.Length);
        byte[] array = new byte[XDRPCExecutionState.ArgumentSize];
        for (int i = 0; i < buffer.Length; i += array.Length)
        {
            Array.Copy(buffer, i, array, 0, Math.Min(array.Length, buffer.Length - i));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0:X} {1:X} | ", i, (long)((ulong)this.bufferBaseAddress + (ulong)((long)i)));
            foreach (byte b in array)
            {
                stringBuilder.AppendFormat("{0:X2} ", b);
            }
            System.Console.WriteLine(stringBuilder.ToString());
        }
    }

    // Token: 0x0400005E RID: 94
    private const ulong ERROR_IO_PENDING_64 = 997UL;

    // Token: 0x0400005F RID: 95
    private const ulong ERROR_SUCCESS_64 = 0UL;

    // Token: 0x04000060 RID: 96
    private const uint XBDM_NOERR = 200U;

    // Token: 0x04000061 RID: 97
    private const uint XBDM_NOMEMORY = 2195324956U;

    // Token: 0x04000062 RID: 98
    private const uint XBDM_INVALIDCMD = 2195324935U;

    // Token: 0x04000063 RID: 99
    private const uint XBDM_INVALIDARG = 2195324951U;

    // Token: 0x04000064 RID: 100
    private const uint XBDM_BINRESPONSE = 203U;

    // Token: 0x04000065 RID: 101
    private const uint XBDM_READYFORBIN = 204U;

    // Token: 0x04000066 RID: 102
    private const uint XBDM_CANNOTCONNECT = 2195325184U;

    // Token: 0x04000067 RID: 103
    private const uint ERROR_MOD_NOT_FOUND = 2147942526U;

    // Token: 0x04000068 RID: 104
    private const uint ERROR_PROC_NOT_FOUND = 2147942527U;

    // Token: 0x04000069 RID: 105
    private const uint ERROR_SUCCESS = 0U;

    // Token: 0x0400006A RID: 106
    private const int MaxConnectRetryCount = 3;

    // Token: 0x0400006B RID: 107
    private const int PostMethodCallReturnValueOffset = 2;

    // Token: 0x0400006C RID: 108
    public const int MaxArgumentsSupported = 8;

    // Token: 0x0400006D RID: 109
    private static readonly TimeSpan RetryPeriod = TimeSpan.FromMilliseconds(100.0);

    // Token: 0x0400006E RID: 110
    private static readonly int ArgumentSize = MarshalingUtils.SizeOf(typeof(ulong));

    // Token: 0x0400006F RID: 111
    public static readonly Version XDRPCVersion = new Version(4, 0);

    // Token: 0x04000070 RID: 112
    private IXboxConsole Console;

    // Token: 0x04000071 RID: 113
    private XDRPCExecutionOptions Options;

    // Token: 0x04000072 RID: 114
    private XDRPCArgumentInfo[] IntegerArguments;

    // Token: 0x04000073 RID: 115
    private XDRPCArgumentInfo[] FloatingPointArguments;

    // Token: 0x04000074 RID: 116
    private XDRPCExecutionState.ArgumentBufferData[] bufferData;

    // Token: 0x04000075 RID: 117
    private XDRPCExecutionState.ArgumentBufferData[] referenceData;

    // Token: 0x04000076 RID: 118
    private int totalBufferSize;

    // Token: 0x04000077 RID: 119
    private uint connectionId;

    // Token: 0x04000078 RID: 120
    private uint bufferBaseAddress;

    // Token: 0x04000079 RID: 121
    private byte[] callData;

    // Token: 0x0400007A RID: 122
    private ulong callResult;

    // Token: 0x0400007B RID: 123
    private XDRPCExecutionState.XDRPCCallFlags callFlags;

    // Token: 0x0400007C RID: 124
    private int totalArgumentCount;

    // Token: 0x02000022 RID: 34
    [Flags]
    internal enum XDRPCCallFlags : ulong
    {
        // Token: 0x04000081 RID: 129
        IntegerReturn = 0UL,
        // Token: 0x04000082 RID: 130
        FloatingPointReturn = 1UL
    }

    // Token: 0x02000023 RID: 35
    private struct ArgumentBufferData
    {
        // Token: 0x04000083 RID: 131
        public int BufferOffset;

        // Token: 0x04000084 RID: 132
        public int BufferSize;
    }
}