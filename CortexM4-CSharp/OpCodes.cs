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
        internal const byte MOV = 0x00;
        internal const byte ADD = 0x01;
        internal const byte SUB = 0x02;
        internal const byte AND = 0x03;
        internal const byte ORR = 0x04;
        internal const byte EOR = 0x05;

        // Load and Store OpCodes
        internal const byte LDR = 0x06;
        internal const byte STR = 0x07;

        // Branch and Control Flow OpCodes
        internal const byte B = 0x08;
        internal const byte BL = 0x09;
        internal const byte BX = 0x0A;
        internal const byte POP = 0x0B;
        internal const byte PUSH = 0x0C;

        // Multiply and Divide OpCodes
        internal const byte MUL = 0x0D;
        internal const byte MLA = 0x0E;
        internal const byte SDIV = 0x0F;
        internal const byte UDIV = 0x10;

        // Control and Status Register OpCodes
        internal const byte MRS = 0x20;

        // FPU OpCodes
        internal const byte VADD = 0x30;
        internal const byte VSUB = 0x40;
        internal const byte VMUL = 0x50;
        internal const byte VDIV = 0x60;

        // SIMD OpCodes
        internal const byte SIMD_VADD = 0x70;
        internal const byte SIMD_VSUB = 0x80;
        internal const byte SIMD_VMUL = 0x90;
        internal const byte SIMD_VMOV = 0xA0;
    }
}
