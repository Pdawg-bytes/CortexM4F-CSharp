using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CortexM4_CSharp.New
{
    internal class Utils
    {
        public static uint Neg(uint i)
        {
            return (uint)(i >> 31);
        }

        public static uint Pos(uint i)
        {
            return (uint)((~i) >> 31);
        }

        public static bool AddCarry(uint a, uint b, uint c)
        {
            return ((Neg(a) & Neg(b)) | (Neg(a) & Pos(c)) | (Neg(b) & Pos(c))) != 0;
        }

        public static bool AddOverflow(uint a, uint b, uint c)
        {
            return ((Neg(a) & Neg(b) & Pos(c)) | (Pos(a) & Pos(b) & Neg(c))) != 0;
        }

        public static bool SubCarry(uint a, uint b, uint c)
        {
            return ((Neg(a) & Pos(b)) | (Neg(a) & Pos(c)) | (Pos(b) & Pos(c))) != 0;
        }

        public static bool SubOverflow(uint a, uint b, uint c)
        {
            return ((Neg(a) & Pos(b) & Pos(c)) | (Pos(a) & Neg(b) & Neg(c))) != 0;
        }

        public static ushort[] ToHalfWord(uint value)
        {
            ushort word1 = (ushort)(value & 0xFFFF);         // Extract the lower 16 bits
            ushort word2 = (ushort)((value >> 16) & 0xFFFF); // Shift right by 16 bits and extract the lower 16 bits
            return new ushort[2] { word1, word2 };
        }
    }
}
