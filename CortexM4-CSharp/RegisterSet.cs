using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp
{
    internal class RegisterSet
    {
        // Define the registers
        public uint[] R { get; private set; } // General-purpose registers (R0-R15)
        public uint PC { get; set; } // Program Counter
        public uint SP { get; set; } // Stack Pointer
        public uint N { get; set; } // Negative flag
        public uint Z { get; set; } // Zero flag
        public uint C { get; set; } // Carry flag
        public uint V { get; set; } // Overflow flag

        public RegisterSet()
        {
            // Initialize the registers
            R = new uint[16];
            PC = 0;
            SP = 0;
            N = 0;
            Z = 0;
            C = 0;
            V = 0;
        }
    }
}
