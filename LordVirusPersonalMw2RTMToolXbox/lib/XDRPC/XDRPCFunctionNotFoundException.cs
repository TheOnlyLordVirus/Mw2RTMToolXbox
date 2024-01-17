using System;

namespace XDRPC;

// Token: 0x0200001A RID: 26
public class XDRPCFunctionNotFoundException : XDRPCException
{
    // Token: 0x060000CA RID: 202 RVA: 0x0000475C File Offset: 0x0000375C
    public XDRPCFunctionNotFoundException(string moduleName, int ordinal) : base(string.Format("Function ordinal not found: {0}@{1}", moduleName, ordinal))
    {
        this.ModuleName = moduleName;
        this.Ordinal = ordinal;
    }

    // Token: 0x060000CB RID: 203 RVA: 0x00004783 File Offset: 0x00003783
    public XDRPCFunctionNotFoundException(string moduleName, string functionName) : base(string.Format("Function name not found: {0}@{1}", moduleName, functionName))
    {
        this.ModuleName = moduleName;
        this.FunctionName = functionName;
    }

    // Token: 0x1700001D RID: 29
    // (get) Token: 0x060000CC RID: 204 RVA: 0x000047A5 File Offset: 0x000037A5
    // (set) Token: 0x060000CD RID: 205 RVA: 0x000047AD File Offset: 0x000037AD
    public string ModuleName { get; private set; }

    // Token: 0x1700001E RID: 30
    // (get) Token: 0x060000CE RID: 206 RVA: 0x000047B6 File Offset: 0x000037B6
    // (set) Token: 0x060000CF RID: 207 RVA: 0x000047BE File Offset: 0x000037BE
    public int Ordinal { get; private set; }

    // Token: 0x1700001F RID: 31
    // (get) Token: 0x060000D0 RID: 208 RVA: 0x000047C7 File Offset: 0x000037C7
    // (set) Token: 0x060000D1 RID: 209 RVA: 0x000047CF File Offset: 0x000037CF
    public string FunctionName { get; private set; }
}
