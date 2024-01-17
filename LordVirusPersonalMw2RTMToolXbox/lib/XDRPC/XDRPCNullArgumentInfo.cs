using System;

namespace XDRPC
{
    // Token: 0x02000008 RID: 8
    public class XDRPCNullArgumentInfo : XDRPCArgumentInfo
    {
        // Token: 0x0600001D RID: 29 RVA: 0x000024BD File Offset: 0x000014BD
        public ulong GetArgumentValue(ulong bufferAddress)
        {
            return 0UL;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x000024C1 File Offset: 0x000014C1
        public int GetRequiredBufferSize()
        {
            return 0;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x000024C4 File Offset: 0x000014C4
        public byte[] PackBufferData()
        {
            return null;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000024C7 File Offset: 0x000014C7
        public void UnpackBufferData(byte[] data)
        {
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000024C9 File Offset: 0x000014C9
        public override string ToString()
        {
            return "[XDRPC]NULL";
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000024D0 File Offset: 0x000014D0
        public bool IsFloatingPointValue()
        {
            return false;
        }

        // Token: 0x06000023 RID: 35 RVA: 0x000024D3 File Offset: 0x000014D3
        public int GetArgumentCount()
        {
            return 1;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000024D8 File Offset: 0x000014D8
        public ulong[] GetArgumentValues(ulong bufferAddress)
        {
            return new ulong[]
            {
                this.GetArgumentValue(bufferAddress)
            };
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000025 RID: 37 RVA: 0x000024F7 File Offset: 0x000014F7
        public ArgumentType PassBy
        {
            get
            {
                return ArgumentType.ByValue;
            }
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000024FA File Offset: 0x000014FA
        public void SetReferenceAddress(ulong bufferAddress)
        {
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000024FC File Offset: 0x000014FC
        public int GetRequiredReferenceSize()
        {
            return 0;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000024FF File Offset: 0x000014FF
        public byte[] PackReferenceData()
        {
            return null;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00002502 File Offset: 0x00001502
        public void UnpackReferenceData(byte[] data)
        {
        }
    }
}
