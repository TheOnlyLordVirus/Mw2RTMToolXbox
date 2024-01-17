// Token: 0x02000020 RID: 32
using System.Text;
using System;

using XDRPC;

public class XDRPCExecutionOptions
{
    // Token: 0x14000001 RID: 1
    // (add) Token: 0x060000E1 RID: 225 RVA: 0x00004866 File Offset: 0x00003866
    // (remove) Token: 0x060000E2 RID: 226 RVA: 0x0000487F File Offset: 0x0000387F
    public event RPCEventHandler<int> OnConnectRetry;

    // Token: 0x14000002 RID: 2
    // (add) Token: 0x060000E3 RID: 227 RVA: 0x00004898 File Offset: 0x00003898
    // (remove) Token: 0x060000E4 RID: 228 RVA: 0x000048B1 File Offset: 0x000038B1
    public event RPCEventHandler<uint> OnConnected;

    // Token: 0x14000003 RID: 3
    // (add) Token: 0x060000E5 RID: 229 RVA: 0x000048CA File Offset: 0x000038CA
    // (remove) Token: 0x060000E6 RID: 230 RVA: 0x000048E3 File Offset: 0x000038E3
    public event RPCEventHandler<int> OnBufferPrepared;

    // Token: 0x14000004 RID: 4
    // (add) Token: 0x060000E7 RID: 231 RVA: 0x000048FC File Offset: 0x000038FC
    // (remove) Token: 0x060000E8 RID: 232 RVA: 0x00004915 File Offset: 0x00003915
    public event RPCEventHandler<uint> OnRPCConnectionReady;

    // Token: 0x14000005 RID: 5
    // (add) Token: 0x060000E9 RID: 233 RVA: 0x0000492E File Offset: 0x0000392E
    // (remove) Token: 0x060000EA RID: 234 RVA: 0x00004947 File Offset: 0x00003947
    public event RPCEventHandler OnDataSent;

    // Token: 0x14000006 RID: 6
    // (add) Token: 0x060000EB RID: 235 RVA: 0x00004960 File Offset: 0x00003960
    // (remove) Token: 0x060000EC RID: 236 RVA: 0x00004979 File Offset: 0x00003979
    public event RPCEventHandler OnWaitComplete;

    // Token: 0x14000007 RID: 7
    // (add) Token: 0x060000ED RID: 237 RVA: 0x00004992 File Offset: 0x00003992
    // (remove) Token: 0x060000EE RID: 238 RVA: 0x000049AB File Offset: 0x000039AB
    public event RPCEventHandler OnDataReceived;

    // Token: 0x14000008 RID: 8
    // (add) Token: 0x060000EF RID: 239 RVA: 0x000049C4 File Offset: 0x000039C4
    // (remove) Token: 0x060000F0 RID: 240 RVA: 0x000049DD File Offset: 0x000039DD
    public event RPCEventHandler OnConnectionClosed;

    // Token: 0x14000009 RID: 9
    // (add) Token: 0x060000F1 RID: 241 RVA: 0x000049F6 File Offset: 0x000039F6
    // (remove) Token: 0x060000F2 RID: 242 RVA: 0x00004A0F File Offset: 0x00003A0F
    public event RPCEventHandler<uint> OnStatusReceived;

    // Token: 0x060000F3 RID: 243 RVA: 0x00004A28 File Offset: 0x00003A28
    internal ulong GetArgumentValue(int index, ulong bufferAddress)
    {
        switch (index)
        {
            case 0:
                return this.moduleInfo.GetArgumentValue(bufferAddress);
            case 1:
                return this.functionInfo.GetArgumentValue(bufferAddress);
            default:
                return 0UL;
        }
    }

    // Token: 0x060000F4 RID: 244 RVA: 0x00004A64 File Offset: 0x00003A64
    internal int GetRequiredBufferSize(int index)
    {
        switch (index)
        {
            case 0:
                return this.moduleInfo.GetRequiredBufferSize();
            case 1:
                return this.functionInfo.GetRequiredBufferSize();
            default:
                return 0;
        }
    }

