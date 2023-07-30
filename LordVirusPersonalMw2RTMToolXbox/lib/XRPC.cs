namespace XRPCLib
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class XRPC
    {
        public bool activeConnection;
        public bool activeTransfer;
        private uint[] buffcheck = new uint[15];
        private uint bufferAddress;
        private uint bufferAddressRead = 0x91c0adca;
        private uint bytePointer;
        private string currentVersion = "1.7";
        private static int firstRan;
        private uint floatPointer;
        public static uint g;
        private static uint meh;
        private int multiple;
        private byte[] nulled = new byte[100];
        private int sa;
        private uint stringPointer;
        public XDevkit.IXboxConsole xbCon;
        public uint xbConnection;
        public XDevkit.IXboxManager xboxMgr;

        public uint Call(uint address, params object[] arg)
        {
            long[] argument = new long[9];
            if (!this.activeConnection)
            {
                this.Connect();
            }
            if (firstRan == 0)
            {
                byte[] buffer = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer);
                this.bufferAddress = BitConverter.ToUInt32(buffer, 0);
                firstRan = 1;
                this.stringPointer = this.bufferAddress + 0x5dc;
                this.floatPointer = this.bufferAddress + 0xa8c;
                this.bytePointer = this.bufferAddress + 0xc80;
                this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 100, this.nulled, out meh);
                this.xbCon.DebugTarget.SetMemory(this.stringPointer, 100, this.nulled, out meh);
            }
            if (this.bufferAddress == 0)
            {
                byte[] buffer2 = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer2, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer2);
                this.bufferAddress = BitConverter.ToUInt32(buffer2, 0);
            }
            this.stringPointer = this.bufferAddress + 0x5dc;
            this.floatPointer = this.bufferAddress + 0xa8c;
            this.bytePointer = this.bufferAddress + 0xc80;
            int num = 0;
            int index = 0;
            foreach (object obj2 in arg)
            {
                if (obj2 is byte)
                {
                    byte[] buffer3 = (byte[]) obj2;
                    argument[index] = BitConverter.ToUInt32(buffer3, 0);
                }
                else if (obj2 is byte[])
                {
                    byte[] buffer4 = (byte[]) obj2;
                    this.xbCon.DebugTarget.SetMemory(this.bytePointer, (uint) buffer4.Length, buffer4, out meh);
                    argument[index] = this.bytePointer;
                    this.bytePointer += (uint) (buffer4.Length + 2);
                }
                else if (obj2 is float)
                {
                    byte[] buffer5 = BitConverter.GetBytes(float.Parse(Convert.ToString(obj2)));
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) buffer5.Length, buffer5, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += (uint) (buffer5.Length + 2);
                }
                else if (obj2 is float[])
                {
                    byte[] dst = new byte[12];
                    int num3 = 0;
                    for (num3 = 0; num3 <= 2; num3++)
                    {
                        byte[] buffer7 = new byte[4];
                        Buffer.BlockCopy((Array) obj2, num3 * 4, buffer7, 0, 4);
                        Array.Reverse(buffer7);
                        Buffer.BlockCopy(buffer7, 0, dst, 4 * num3, 4);
                    }
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) dst.Length, dst, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += 2;
                }
                else if (obj2 is string)
                {
                    byte[] buffer8 = Encoding.ASCII.GetBytes(Convert.ToString(obj2));
                    this.xbCon.DebugTarget.SetMemory(this.stringPointer, (uint) buffer8.Length, buffer8, out meh);
                    argument[index] = this.stringPointer;
                    string str = Convert.ToString(obj2);
                    this.stringPointer += (uint) (str.Length + 1);
                }
                else
                {
                    argument[index] = Convert.ToInt64(obj2);
                }
                num++;
                index++;
            }
            byte[] data = getData(argument);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 8, (uint) data.Length, data, out meh);
            byte[] bytes = BitConverter.GetBytes(num);
            Array.Reverse(bytes);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 4, 4, bytes, out meh);
            Thread.Sleep(0);
            byte[] array = BitConverter.GetBytes(address);
            Array.Reverse(array);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 4, array, out meh);
            Thread.Sleep(50);
            byte[] buffer12 = new byte[4];
            this.xbCon.DebugTarget.GetMemory(this.bufferAddress + 0xffc, 4, buffer12, out meh);
            this.xbCon.DebugTarget.InvalidateMemoryCache(true, this.bufferAddress + 0xffc, 4);
            Array.Reverse(buffer12);
            return BitConverter.ToUInt32(buffer12, 0);
        }

        public uint CallSysFunction(uint address, params object[] arg)
        {
            long[] argument = new long[9];
            if (!this.activeConnection)
            {
                this.Connect();
            }
            if (firstRan == 0)
            {
                byte[] buffer = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer);
                this.bufferAddress = BitConverter.ToUInt32(buffer, 0);
                firstRan = 1;
                this.stringPointer = this.bufferAddress + 0x5dc;
                this.floatPointer = this.bufferAddress + 0xa8c;
                this.bytePointer = this.bufferAddress + 0xc80;
                this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 100, this.nulled, out meh);
                this.xbCon.DebugTarget.SetMemory(this.stringPointer, 100, this.nulled, out meh);
            }
            if (this.bufferAddress == 0)
            {
                byte[] buffer2 = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer2, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer2);
                this.bufferAddress = BitConverter.ToUInt32(buffer2, 0);
            }
            this.stringPointer = this.bufferAddress + 0x5dc;
            this.floatPointer = this.bufferAddress + 0xa8c;
            this.bytePointer = this.bufferAddress + 0xc80;
            int num = 0;
            int index = 0;
            argument[index] = address;
            index++;
            foreach (object obj2 in arg)
            {
                if (obj2 is byte)
                {
                    byte[] buffer3 = (byte[]) obj2;
                    argument[index] = BitConverter.ToUInt32(buffer3, 0);
                }
                else if (obj2 is byte[])
                {
                    byte[] buffer4 = (byte[]) obj2;
                    this.xbCon.DebugTarget.SetMemory(this.bytePointer, (uint) buffer4.Length, buffer4, out meh);
                    argument[index] = this.bytePointer;
                    this.bytePointer += (uint) (buffer4.Length + 2);
                }
                else if (obj2 is float)
                {
                    byte[] buffer5 = BitConverter.GetBytes(float.Parse(Convert.ToString(obj2)));
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) buffer5.Length, buffer5, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += (uint) (buffer5.Length + 2);
                }
                else if (obj2 is float[])
                {
                    byte[] dst = new byte[12];
                    int num3 = 0;
                    for (num3 = 0; num3 <= 2; num3++)
                    {
                        byte[] buffer7 = new byte[4];
                        Buffer.BlockCopy((Array) obj2, num3 * 4, buffer7, 0, 4);
                        Array.Reverse(buffer7);
                        Buffer.BlockCopy(buffer7, 0, dst, 4 * num3, 4);
                    }
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) dst.Length, dst, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += 2;
                }
                else if (obj2 is string)
                {
                    byte[] buffer8 = Encoding.ASCII.GetBytes(Convert.ToString(obj2));
                    this.xbCon.DebugTarget.SetMemory(this.stringPointer, (uint) buffer8.Length, buffer8, out meh);
                    argument[index] = this.stringPointer;
                    string str = Convert.ToString(obj2);
                    this.stringPointer += (uint) (str.Length + 1);
                }
                else
                {
                    argument[index] = Convert.ToInt64(obj2);
                }
                num++;
                index++;
            }
            byte[] data = getData(argument);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 8, (uint) data.Length, data, out meh);
            byte[] bytes = BitConverter.GetBytes(num);
            Array.Reverse(bytes);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 4, 4, bytes, out meh);
            Thread.Sleep(0);
            byte[] array = BitConverter.GetBytes((uint) 0x82000000);
            Array.Reverse(array);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 4, array, out meh);
            Thread.Sleep(50);
            byte[] buffer12 = new byte[4];
            this.xbCon.DebugTarget.GetMemory(this.bufferAddress + 0xffc, 4, buffer12, out meh);
            this.xbCon.DebugTarget.InvalidateMemoryCache(true, this.bufferAddress + 0xffc, 4);
            Array.Reverse(buffer12);
            return BitConverter.ToUInt32(buffer12, 0);
        }

        public void Connect()
        {
            this.initialize();
            if (this.activeConnection && (this.sa == 0))
            {
                this.sa = 1;
            }
        }

        private static byte[] getData(long[] argument)
        {
            byte[] array = new byte[argument.Length * 8];
            int index = 0;
            foreach (long num2 in argument)
            {
                byte[] bytes = BitConverter.GetBytes(num2);
                Array.Reverse(bytes);
                bytes.CopyTo(array, index);
                index += 8;
            }
            return array;
        }

        public byte[] GetMemory(uint address, uint length)
        {
            byte[] data = new byte[length];
            this.xbCon.DebugTarget.GetMemory(address, length, data, out g);
            this.xbCon.DebugTarget.InvalidateMemoryCache(true, address, length);
            return data;
        }

        public void initialize()
        {
            string str;
            string str2;
            if (!this.activeConnection)
            {
                this.xboxMgr = (XDevkit.XboxManager) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("A5EB45D8-F3B6-49B9-984A-0D313AB60342")));
                this.xbCon = this.xboxMgr.OpenConsole(this.xboxMgr.DefaultConsole);
                try
                {
                    this.xbConnection = this.xbCon.OpenConnection(null);
                }
                catch (Exception)
                {
                    return;
                }
                if (this.xbCon.DebugTarget.IsDebuggerConnected(out str, out str2))
                {
                    this.activeConnection = true;
                }
                else
                {
                    this.xbCon.DebugTarget.ConnectAsDebugger("XRPC", XDevkit.XboxDebugConnectFlags.Force);
                    if (this.xbCon.DebugTarget.IsDebuggerConnected(out str, out str2))
                    {
                        this.activeConnection = true;
                    }
                }
            }
            else if (!this.xbCon.DebugTarget.IsDebuggerConnected(out str, out str2))
            {
                this.activeConnection = false;
                this.Connect();
            }
        }

        public void Notify(XNotiyLogo type, string text)
        {
            byte[] buffer = this.WideChar(text);
            this.SystemCall(new object[] { this.ResolveFunction("xam.xex", 0x290), Convert.ToUInt32(type), 0, 2, buffer, 0 });
        }

        public uint ResolveFunction(string titleID, uint ord)
        {
            if (firstRan == 0)
            {
                byte[] buffer = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer);
                this.bufferAddress = BitConverter.ToUInt32(buffer, 0);
                firstRan = 1;
                this.stringPointer = this.bufferAddress + 0x5dc;
                this.floatPointer = this.bufferAddress + 0xa8c;
                this.bytePointer = this.bufferAddress + 0xc80;
                this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 100, this.nulled, out meh);
                this.xbCon.DebugTarget.SetMemory(this.stringPointer, 100, this.nulled, out meh);
            }
            byte[] bytes = Encoding.ASCII.GetBytes(titleID);
            this.xbCon.DebugTarget.SetMemory(this.stringPointer, (uint) bytes.Length, bytes, out meh);
            long[] argument = new long[2];
            argument[0] = this.stringPointer;
            string str = Convert.ToString(titleID);
            this.stringPointer += (uint) (str.Length + 1);
            argument[1] = ord;
            byte[] data = getData(argument);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 8, (uint) data.Length, data, out meh);
            byte[] array = BitConverter.GetBytes((uint) 0x82000001);
            Array.Reverse(array);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 4, array, out meh);
            Thread.Sleep(50);
            byte[] buffer5 = new byte[4];
            this.xbCon.DebugTarget.GetMemory(this.bufferAddress + 0xffc, 4, buffer5, out meh);
            this.xbCon.DebugTarget.InvalidateMemoryCache(true, this.bufferAddress + 0xffc, 4);
            Array.Reverse(buffer5);
            return BitConverter.ToUInt32(buffer5, 0);
        }

        public void SetMemory(uint address, byte[] data)
        {
            this.xbCon.DebugTarget.SetMemory(address, (uint) data.Length, data, out g);
        }

        public uint SystemCall(params object[] arg)
        {
            long[] argument = new long[9];
            if (!this.activeConnection)
            {
                this.Connect();
            }
            if (firstRan == 0)
            {
                byte[] buffer = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer);
                this.bufferAddress = BitConverter.ToUInt32(buffer, 0);
                firstRan = 1;
                this.stringPointer = this.bufferAddress + 0x5dc;
                this.floatPointer = this.bufferAddress + 0xa8c;
                this.bytePointer = this.bufferAddress + 0xc80;
                this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 100, this.nulled, out meh);
                this.xbCon.DebugTarget.SetMemory(this.stringPointer, 100, this.nulled, out meh);
            }
            if (this.bufferAddress == 0)
            {
                byte[] buffer2 = new byte[4];
                this.xbCon.DebugTarget.GetMemory(0x91c088ae, 4, buffer2, out meh);
                this.xbCon.DebugTarget.InvalidateMemoryCache(true, 0x91c088ae, 4);
                Array.Reverse(buffer2);
                this.bufferAddress = BitConverter.ToUInt32(buffer2, 0);
            }
            this.stringPointer = this.bufferAddress + 0x5dc;
            this.floatPointer = this.bufferAddress + 0xa8c;
            this.bytePointer = this.bufferAddress + 0xc80;
            int num = 0;
            int index = 0;
            foreach (object obj2 in arg)
            {
                if (obj2 is byte)
                {
                    byte[] buffer3 = (byte[]) obj2;
                    argument[index] = BitConverter.ToUInt32(buffer3, 0);
                }
                else if (obj2 is byte[])
                {
                    byte[] buffer4 = (byte[]) obj2;
                    this.xbCon.DebugTarget.SetMemory(this.bytePointer, (uint) buffer4.Length, buffer4, out meh);
                    argument[index] = this.bytePointer;
                    this.bytePointer += (uint) (buffer4.Length + 2);
                }
                else if (obj2 is float)
                {
                    byte[] buffer5 = BitConverter.GetBytes(float.Parse(Convert.ToString(obj2)));
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) buffer5.Length, buffer5, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += (uint) (buffer5.Length + 2);
                }
                else if (obj2 is float[])
                {
                    byte[] dst = new byte[12];
                    int num3 = 0;
                    for (num3 = 0; num3 <= 2; num3++)
                    {
                        byte[] buffer7 = new byte[4];
                        Buffer.BlockCopy((Array) obj2, num3 * 4, buffer7, 0, 4);
                        Array.Reverse(buffer7);
                        Buffer.BlockCopy(buffer7, 0, dst, 4 * num3, 4);
                    }
                    this.xbCon.DebugTarget.SetMemory(this.floatPointer, (uint) dst.Length, dst, out meh);
                    argument[index] = this.floatPointer;
                    this.floatPointer += 2;
                }
                else if (obj2 is string)
                {
                    byte[] buffer8 = Encoding.ASCII.GetBytes(Convert.ToString(obj2));
                    this.xbCon.DebugTarget.SetMemory(this.stringPointer, (uint) buffer8.Length, buffer8, out meh);
                    argument[index] = this.stringPointer;
                    string str = Convert.ToString(obj2);
                    this.stringPointer += (uint) (str.Length + 1);
                }
                else
                {
                    argument[index] = Convert.ToInt64(obj2);
                }
                num++;
                index++;
            }
            byte[] data = getData(argument);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 8, (uint) data.Length, data, out meh);
            byte[] bytes = BitConverter.GetBytes(num);
            Array.Reverse(bytes);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress + 4, 4, bytes, out meh);
            Thread.Sleep(0);
            byte[] array = BitConverter.GetBytes((uint) 0x82000000);
            Array.Reverse(array);
            this.xbCon.DebugTarget.SetMemory(this.bufferAddress, 4, array, out meh);
            Thread.Sleep(50);
            byte[] buffer12 = new byte[4];
            this.xbCon.DebugTarget.GetMemory(this.bufferAddress + 0xffc, 4, buffer12, out meh);
            this.xbCon.DebugTarget.InvalidateMemoryCache(true, this.bufferAddress + 0xffc, 4);
            Array.Reverse(buffer12);
            return BitConverter.ToUInt32(buffer12, 0);
        }

        private float[] toFloatArray(double[] arr)
        {
            if (arr == null)
            {
                return null;
            }
            int length = arr.Length;
            float[] numArray = new float[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = (float) arr[i];
            }
            return numArray;
        }

        public byte[] WideChar(string text)
        {
            byte[] buffer = new byte[(text.Length * 2) + 2];
            int index = 1;
            buffer[0] = 0;
            foreach (char ch in text)
            {
                buffer[index] = Convert.ToByte(ch);
                index += 2;
            }
            return buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct wChar
        {
            public string Text;
        }

        public enum XNotiyLogo
        {
            ACHIEVEMENT_UNLOCKED = 0x1b,
            ACHIEVEMENTS_UNLOCKED = 0x27,
            AVATAR_AWARD_UNLOCKED = 60,
            BLANK = 0x2a,
            CANT_CONNECT_XBL_PARTY = 0x38,
            CANT_DOWNLOAD_X = 0x20,
            CANT_SIGN_IN_MESSENGER = 0x2b,
            DEVICE_FULL = 0x24,
            DISCONNECTED_FROM_XBOX_LIVE = 11,
            DISCONNECTED_XBOX_LIVE_11_MINUTES_REMAINING = 0x2e,
            DISCONNECTED_XBOX_LIVE_PARTY = 0x36,
            DOWNLOAD = 12,
            DOWNLOAD_STOPPED_FOR_X = 0x21,
            DOWNLOADED = 0x37,
            FAMILY_TIMER_X_TIME_REMAINING = 0x2d,
            FLASH_LOGO = 0x17,
            FLASHING_CHAT_ICON = 0x26,
            FLASHING_CHAT_SYMBOL = 0x41,
            FLASHING_DOUBLE_SIDED_HAMMER = 0x10,
            FLASHING_FROWNING_FACE = 15,
            FLASHING_HAPPY_FACE = 14,
            FLASHING_MUSIC_SYMBOL = 13,
            FLASHING_XBOX_CONSOLE = 0x22,
            FLASHING_XBOX_LOGO = 4,
            FOUR_2 = 0x19,
            FOUR_3 = 0x1a,
            FOUR_5 = 0x30,
            FOUR_7 = 0x25,
            FOUR_9 = 0x1c,
            FRIEND_REQUEST_LOGO = 2,
            GAME_INVITE_SENT = 0x16,
            GAME_INVITE_SENT_TO_XBOX_LIVE_PARTY = 0x33,
            GAMER_PICTURE_UNLOCKED = 0x3b,
            GAMERTAG_HAS_JOINED_CHAT = 20,
            GAMERTAG_HAS_JOINED_XBL_PARTY = 0x39,
            GAMERTAG_HAS_LEFT_CHAT = 0x15,
            GAMERTAG_HAS_LEFT_XBL_PARTY = 0x3a,
            GAMERTAG_SENT_YOU_A_MESSAGE = 5,
            GAMERTAG_SIGNED_IN_OFFLINE = 9,
            GAMERTAG_SIGNED_INTO_XBOX_LIVE = 8,
            GAMERTAG_SIGNEDIN = 7,
            GAMERTAG_SINGED_OUT = 6,
            GAMERTAG_WANTS_TO_CHAT = 10,
            GAMERTAG_WANTS_TO_CHAT_2 = 0x11,
            GAMERTAG_WANTS_TO_TALK_IN_VIDEO_KINECT = 0x1d,
            GAMERTAG_WANTS_YOU_TO_JOIN_AN_XBOX_LIVE_PARTY = 0x31,
            JOINED_XBL_PARTY = 0x3d,
            KICKED_FROM_XBOX_LIVE_PARTY = 0x34,
            KINECT_HEALTH_EFFECTS = 0x2f,
            MESSENGER_DISCONNECTED = 0x29,
            MISSED_MESSENGER_CONVERSATION = 0x2c,
            NEW_MESSAGE = 3,
            NEW_MESSAGE_LOGO = 1,
            NULLED = 0x35,
            PAGE_SENT_TO = 0x18,
            PARTY_INVITE_SENT = 50,
            PLAYER_MUTED = 0x3f,
            PLAYER_UNMUTED = 0x40,
            PLEASE_RECONNECT_CONTROLLERM = 0x13,
            PLEASE_REINSERT_MEMORY_UNIT = 0x12,
            PLEASE_REINSERT_USB_STORAGE_DEVICE = 0x3e,
            READY_TO_PLAY = 0x1f,
            UPDATING = 0x4c,
            VIDEO_CHAT_INVITE_SENT = 30,
            X_HAS_SENT_YOU_A_NUDGE = 40,
            X_SENT_YOU_A_GAME_MESSAGE = 0x23,
            XBOX_LOGO = 0
        }
    }
}

