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

            /*// MOV R0, #12
            cpu.mmu.Write16(codeInitAddress, 0x200C);

            // MOV R1, #1
            cpu.mmu.Write16(codeInitAddress + 2, 0x2101);

            // ADD R0, R1
            cpu.mmu.Write16(codeInitAddress + 4, 0x1840);*/

            cpu.mmu.Write16(codeInitAddress, 0x2100);
            cpu.mmu.Write16(codeInitAddress + 2, 0xFF7F);
            cpu.mmu.Write16(codeInitAddress + 4, 0x0080);
            cpu.mmu.Write16(codeInitAddress + 6, 0x2101);
            cpu.mmu.Write16(codeInitAddress + 8, 0x7FFF);
            cpu.mmu.Write16(codeInitAddress + 10, 0x4308);

            cpu.Reset();
            cpu.VerboseRun(39);
        }
    }
}