    // Token: 0x060000F5 RID: 245 RVA: 0x00004A9C File Offset: 0x00003A9C
    internal byte[] PackBufferData(int index)
    {
        switch (index)
        {
            case 0:
                return this.moduleInfo.PackBufferData();
            case 1:
                return this.functionInfo.PackBufferData();
            default:
                return null;
        }
    }

    // Token: 0x17000023 RID: 35
    // (get) Token: 0x060000F6 RID: 246 RVA: 0x00004AD4 File Offset: 0x00003AD4
    // (set) Token: 0x060000F7 RID: 247 RVA: 0x00004ADC File Offset: 0x00003ADC
    public XDRPCMode Mode { get; set; }

    // Token: 0x17000024 RID: 36
    // (get) Token: 0x060000F8 RID: 248 RVA: 0x00004AE5 File Offset: 0x00003AE5
    // (set) Token: 0x060000F9 RID: 249 RVA: 0x00004AED File Offset: 0x00003AED
    public string ModuleName { get; private set; }

    // Token: 0x17000025 RID: 37
    // (get) Token: 0x060000FA RID: 250 RVA: 0x00004AF6 File Offset: 0x00003AF6
    // (set) Token: 0x060000FB RID: 251 RVA: 0x00004AFE File Offset: 0x00003AFE
    public int Ordinal { get; private set; }

    // Token: 0x17000026 RID: 38
    // (get) Token: 0x060000FC RID: 252 RVA: 0x00004B07 File Offset: 0x00003B07
    // (set) Token: 0x060000FD RID: 253 RVA: 0x00004B0F File Offset: 0x00003B0F
    public string FunctionName { get; private set; }

    // Token: 0x17000027 RID: 39
    // (get) Token: 0x060000FE RID: 254 RVA: 0x00004B18 File Offset: 0x00003B18
    // (set) Token: 0x060000FF RID: 255 RVA: 0x00004B20 File Offset: 0x00003B20
    public uint FunctionAddress { get; private set; }

    // Token: 0x17000028 RID: 40
    // (get) Token: 0x06000100 RID: 256 RVA: 0x00004B29 File Offset: 0x00003B29
    // (set) Token: 0x06000101 RID: 257 RVA: 0x00004B31 File Offset: 0x00003B31
    public uint Processor { get; private set; }

    // Token: 0x17000029 RID: 41
    // (get) Token: 0x06000102 RID: 258 RVA: 0x00004B3A File Offset: 0x00003B3A
    // (set) Token: 0x06000103 RID: 259 RVA: 0x00004B42 File Offset: 0x00003B42
    public string ThreadName { get; private set; }

    // Token: 0x1700002A RID: 42
    // (get) Token: 0x06000104 RID: 260 RVA: 0x00004B4B File Offset: 0x00003B4B
    // (set) Token: 0x06000105 RID: 261 RVA: 0x00004B53 File Offset: 0x00003B53
    public XDRPCPostMethodCall PostMethodCall { get; private set; }

    // Token: 0x1700002B RID: 43
    // (get) Token: 0x06000106 RID: 262 RVA: 0x00004B5C File Offset: 0x00003B5C
    // (set) Token: 0x06000107 RID: 263 RVA: 0x00004B64 File Offset: 0x00003B64
    public TimeSpan ExecutionTimeoutPeriod { get; private set; }

    // Token: 0x06000108 RID: 264 RVA: 0x00004B6D File Offset: 0x00003B6D
    public XDRPCExecutionOptions() : this(XDRPCMode.System, string.Empty, string.Empty, 0, 0U)
    {
    }

