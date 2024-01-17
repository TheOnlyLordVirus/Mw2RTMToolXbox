using System;

namespace XDRPC;

// Token: 0x0200001B RID: 27
public class XDRPCTimeoutException : XDRPCException
{
    // Token: 0x060000D2 RID: 210 RVA: 0x000047D8 File Offset: 0x000037D8
    public XDRPCTimeoutException(string message, DateTime timeoutStartTime, DateTime timeoutCurrentTime, TimeSpan timeoutPeriod) : base(string.Format("{0}, start time: {1:MM/dd/yyy hh:mm:ss.fff}, operation current time: {2:MM/dd/yyy hh:mm:ss.fff}, timeout period {3}", new object[]
    {
        message,
        timeoutStartTime,
        timeoutCurrentTime,
        timeoutPeriod
    }))
    {
        this.TimeoutStartTime = timeoutStartTime;
        this.TimeoutCurrentTime = timeoutCurrentTime;
        this.TimeoutPeriod = timeoutPeriod;
    }

    // Token: 0x17000020 RID: 32
    // (get) Token: 0x060000D3 RID: 211 RVA: 0x00004833 File Offset: 0x00003833
    // (set) Token: 0x060000D4 RID: 212 RVA: 0x0000483B File Offset: 0x0000383B
    public DateTime TimeoutStartTime { get; private set; }

    // Token: 0x17000021 RID: 33
    // (get) Token: 0x060000D5 RID: 213 RVA: 0x00004844 File Offset: 0x00003844
    // (set) Token: 0x060000D6 RID: 214 RVA: 0x0000484C File Offset: 0x0000384C
    public DateTime TimeoutCurrentTime { get; private set; }

    // Token: 0x17000022 RID: 34
    // (get) Token: 0x060000D7 RID: 215 RVA: 0x00004855 File Offset: 0x00003855
    // (set) Token: 0x060000D8 RID: 216 RVA: 0x0000485D File Offset: 0x0000385D
    public TimeSpan TimeoutPeriod { get; private set; }
}