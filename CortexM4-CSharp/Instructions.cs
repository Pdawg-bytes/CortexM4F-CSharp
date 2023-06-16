using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp
{
    internal class Instructions
    {
        public static void Add(RegisterSet registers, int dest, int r1, int r2)
        {
            registers.R[dest] = r1 + r2;
        }

        public static void Subtract(RegisterSet registers, int dest, int r1, int r2)
        {
            registers.R[dest] = r1 - r2;
        }

        public static void Move(RegisterSet registers, int rd, int immediate)
        {
            registers.R[rd] = immediate;
        }
    }
}
