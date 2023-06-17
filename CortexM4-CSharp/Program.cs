using CortexM4_CSharp.New;
using System;

namespace CortexM4_CSharp
{
    internal class Program
    {
        const int CODE_INIT_ADDRESS = 0x00000058;
        const int PC_INIT_ADDRESS = 0x00000004;
        public static void Main(string[] args)
        {
            CPU cpu = new CPU();

            cpu.mmu.Write32(PC_INIT_ADDRESS, CODE_INIT_ADDRESS);

            // MOV R0, #12
            cpu.mmu.Write16(CODE_INIT_ADDRESS, 0x200C);

            // MOV R1, #1
            cpu.mmu.Write16(CODE_INIT_ADDRESS + 2, 0x2101);

            // ADD R0, R1
            cpu.mmu.Write16(CODE_INIT_ADDRESS + 4, 0x1840);

            cpu.Reset();
            cpu.VerboseRun(1000);
        }
    }
}