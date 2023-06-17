using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp.New
{
    internal class Registers
    {
        // Define registers
        public uint[] R { get; private set; } // General-purpose registers (R0-R15)
        public uint PC { get; set; } // Program Counter
        public uint SP { get; set; } // Stack Pointer
        public bool N { get; set; } // Negative flag
        public bool Z { get; set; } // Zero flag
        public bool C { get; set; } // Carry flag
        public bool V { get; set; } // Overflow flag
        public bool T { get; set; } // Thumb state
        public ushort[] PrefetchVal { get; set; }

        public Registers()
        {
            // Initialize registers
            R = new uint[16];
            PC = 0;
            SP = 0;
            N = false;
            Z = false;
            C = false;
            V = false;
            T = false;
            PrefetchVal = new ushort[2];
        }
    }
}