    // Token: 0x06000109 RID: 265 RVA: 0x00004B82 File Offset: 0x00003B82
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal) : this(mode, module, string.Empty, ordinal, 0U)
    {
    }

    // Token: 0x0600010A RID: 266 RVA: 0x00004B93 File Offset: 0x00003B93
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, XDRPCPostMethodCall postMethodCall) : this(mode, module, string.Empty, ordinal, 0U, postMethodCall)
    {
    }

    // Token: 0x0600010B RID: 267 RVA: 0x00004BA6 File Offset: 0x00003BA6
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, TimeSpan executionTimeoutPeriod) : this(mode, module, string.Empty, ordinal, 0U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600010C RID: 268 RVA: 0x00004BBA File Offset: 0x00003BBA
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, module, string.Empty, ordinal, 0U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600010D RID: 269 RVA: 0x00004BCF File Offset: 0x00003BCF
    public XDRPCExecutionOptions(string threadName, string module, int ordinal) : this(threadName, module, string.Empty, ordinal, 0U)
    {
    }

    // Token: 0x0600010E RID: 270 RVA: 0x00004BE0 File Offset: 0x00003BE0
    public XDRPCExecutionOptions(string threadName, string module, int ordinal, XDRPCPostMethodCall postMethodCall) : this(threadName, module, string.Empty, ordinal, 0U, postMethodCall)
    {
    }

    // Token: 0x0600010F RID: 271 RVA: 0x00004BF3 File Offset: 0x00003BF3
    public XDRPCExecutionOptions(string threadName, string module, int ordinal, TimeSpan executionTimeoutPeriod) : this(threadName, module, string.Empty, ordinal, 0U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000110 RID: 272 RVA: 0x00004C07 File Offset: 0x00003C07
    public XDRPCExecutionOptions(string threadName, string module, int ordinal, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(threadName, module, string.Empty, ordinal, 0U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000111 RID: 273 RVA: 0x00004C1C File Offset: 0x00003C1C
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, uint processor) : this(mode, module, string.Empty, ordinal, 0U, processor, XDRPCPostMethodCall.None)
    {
    }

    // Token: 0x06000112 RID: 274 RVA: 0x00004C30 File Offset: 0x00003C30
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, uint processor, XDRPCPostMethodCall postMethodCall) : this(mode, module, string.Empty, ordinal, 0U, processor, postMethodCall)
    {
    }

    // Token: 0x06000113 RID: 275 RVA: 0x00004C48 File Offset: 0x00003C48
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, uint processor, TimeSpan executionTimeoutPeriod) : this(mode, module, string.Empty, ordinal, 0U, processor, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000114 RID: 276 RVA: 0x00004C6C File Offset: 0x00003C6C
    public XDRPCExecutionOptions(XDRPCMode mode, string module, int ordinal, uint processor, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, module, string.Empty, ordinal, 0U, processor, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000115 RID: 277 RVA: 0x00004C8E File Offset: 0x00003C8E
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName) : this(mode, module, functionName, 0, 0U)
    {
    }

    // Token: 0x06000116 RID: 278 RVA: 0x00004C9B File Offset: 0x00003C9B
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, XDRPCPostMethodCall postMethodCall) : this(mode, module, functionName, 0, 0U, postMethodCall)
    {
    }

    // Token: 0x06000117 RID: 279 RVA: 0x00004CAA File Offset: 0x00003CAA
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, 0, 0U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000118 RID: 280 RVA: 0x00004CBA File Offset: 0x00003CBA
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, 0, 0U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000119 RID: 281 RVA: 0x00004CCB File Offset: 0x00003CCB
    public XDRPCExecutionOptions(string threadName, string module, string functionName) : this(threadName, module, functionName, 0, 0U)
    {
    }

    // Token: 0x0600011A RID: 282 RVA: 0x00004CD8 File Offset: 0x00003CD8
    public XDRPCExecutionOptions(string threadName, string module, string functionName, XDRPCPostMethodCall postMethodCall) : this(threadName, module, functionName, 0, 0U, postMethodCall)
    {
    }

    // Token: 0x0600011B RID: 283 RVA: 0x00004CE7 File Offset: 0x00003CE7
    public XDRPCExecutionOptions(string threadName, string module, string functionName, TimeSpan executionTimeoutPeriod) : this(threadName, module, functionName, 0, 0U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600011C RID: 284 RVA: 0x00004CF7 File Offset: 0x00003CF7
    public XDRPCExecutionOptions(string threadName, string module, string functionName, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(threadName, module, functionName, 0, 0U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600011D RID: 285 RVA: 0x00004D08 File Offset: 0x00003D08
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, uint processor) : this(mode, module, functionName, 0, 0U, processor, XDRPCPostMethodCall.None)
    {
    }

    // Token: 0x0600011E RID: 286 RVA: 0x00004D18 File Offset: 0x00003D18
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, uint processor, XDRPCPostMethodCall postMethodCall) : this(mode, module, functionName, 0, 0U, processor, postMethodCall)
    {
    }

    // Token: 0x0600011F RID: 287 RVA: 0x00004D2C File Offset: 0x00003D2C
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, uint processor, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, 0, 0U, processor, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000120 RID: 288 RVA: 0x00004D4C File Offset: 0x00003D4C
    public XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, uint processor, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, 0, 0U, processor, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000121 RID: 289 RVA: 0x00004D6A File Offset: 0x00003D6A
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress) : this(mode, string.Empty, string.Empty, 0, functionAddress)
    {
    }

    // Token: 0x06000122 RID: 290 RVA: 0x00004D7F File Offset: 0x00003D7F
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, XDRPCPostMethodCall postMethodCall) : this(mode, string.Empty, string.Empty, 0, functionAddress, postMethodCall)
    {
    }

    // Token: 0x06000123 RID: 291 RVA: 0x00004D95 File Offset: 0x00003D95
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, string.Empty, 0, functionAddress, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000124 RID: 292 RVA: 0x00004DAC File Offset: 0x00003DAC
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, string.Empty, 0, functionAddress, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000125 RID: 293 RVA: 0x00004DC4 File Offset: 0x00003DC4
    public XDRPCExecutionOptions(string threadName, uint functionAddress) : this(threadName, string.Empty, string.Empty, 0, functionAddress)
    {
    }

    // Token: 0x06000126 RID: 294 RVA: 0x00004DD9 File Offset: 0x00003DD9
    public XDRPCExecutionOptions(string threadName, uint functionAddress, XDRPCPostMethodCall postMethodCall) : this(threadName, string.Empty, string.Empty, 0, functionAddress, postMethodCall)
    {
    }

    // Token: 0x06000127 RID: 295 RVA: 0x00004DEF File Offset: 0x00003DEF
    public XDRPCExecutionOptions(string threadName, uint functionAddress, TimeSpan executionTimeoutPeriod) : this(threadName, string.Empty, string.Empty, 0, functionAddress, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000128 RID: 296 RVA: 0x00004E06 File Offset: 0x00003E06
    public XDRPCExecutionOptions(string threadName, uint functionAddress, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(threadName, string.Empty, string.Empty, 0, functionAddress, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000129 RID: 297 RVA: 0x00004E1E File Offset: 0x00003E1E
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, uint processor) : this(mode, string.Empty, string.Empty, 0, functionAddress, processor, XDRPCPostMethodCall.None)
    {
    }

    // Token: 0x0600012A RID: 298 RVA: 0x00004E35 File Offset: 0x00003E35
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, uint processor, XDRPCPostMethodCall postMethodCall) : this(mode, string.Empty, string.Empty, 0, functionAddress, processor, postMethodCall)
    {
    }

    // Token: 0x0600012B RID: 299 RVA: 0x00004E50 File Offset: 0x00003E50
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, uint processor, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, string.Empty, 0, functionAddress, processor, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600012C RID: 300 RVA: 0x00004E74 File Offset: 0x00003E74
    public XDRPCExecutionOptions(XDRPCMode mode, uint functionAddress, uint processor, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, string.Empty, 0, functionAddress, processor, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x0600012D RID: 301 RVA: 0x00004E9C File Offset: 0x00003E9C
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress) : this(mode, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, XDRPCExecutionOptions.DefaultTimeoutPeriod)
    {
    }

    // Token: 0x0600012E RID: 302 RVA: 0x00004EBD File Offset: 0x00003EBD
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, XDRPCPostMethodCall postMethodCall) : this(mode, module, functionName, ordinal, functionAddress, 5U, postMethodCall)
    {
    }

    // Token: 0x0600012F RID: 303 RVA: 0x00004ED0 File Offset: 0x00003ED0
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000130 RID: 304 RVA: 0x00004EF0 File Offset: 0x00003EF0
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, module, functionName, ordinal, functionAddress, 5U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000131 RID: 305 RVA: 0x00004F10 File Offset: 0x00003F10
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, uint processor) : this(mode, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, XDRPCExecutionOptions.DefaultTimeoutPeriod)
    {
    }

    // Token: 0x06000132 RID: 306 RVA: 0x00004F34 File Offset: 0x00003F34
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, uint processor, XDRPCPostMethodCall postMethodCall) : this(mode, string.Empty, module, functionName, ordinal, functionAddress, 5U, postMethodCall, XDRPCExecutionOptions.DefaultTimeoutPeriod)
    {
    }

    // Token: 0x06000133 RID: 307 RVA: 0x00004F5C File Offset: 0x00003F5C
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, uint processor, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000134 RID: 308 RVA: 0x00004F80 File Offset: 0x00003F80
    private XDRPCExecutionOptions(XDRPCMode mode, string module, string functionName, int ordinal, uint functionAddress, uint processor, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(mode, string.Empty, module, functionName, ordinal, functionAddress, 5U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000135 RID: 309 RVA: 0x00004FA4 File Offset: 0x00003FA4
    private XDRPCExecutionOptions(string threadName, string module, string functionName, int ordinal, uint functionAddress) : this(XDRPCMode.System, threadName, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, XDRPCExecutionOptions.DefaultTimeoutPeriod)
    {
    }

    // Token: 0x06000136 RID: 310 RVA: 0x00004FC8 File Offset: 0x00003FC8
    private XDRPCExecutionOptions(string threadName, string module, string functionName, int ordinal, uint functionAddress, XDRPCPostMethodCall postMethodCall) : this(XDRPCMode.System, threadName, module, functionName, ordinal, functionAddress, 5U, postMethodCall, XDRPCExecutionOptions.DefaultTimeoutPeriod)
    {
    }

    // Token: 0x06000137 RID: 311 RVA: 0x00004FEC File Offset: 0x00003FEC
    private XDRPCExecutionOptions(string threadName, string module, string functionName, int ordinal, uint functionAddress, TimeSpan executionTimeoutPeriod) : this(XDRPCMode.System, threadName, module, functionName, ordinal, functionAddress, 5U, XDRPCPostMethodCall.None, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000138 RID: 312 RVA: 0x0000500C File Offset: 0x0000400C
    private XDRPCExecutionOptions(string threadName, string module, string functionName, int ordinal, uint functionAddress, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod) : this(XDRPCMode.System, threadName, module, functionName, ordinal, functionAddress, 5U, postMethodCall, executionTimeoutPeriod)
    {
    }

    // Token: 0x06000139 RID: 313 RVA: 0x0000502C File Offset: 0x0000402C
    private XDRPCExecutionOptions(XDRPCMode mode, string threadName, string module, string functionName, int ordinal, uint functionAddress, uint processor, XDRPCPostMethodCall postMethodCall, TimeSpan executionTimeoutPeriod)
    {
        this.Mode = mode;
        if (!string.IsNullOrEmpty(module))
        {
            this.moduleInfo = new XDRPCStringArgumentInfo(module, Encoding.ASCII, ArgumentType.ByRef);
            if (string.IsNullOrEmpty(functionName))
            {
                this.functionInfo = new XDRPCArgumentInfo<int>(ordinal);
            }
            else
            {
                this.functionInfo = new XDRPCStringArgumentInfo(functionName, Encoding.ASCII, ArgumentType.ByRef);
            }
        }
        else
        {
            this.moduleInfo = new XDRPCNullArgumentInfo();
            this.functionInfo = new XDRPCArgumentInfo<uint>(functionAddress);
        }
        this.FunctionName = functionName;
        this.ModuleName = module;
        this.Ordinal = ordinal;
        this.FunctionAddress = functionAddress;
        this.Processor = processor;
        this.PostMethodCall = postMethodCall;
        this.ThreadName = threadName;
        this.ExecutionTimeoutPeriod = executionTimeoutPeriod;
    }

    // Token: 0x0600013A RID: 314 RVA: 0x000050E3 File Offset: 0x000040E3
    internal void NotifyConnectRetry(int retry)
    {
        if (this.OnConnectRetry != null)
        {
            this.OnConnectRetry(this, retry);
        }
    }

    // Token: 0x0600013B RID: 315 RVA: 0x000050FA File Offset: 0x000040FA
    internal void NotifyConnected(uint id)
    {
        if (this.OnConnected != null)
        {
            this.OnConnected(this, id);
        }
    }

    // Token: 0x0600013C RID: 316 RVA: 0x00005111 File Offset: 0x00004111
    internal void NotifyBufferPrepared(int size)
    {
        if (this.OnBufferPrepared != null)
        {
            this.OnBufferPrepared(this, size);
        }
    }

    // Token: 0x0600013D RID: 317 RVA: 0x00005128 File Offset: 0x00004128
    internal void NotifyRPCConnectionReady(uint baseAddr)
    {
        if (this.OnRPCConnectionReady != null)
        {
            this.OnRPCConnectionReady(this, baseAddr);
        }
    }

    // Token: 0x0600013E RID: 318 RVA: 0x0000513F File Offset: 0x0000413F
    internal void NotifyDataSent()
    {
        if (this.OnDataSent != null)
        {
            this.OnDataSent(this);
        }
    }

    // Token: 0x0600013F RID: 319 RVA: 0x00005155 File Offset: 0x00004155
    internal void NotifyWaitComplete()
    {
        if (this.OnWaitComplete != null)
        {
            this.OnWaitComplete(this);
        }
    }

    // Token: 0x06000140 RID: 320 RVA: 0x0000516B File Offset: 0x0000416B
    internal void NotifyDataReceived()
    {
        if (this.OnDataReceived != null)
        {
            this.OnDataReceived(this);
        }
    }

    // Token: 0x06000141 RID: 321 RVA: 0x00005181 File Offset: 0x00004181
    internal void NotifyConnectionClosed()
    {
        if (this.OnConnectionClosed != null)
        {
            this.OnConnectionClosed(this);
        }
    }

    // Token: 0x06000142 RID: 322 RVA: 0x00005197 File Offset: 0x00004197
    internal void NotifyStatusReceived(uint status)
    {
        if (this.OnStatusReceived != null)
        {
            this.OnStatusReceived(this, status);
        }
    }

    // Token: 0x04000046 RID: 70
    private const uint DefaultProcessor = 5U;

    // Token: 0x04000047 RID: 71
    private const XDRPCPostMethodCall DefaultPostMethodCall = XDRPCPostMethodCall.None;

    // Token: 0x04000048 RID: 72
    internal const int BufferReserveAmount = 2;

    // Token: 0x04000052 RID: 82
    private XDRPCArgumentInfo moduleInfo;

    // Token: 0x04000053 RID: 83
    private XDRPCArgumentInfo functionInfo;

    // Token: 0x04000054 RID: 84
    private static readonly TimeSpan DefaultTimeoutPeriod = TimeSpan.FromMinutes(5.0);
}
