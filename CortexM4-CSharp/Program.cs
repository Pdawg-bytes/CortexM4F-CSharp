using CortexM4_CSharp.New;
using System;

namespace CortexM4_CSharp
{
    internal class Program
    {
        const int codeInitAddress = 0x00000058;
        const int pcInitAddress = 0x00000004;
        public static void Main(string[] args)
        {
            CPU cpu = new CPU();

            cpu.mmu.Write32(pcInitAddress, codeInitAddress);

            // MOV R0, #12
            cpu.mmu.Write16(codeInitAddress, 0x200C);

            // MOV R1, #1
            cpu.mmu.Write16(codeInitAddress + 2, 0x2101);

            // ADD R0, R1
            cpu.mmu.Write16(codeInitAddress + 4, 0x1840);

            cpu.Reset();
            cpu.Run(2000);
        }
    }
}