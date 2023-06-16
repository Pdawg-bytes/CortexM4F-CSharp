using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp
{
    internal static class OpCodes
    {
        // Data Processing OpCodes
        internal const byte MOV = 0x01;
        internal const byte ADD = 0x02;
        internal const byte SUB = 0x03;
        internal const byte AND = 0x04;
        internal const byte ORR = 0x05;
        internal const byte EOR = 0x06;

        // Load and Store OpCodes
        internal const byte LDR = 0x07;
        internal const byte STR = 0x08;

        // Branch and Control Flow OpCodes
        internal const byte B = 0x09;
        internal const byte BL = 0x0A;
        internal const byte BX = 0x0B;
        internal const byte POP = 0x0C;
        internal const byte PUSH = 0x0D;

        // Multiply and Divide OpCodes
        internal const byte MUL = 0x0E;
        internal const byte MLA = 0x0F;
        internal const byte SDIV = 0x10;
        internal const byte UDIV = 0x11;

        // Control and Status Register OpCodes
        internal const byte MRS = 0x12;

        // FPU OpCodes
        internal const byte VADD = 0x13;
        internal const byte VSUB = 0x14;
        internal const byte VMUL = 0x15;
        internal const byte VDIV = 0x16;

        // SIMD OpCodes
        internal const byte SIMD_VADD = 0x17;
        internal const byte SIMD_VSUB = 0x18;
        internal const byte SIMD_VMUL = 0x19;
        internal const byte SIMD_VMOV = 0x20;
    }
}
