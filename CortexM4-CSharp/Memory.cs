﻿using static CortexM4_CSharp.OpCodes;

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
    }
}
