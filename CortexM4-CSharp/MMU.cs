using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexM4_CSharp.New
{
    internal class MMU
    {
        private byte[] codeRegion;
        private byte[] sramRegion;
        private List<Peripheral> peripherals;

        private const uint codeBegin = 0; // Replace with the actual code start address
        private const uint codeEnd = 255; // Replace with the actual code end address
        private const uint sramBegin = 0; // Replace with the actual SRAM start address
        private const uint sramEnd = 0;   // Replace with the actual SRAM end address

        /// <summary>
        /// The CPU's Memory Management Unit
        /// </summary>
        /// <param name="codeRegion">Array of space for code</param>
        /// <param name="sramRegion">Array of space for SRAM storage</param>
        public MMU(byte[] codeRegion, byte[] sramRegion)
        {
            this.codeRegion = codeRegion;
            this.sramRegion = sramRegion;
            peripherals = new List<Peripheral>();
        }

        /// <summary>
        /// Registers a peripheral to the MMU
        /// </summary>
        /// <param name="periph">The peripheral we want to add</param>
        /// <exception cref="Exception">[!] Couldn't register peripheral</exception>
        public void RegisterPeripheral(Peripheral periph)
        {
            // Check if there is a peripheral that is conflicting with this new one
            foreach (Peripheral r in peripherals)
            {
                if (r.InConflict(periph))
                {
                    throw new Exception("[!] Could not register the peripheral " + periph.GetName() + " in conflict with: " + r.GetName() + "\n");
                }
            }
            peripherals.Add(periph);
        }

        /// <summary>
        /// Writes a 32bit value to a 32bit address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <param name="value">The value to be written</param>
        public void Write32(uint address, uint value)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Copy(valueBytes, 0, codeRegion, address - codeBegin, sizeof(uint));
                return;
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Copy(valueBytes, 0, sramRegion, address - sramBegin, sizeof(uint));
                return;
            }
        }

        /// <summary>
        /// Reads a 32bit value from a given address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <returns>The value that was read from the address</returns>
        public uint Read32(uint address)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                return BitConverter.ToUInt32(codeRegion, (int)(address - codeBegin));
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                return BitConverter.ToUInt32(sramRegion, (int)(address - sramBegin));
            }

            return 0;
        }


        /// <summary>
        /// Writes a 16bit value to a 32bit address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <param name="value">The value to be written</param>
        public void Write16(uint address, ushort value)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Copy(valueBytes, 0, codeRegion, address - codeBegin, sizeof(ushort));
                return;
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Copy(valueBytes, 0, sramRegion, address - sramBegin, sizeof(ushort));
                return;
            }
        }

        /// <summary>
        /// Reads a 16bit value from a given address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <returns>The value that was read from the address</returns>
        public ushort Read16(uint address)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                return BitConverter.ToUInt16(codeRegion, (int)(address - codeBegin));
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                return BitConverter.ToUInt16(sramRegion, (int)(address - sramBegin));
            }

            return 0;
        }

        /// <summary>
        /// Reads a 16bit signed value from a given address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <returns>The value that was read from the address</returns>
        public ushort Read16s(uint address)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                return BitConverter.ToUInt16(codeRegion, (int)(address - codeBegin));
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                return BitConverter.ToUInt16(sramRegion, (int)(address - sramBegin));
            }

            return 0;
        }


        /// <summary>
        /// Writes an 8bit value to a 32bit address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <param name="value">The value to be written</param>
        public void Write8(uint address, byte value)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                codeRegion[address - codeBegin] = value;
                return;
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                sramRegion[address - sramBegin] = value;
                return;
            }
        }

        /// <summary>
        /// Reads an 8bit value from a given address
        /// </summary>
        /// <param name="address">32bit address</param>
        /// <returns>The value that was read from the address</returns>
        public byte Read8(uint address)
        {
            if (address <= codeEnd && address >= codeBegin)
            {
                return codeRegion[address - codeBegin];
            }
            if (address <= sramEnd && address >= sramBegin)
            {
                return sramRegion[address - sramBegin];
            }

            return 0;
        }
    }
}
