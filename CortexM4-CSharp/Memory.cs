using static CortexM4_CSharp.OpCodes;

namespace CortexM4_CSharp
{
    internal class Memory
    {
        public byte[] memory;

        public Memory()
        {
            memory = new byte[64]; // 64B memory
        }

        public byte[] ReadWord(uint address)
        {
            byte[] word = new byte[4];
            Buffer.BlockCopy(memory, (int)address, word, 0, 4);
            return word;
        }

        public void WriteWord(uint address, uint value)
        {
            byte[] word = BitConverter.GetBytes(value);
            Buffer.BlockCopy(word, 0, memory, (int)address, 4);
        }

        // Helper functions
        public byte[] Reg32ToReg8(int regValue)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(regValue & 0xFF);
            bytes[1] = (byte)((regValue >> 8) & 0xFF);
            bytes[2] = (byte)((regValue >> 16) & 0xFF);
            bytes[3] = (byte)((regValue >> 24) & 0xFF);
            return bytes;
        }
        public int Reg8ToReg32(byte[] reg8)
        {
            int reg32 = 0;

            reg32 |= reg8[0];
            reg32 |= (reg8[1] << 8);
            reg32 |= (reg8[2] << 16);
            reg32 |= (reg8[3] << 24);

            return reg32;
        }
    }
}
