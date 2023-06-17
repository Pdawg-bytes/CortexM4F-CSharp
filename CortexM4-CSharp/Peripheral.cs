using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp.New
{
    internal abstract class Peripheral
    {
        protected uint start_address;
        protected uint end_address;
        protected string name;

        public Peripheral(uint start_address, uint end_address)
        {
            this.start_address = start_address;
            this.end_address = end_address;
        }

        public abstract void WriteWord(uint address, byte value);
        public abstract void WriteUshort(uint address, ushort value);
        public abstract void WriteUint(uint address, uint value);
        public abstract void ReadByte(uint address, out byte value);
        public abstract void ReadUshort(uint address, out ushort value);
        public abstract void ReadUint(uint address, out uint value);

        public uint GetStartAddress()
        {
            return start_address;
        }

        public uint GetEndAddress()
        {
            return end_address;
        }

        public string GetName()
        {
            return name;
        }

        public bool InConflict(Peripheral p)
        {
            return (p.start_address >= this.start_address && p.start_address <= this.end_address) ||
                   (this.start_address >= p.start_address && this.start_address <= p.end_address);
        }

        public bool InRange(uint address)
        {
            return this.start_address >= address && address < this.end_address;
        }
    }
}
