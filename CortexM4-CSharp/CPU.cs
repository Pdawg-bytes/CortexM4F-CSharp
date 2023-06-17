using System;
using System.Diagnostics;
using static CortexM4_CSharp.New.Utils;

namespace CortexM4_CSharp.New
{
    internal class CPU
    {
        const byte REGISTER_MASK = 0b0000000000000111;
        const byte OFFSET_5_MASK = 0b0000000000011111;
        const byte OFFSET_8_MASK = 0b0000000011111111;
        const byte OPERATION_2_MASK = 0b0000000000000011;
        const byte OPERATION_4_MASK = 0b0000000000000111;
        const byte FLAG_MASK = 0b0000000000000001;
        const byte FLAG_MASK_2 = 0b0000000000000011;
        const byte FLAG_MASK_4 = 0b0000000000001111;

        public bool holdState = false;

        public MMU mmu;

        public Registers registers = new Registers();

        public int[] cpuBitsSet = new int[256];
        const int PC_INIT_ADDRESS = 0x00000004;
        private int instrBase = 0;

        public enum CPUMode
        {
            THREAD_MODE,
            HANDLER_MODE
        };

        /// <summary>
        /// Provides the entry point to the emulated ARMV7 CPU.
        /// </summary>
        public CPU() 
        {
            mmu = new MMU(new byte[256], new byte[256]);

            Reset();

            InitCpuBitsSet();
        }

        #region Run
        public void Run()
        {
            while (!holdState)
            {
                ushort instr = registers.PrefetchVal[0];
                registers.PrefetchVal[0] = registers.PrefetchVal[1];

                registers.PC = registers.R[15];
                registers.R[15] += 2;
                PrefetchNext();

                ExecuteOp(instr);
            }
            Print();
        }

        public void Run(uint n_instr)
        {
            Prefetch();

            while (n_instr > 0)
            {
                ushort instr = registers.PrefetchVal[0];
                registers.PrefetchVal[0] = registers.PrefetchVal[1];

                registers.PC = registers.R[15];
                registers.R[15] += 2;
                PrefetchNext();

                ExecuteOp(instr);

                n_instr--;
            }

            Print();
        }

        public void VerboseRun(uint n_instr)
        {
            Console.WriteLine("Starting PC: 0x" + registers.PC.ToString("X"));
            Prefetch();

            while(n_instr > 0)
            {
                ushort instr = registers.PrefetchVal[0];
                registers.PrefetchVal[0] = registers.PrefetchVal[1];

                Console.WriteLine("Executing instruction : 0x" + instr.ToString("X") + " | PC: " + registers.PC);

                registers.PC = registers.R[15];
                registers.R[15] += 2;
                PrefetchNext();

                ExecuteOp(instr);

                n_instr--;
            }
            Print();
        }

        public void Print()
        {
            Console.WriteLine("\nRegisters: ");

            for (int i = 0; i < 15; ++i)
            {
                Console.WriteLine("Register " + i + " contains value: " + registers.R[i]);
            }

            Console.WriteLine();
            Console.WriteLine("Thumb state: " + registers.T);
            Console.WriteLine("Carry flag: " + registers.C);
            Console.WriteLine("Negative flag: " + registers.N);
            Console.WriteLine("Overflow flag: " + registers.V);
            Console.WriteLine("Zero flag: " + registers.Z + "\n");
            Console.WriteLine("Final Program Counter (R15): " + registers.R[15]);
        }
        #endregion

        #region CPU Utils
        public void Reset()
        {
            CPUMode mode = CPUMode.THREAD_MODE;

            // Set thumb register
            registers.T = true;

            // Initialize the program counter
            registers.PC = mmu.Read32(PC_INIT_ADDRESS);

            registers.R[15] = registers.PC + 2;
        }

        public void Prefetch()
        {
            registers.PrefetchVal[0] = mmu.Read16(registers.PC);
            registers.PrefetchVal[1] = mmu.Read16(registers.PC + 2);
        }
        public void PrefetchNext()
        {
            registers.PrefetchVal[1] = mmu.Read16(registers.PC + 2);
        }


        #region Register Operations
        private void PushReg(ushort instruction, ref uint address, int val, int reg)
        {
            if ((instruction & val) != 0)
            {
                mmu.Write32(address, registers.R[reg]);
                address += 4;
            }
        }
        private void PopReg(ushort instr, ref uint address, int val, int reg)
        {
            if ((instr & val) != 0)
            {
                registers.R[reg] = mmu.Read32(address);
                address += 4;
            }
        }

        private void ThumbLdmReg(uint instr, ref uint address, int val, int r)
        {
            if ((instr & val) != 0)
            {
                registers.R[r] = mmu.Read32(address);
                address += 4;
            }
        }
        private void ThumbStmReg(uint instr, ref uint address, int val, int r)
        {
            if ((instr & val) != 0)
            {
                mmu.Write32(address, registers.R[r]);
                address += 4;
            }
        }
        #endregion

        private void InitCpuBitsSet()
        {
            for (int i = 0; i < 256; i++)
            {
                int count = 0;
                for (int j = 0; j < 8; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        count++;
                    }
                }
                cpuBitsSet[i] = count;
            }
        }
        #endregion

        /// <summary>
        /// Executes the ARM ASM instruction
        /// </summary>
        /// <param name="instruction">The CPU instruction.</param>
        /// <exception cref="Exception">Throws when an unrecognized Opcode is sent to the CPU.</exception>
        void ExecuteOp(ushort instruction)
        {
            if (instruction == 0b0100011011000000)
            {
                NOP(instruction);
            }
            else if ((0b1111111111101111 & instruction) == 0b1011011001100010)
            {
                CpsiDE(instruction);
            }
            else if ((0b1111111111101111 & instruction) == 0b1011111100100000)
            {
                WaitForInterruptEvent(instruction);
            }
            else if (instruction == 0b1011111101000000)
            {
                SendEvent(instruction);
            }
            else if ((instruction & 0xFF00) == 0b1101111100000000)
            {
                SupervisorCall(instruction);
            }
            else if ((instruction & 0xFF00) == 0b1101111000000000)
            {
                Breakpoint(instruction);
            }
            else if ((instruction & 0xFF00) == 0b1101111100000000)
            {
                SoftwareInterrupt(instruction);
            }
            else if ((instruction & 0xFF00) == 0b1011000000000000)
            {
                AddOffsetToStackPointer(instruction);
            }
            else if ((instruction & 0b1111001000000000) == 0b0101000000000000)
            {
                LoadStoreWithRegisterOffset(instruction);
            }
            else if ((instruction & 0b1111001000000000) == 0b0101001000000000)
            {
                LoadStoreSignExtendedByteHalfword(instruction);
            }
            else if ((instruction & 0b1111011000000000) == 0b1011010000000000)
            {
                PushPopRegisters(instruction);
            }
            else if ((instruction & 0b1111110000000000) == 0b0100000000000000)
            {
                ALUOperations(instruction);
            }
            else if ((instruction & 0b1111110000000000) == 0b0100010000000000)
            {
                HiRegisterOperationsBranchExchange(instruction);
            }
            else if ((instruction & 0b1111100000000000) == 0b0001100000000000)
            {
                AddSubtract(instruction);
            }
            else if ((instruction & 0b1111100000000000) == 0b0100100000000000)
            {
                PCRelativeLoad(instruction);
            }
            else if ((instruction & 0b1111100000000000) == 0b0111000000000000)
            {
                UnconditionalBranch(instruction);
            }
            else if ((instruction & 0b1111100000000000) == 0b0111000000000000)
            {
                UnconditionalBranch(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1000000000000000)
            {
                LoadStoreHalfwordImmediateOffset(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1001000000000000)
            {
                SPRelativeLoadStore(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1010000000000000)
            {
                LoadAddress(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1100000000000000)
            {
                MultipleLoadStore(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1101000000000000)
            {
                ConditionalBranch(instruction);
            }
            else if ((instruction & 0b1111000000000000) == 0b1111000000000000)
            {
                LongBranchWithLink(instruction);
            }
            else if ((instruction & 0b1110000000000000) == 0b0010000000000000)
            {
                MoveCompareAddSubtractImmediate(instruction);
            }
            else if ((instruction & 0b1110000000000000) == 0b0000000000000000)
            {
                MoveShiftedRegister(instruction);
            }
            else if ((instruction & 0b1110000000000000) == 0b0110000000000000)
            {
                LoadStoreWithImmediateOffset(instruction);
            }
            else
            {
                throw new Exception("[!] This instruction is unknown or unimplemented. | EXEC, " + instruction);
            }
        }

        #region Instructions
        // CPU Instructions
        public void NOP(ushort instruction) { /* Do nothing, it's a NOP! */ }

        public void PCRelativeLoad(ushort instruction)
        {
            // LDR Rd, [PC, #Imm] same as ADR Rd, label
            uint address = (uint)((registers.R[15] & 0xFFFFFFFC) + ((instruction & 0xFF) << 2));
            registers.R[(instruction >> 8) & 7] = mmu.Read32(address);
        }

        public void SoftwareInterrupt(ushort instruction)
        {
            throw new InvalidOperationException("[!] Software interrupt is not implemented");
        }

        public void DataMemSyncBarier(uint instruction)
        {
            throw new InvalidOperationException("[!] Data Memory Barrier and Data Synchronization Barrier not implemented yet!");
        }

        public void CpsiDE(ushort instruction)
        {
            throw new InvalidOperationException("[!] CPSIE and CPSID are not implemented yet!");
        }

        public void SupervisorCall(ushort instruction)
        {
            throw new InvalidOperationException("[!] The supervisor call instruction is not implemented yet!");
        }

        public void Breakpoint(ushort instruction)
        {
            throw new InvalidOperationException("[!] The breakpoint instruction is not implemented yet!");
        }

        public void WaitForInterruptEvent(ushort instruction)
        {
            throw new InvalidOperationException("[!] Wait For Event and Wait For Interrupt are not implemented yet!");
        }

        public void SendEvent(ushort instruction)
        {
            throw new InvalidOperationException("[!] Send Event is not implemented yet!");
        }

        public void InstructionSyncBarier(uint instruction)
        {
            throw new InvalidOperationException("[!] Instruction Synchronization Barrier is not implemented yet");
        }

        public void SignZeroExtendByteHalfword(uint instruction)
        {
            throw new InvalidOperationException("[!] Sign extend or zero extend is not implemented yet");
        }

        public void MoveShiftedRegister(ushort instruction)
        {
            int rd = instruction & REGISTER_MASK;
            int rs = (instruction >> 3) & REGISTER_MASK;
            int offset5 = (instruction >> 6) & OFFSET_5_MASK;
            int op = (instruction >> 11) & OPERATION_2_MASK;

            switch (op)
            {
                // the case of left shift
                // LSL Rd, Rs, #Offset5
                case 0b00:
                    {
                        uint value;
                        registers.C = ((registers.R[rs] >> (32 - offset5)) & 1) != 0;
                        value = registers.R[rs] << offset5;
                        registers.R[rd] = value;
                        registers.N = (value & 0x80000000) != 0;
                        registers.Z = value == 0;
                        break;
                    }
                // the case of logical right shift
                // LSR Rd, Rs, #Offset5
                case 0b01:
                    {
                        uint value;
                        registers.C = ((registers.R[rs] >> (offset5 - 1)) & 1) != 0;
                        value = registers.R[rs] >> offset5;
                        registers.R[rd] = value;
                        registers.N = (value & 0x80000000) != 0;
                        registers.Z = value == 0;
                        break;
                    }
                // the case of arithmetic right shift
                // ASR Rd, Rs, #Offset5
                case 0b10:
                    {
                        uint value;
                        registers.C = ((registers.R[rs] >> (offset5 - 1)) & 1) != 0;
                        value = registers.R[rs] >> offset5;
                        registers.R[rd] = (uint)value;
                        registers.N = (value & 0x80000000) != 0;
                        registers.Z = value == 0;
                        break;
                    }
                default:
                    throw new NotSupportedException("[!] The operation in the move shifted register is unsupported! | MOV SHFT REG, " + instruction);
            }
        }

        public void AddSubtract(ushort instruction)
        {
            int rd = instruction & REGISTER_MASK;
            int rs = (instruction >> 3) & REGISTER_MASK;
            uint rn_offset3 = (uint)((instruction >> 6) & REGISTER_MASK);
            int op = (instruction >> 9) & FLAG_MASK;
            int i = (instruction >> 10) & FLAG_MASK;

            uint value = (i == 0) ? registers.R[rn_offset3] : rn_offset3;

            registers.R[rd] = (op == 0) ? registers.R[rs] + value : registers.R[rs] - value;

            registers.Z = registers.R[rd] == 0;
            registers.N = Neg(registers.R[rd]) != 0;
            registers.C = AddCarry(registers.R[rs], value, registers.R[rd]);
            registers.V = AddOverflow(registers.R[rs], value, registers.R[rd]);
        }

        public void MoveCompareAddSubtractImmediate(ushort instruction)
        {
            uint offset8 = (uint)instruction & OFFSET_8_MASK;
            int rd = (instruction >> 8) & REGISTER_MASK;
            int op = (instruction >> 11) & OPERATION_2_MASK;

            switch (op)
            {
                case 0b00:
                    registers.R[rd] = offset8;
                    registers.N = false;
                    registers.Z = registers.R[rd] == 0;
                    break;
                case 0b01:
                    uint lhs = registers.R[rd];
                    uint rhs = (uint)instruction & 0xFF;
                    uint res = lhs - rhs;

                    registers.Z = res == 0;
                    registers.N = Neg(res) != 0;
                    registers.C = SubCarry(lhs, rhs, res);
                    registers.V = SubOverflow(lhs, rhs, res);

                    break;

                case 0b10:
                    lhs = registers.R[rd];
                    rhs = (uint)instruction & 0xFF;
                    res = lhs + rhs;
                    registers.R[rd] = res;

                    registers.Z = res == 0;
                    registers.N = Neg(res) != 0;
                    registers.C = AddCarry(lhs, rhs, res);
                    registers.V = AddOverflow(lhs, rhs, res);

                    break;

                case 0b11:
                    lhs = registers.R[rd];
                    rhs = (uint)instruction & 0xFF;
                    res = lhs - rhs;
                    registers.R[rd] = res;

                    registers.Z = res == 0;
                    registers.N = Neg(res) != 0;
                    registers.C = AddCarry(lhs, rhs, res);
                    registers.V = SubOverflow(lhs, rhs, res);

                    break;

                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | MOV COMP ADD SUB IMME, " + instruction);
            }
        }

        public void ALUOperations(ushort instruction)
        {
            // | 0 1 0 0 0 0 | Op | Rs | Rd |
            int rd = instruction & REGISTER_MASK;
            int rs = (instruction >> 3) & REGISTER_MASK;
            int op = (instruction >> 6) & OPERATION_4_MASK;

            switch (op)
            {
                // AND Rd, Rs
                case 0b0000:
                    {
                        registers.R[rd] &= registers.R[rs];

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // EOR Rd, Rs
                case 0b0001:
                    {
                        registers.R[rd] ^= registers.R[rs];

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // LSL Rd, Rs
                case 0b0010:
                    {
                        int value = BitConverter.GetBytes(registers.R[rs])[0];
                        if (value != 0)
                        {
                            if (value == 32)
                            {
                                value = 0;
                                registers.C = (registers.R[rd] & 1) != 0;
                            }
                            else if (value < 32)
                            {
                                registers.C = ((registers.R[rd] >> (32 - value)) & 1) != 0;
                                value = (int)(registers.R[rd] << value);
                            }
                            else
                            {
                                value = 0;
                                registers.C = false;
                            }
                            registers.R[rd] = (uint)value;
                        }

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // LSR Rd, Rs
                case 0b0011:
                    {
                        int value = BitConverter.GetBytes(registers.R[rs])[0];
                        if (value != 0)
                        {
                            if (value == 32)
                            {
                                value = 0;
                                registers.C = (registers.R[rd] & 0x80000000) != 0;
                            }
                            else if (value < 32)
                            {
                                registers.C = ((registers.R[rd] >> value - 1) & 1) != 0;
                                value = (int)(registers.R[rd] >> value);
                            }
                            else
                            {
                                value = 0;
                                registers.C = false;
                            }
                            registers.R[rd] = (uint)value;
                        }

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // ASR Rd, Rs
                case 0b0100:
                    {
                        int value = BitConverter.GetBytes(registers.R[rs])[0];
                        if (value != 0)
                        {
                            if (value < 32)
                            {
                                registers.C = (((int)registers.R[rd] >> (value - 1)) & 1) != 0;
                                value = (int)registers.R[rd] >> value;
                            }
                            else
                            {
                                if ((registers.R[rd] & 0x80000000) != 0)
                                    value = -1;
                                else
                                    value = 0;
                                registers.C = false;
                            }
                            registers.R[rd] = (uint)value;
                        }

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // ADC Rd, Rs
                case 0b0101:
                    {
                        uint result = registers.R[rd] + registers.R[rs] + (registers.C ? 1u : 0u);
                        registers.C = result < registers.R[rd] || result < registers.R[rs];

                        registers.R[rd] = result;

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // SBC Rd, Rs
                case 0b0110:
                    {
                        uint result = registers.R[rd] - (registers.R[rs] + (registers.C ? 1u : 0u));
                        registers.C = result > registers.R[rd];

                        registers.R[rd] = result;

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // TST Rd, Rs
                case 0b1000:
                    {
                        uint result = registers.R[rd] & registers.R[rs];

                        // update the flags
                        registers.N = (result & 0x80000000) != 0;
                        registers.Z = result == 0;

                        break;
                    }
                // CMP Rd, Rs
                case 0b1010:
                    {
                        uint result = registers.R[rd] - registers.R[rs];

                        // update the flags
                        registers.N = (result & 0x80000000) != 0;
                        registers.Z = result == 0;
                        registers.C = registers.R[rd] >= registers.R[rs];

                        break;
                    }
                // ORR Rd, Rs
                case 0b1100:
                    {
                        registers.R[rd] |= registers.R[rs];

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // MUL Rd, Rs
                case 0b1101:
                    {
                        registers.R[rd] *= registers.R[rs];

                        // update the flags
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;

                        break;
                    }
                // BIC Rd, Rs
                case 0b1110:
                    {
                        registers.R[rd] &= ~registers.R[rs];
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;
                        break;
                    }
                // MVN Rd, Rs
                case 0b1111:
                    {
                        registers.R[rd] = ~registers.R[rs];
                        registers.N = (registers.R[rd] & 0x80000000) != 0;
                        registers.Z = registers.R[rd] == 0;
                        break;
                    }
                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | ALU OP, " + instruction);
            }
        }

        public void HiRegisterOperationsBranchExchange(ushort instruction)
        {
            int op_h1_h2 = (instruction >> 6) & 0b1111;

            switch (op_h1_h2)
            {
                // ADD Rd, Hs
                case 0b0001:
                    registers.R[instruction & 7] += registers.R[((instruction >> 3) & 7) + 8];
                    break;
                // ADD Hd, Rs
                case 0b0010:
                    {
                        registers.R[(instruction & 7) + 8] += registers.R[(instruction >> 3) & 7];

                        // Check if this is the PC register
                        if ((instruction & 7) == 7)
                        {
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        break;
                    }
                // ADD Hd, Hs
                case 0b0011:
                    {
                        registers.R[(instruction & 7) + 8] += registers.R[((instruction >> 3) & 7) + 8];

                        // Check if this is the PC register
                        if ((instruction & 7) == 7)
                        {
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        break;
                    }
                // CMP Rd, Hs
                case 0b0101:
                    {
                        int dest = instruction & 7;
                        uint value = registers.R[((instruction >> 3) & 7) + 8];

                        uint lhs = registers.R[dest];
                        uint rhs = value;
                        uint res = lhs - rhs;
                        registers.Z = res == 0;
                        registers.N = Neg(res) != 0;
                        registers.C = SubCarry(lhs, rhs, res);
                        registers.V = SubOverflow(lhs, rhs, res);

                        break;
                    }
                // CMP Hd, Rs
                case 0b0110:
                    {
                        int dest = (instruction & 7) + 8;
                        uint value = registers.R[(instruction >> 3) & 7];

                        uint lhs = registers.R[dest];
                        uint rhs = value;
                        uint res = lhs - rhs;
                        registers.Z = res == 0;
                        registers.N = Neg(res) != 0;
                        registers.C = SubCarry(lhs, rhs, res);
                        registers.V = SubOverflow(lhs, rhs, res);

                        break;
                    }
                // CMP Hd, Hs
                case 0b0111:
                    {
                        int dest = (instruction & 7) + 8;
                        uint value = registers.R[((instruction >> 3) & 7) + 8];

                        uint lhs = registers.R[dest];
                        uint rhs = value;
                        uint res = lhs - rhs;
                        registers.Z = res == 0;
                        registers.N = Neg(res) != 0;
                        registers.C = SubCarry(lhs, rhs, res);
                        registers.V = SubOverflow(lhs, rhs, res);

                        break;
                    }
                // MOV Rd, Hs
                case 0b1001:
                    {
                        registers.R[instruction & 7] = registers.R[((instruction >> 3) & 7) + 8];
                        break;
                    }
                // MOV Hd, Rs
                case 0b1010:
                    {
                        registers.R[(instruction & 7) + 8] = registers.R[(instruction >> 3) & 7];

                        // Check if this is the PC register
                        if ((instruction & 7) == 7)
                        {
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        break;
                    }
                // MOV Hd, Hs
                case 0b1011:
                    {
                        registers.R[(instruction & 7) + 8] = registers.R[((instruction >> 3) & 7) + 8];

                        // Check if this is the PC register
                        if ((instruction & 7) == 7)
                        {
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        break;
                    }
                // BX Rs
                case 0b1100:
                // BX Hs
                case 0b1101:
                    {
                        instrBase = (instruction >> 3) & 15;
                        registers.R[15] = registers.R[instrBase];

                        if ((registers.R[15] & 1) != 0u)
                        {
                            // We are in thumb state because the address had a 1 bit set
                            registers.T = true;

                            // Remove the last bit of the address since it's not a valid one
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        else
                        {
                            throw new InvalidOperationException("[!] Going to ARM state is not possible on an M0 CPU");
                        }
                        break;
                    }
                // BLX Rs (used for both cases)
                case 0b1110:
                case 0b1111:
                    {
                        instrBase = (instruction >> 3) & 15;
                        registers.R[14] = registers.R[15];
                        registers.R[15] = registers.R[instrBase];

                        if ((registers.R[15] & 1) != 0u)
                        {
                            // We are in thumb state because the address had a 1 bit set
                            registers.T = true;

                            // Remove the last bit of the address since it's not a valid one
                            registers.R[15] &= 0xFFFFFFFE;
                            registers.PC = (uint)registers.R[15];
                            registers.R[15] += 2;
                            Prefetch();
                        }
                        else
                        {
                            throw new InvalidOperationException("[!] Going to ARM state is not possible on an M0 CPU");
                        }
                        break;
                    }
                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | HI REG OP BE, " + instruction);
            }
        }

        public void PcRelativeLoad(ushort instruction)
        {
            uint address = (uint)((registers.R[15] & 0xFFFFFFFC) + ((instruction & 0xFF) << 2));
            registers.R[(instruction >> 8) & 7] = mmu.Read32(address);
        }

        public void LoadStoreWithRegisterOffset(ushort instruction)
        {
            int flags = (instruction >> 10) & FLAG_MASK_2;

            switch (flags)
            {
                // STR Rd, [Rb, Ro]
                case 0b00:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        mmu.Write32(address, registers.R[instruction & 7]);
                        break;
                    }
                // STRB Rd, [Rb, Ro]
                case 0b01:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        mmu.Write8(address, BitConverter.GetBytes(registers.R[instruction & 7])[0]);
                        break;
                    }
                // LDR Rd, [Rb, Ro]
                case 0b10:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        registers.R[instruction & 7] = mmu.Read32(address);
                        break;
                    }
                // LDRB Rd, [Rb, Ro]
                case 0b11:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        registers.R[instruction & 7] = mmu.Read8(address);
                        break;
                    }
                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | LDR STR REG OFFSET, " + instruction);
            }
        }

        public void LoadStoreSignExtendedByteHalfword(ushort instruction)
        {
            int flags = (instruction >> 10) & FLAG_MASK_2;

            switch (flags)
            {
                // STRH Rd, [Rb, Ro]
                case 0b00:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        mmu.Write16(address, ToHalfWord(registers.R[instruction & 7])[0]);
                        break;
                    }
                // LDRH Rd, [Rb, Ro]
                case 0b01:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        registers.R[instruction & 7] = mmu.Read16(address);
                        break;
                    }
                // LDSB Rd, [Rb, Ro]
                case 0b10:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        registers.R[instruction & 7] = mmu.Read8(address);
                        break;
                    }
                // LDSH Rd, [Rb, Ro]
                case 0b11:
                    {
                        uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                        registers.R[instruction & 7] = mmu.Read16s(address);
                        break;
                    }
                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | LDR STR SIGN EXT BYTE HALF, " + instruction);
            }
        }

        public void LoadStoreWithImmediateOffset(ushort instruction)
        {
            int flags = (instruction >> 11) & FLAG_MASK_2;

            switch (flags)
            {
                case 0b00:
                    // STR Rd, [Rb, #Imm]
                    uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                    mmu.Write32(address, registers.R[instruction & 7]);
                    break;
                case 0b10:
                    // LDR Rd, [Rb, #Imm]
                    address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                    registers.R[instruction & 7]= mmu.Read32(address);
                    break;
                case 0b01:
                    // STRB Rd, [Rb, #Imm]
                    address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                    mmu.Write8(address, BitConverter.GetBytes(registers.R[instruction & 7])[0]);
                    break;
                case 0b11:
                    // LDRB Rd, [Rb, #Imm]
                    address = (uint)(registers.R[(instruction >> 3) & 7] + ((instruction >> 6) & 31));
                    registers.R[instruction & 7] = mmu.Read8(address);
                    break;
                default:
                    throw new InvalidOperationException("The operation in the ALI is unsupported! | LDR STR IMME OFFSET, " + instruction);
            }
        }

        public void LoadStoreHalfwordImmediateOffset(ushort instruction)
        {
            int flag = (instruction >> 11) & 0b1;

            if (flag != 0)
            {
                // STRH Rd, [Rs, Rn]
                uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                mmu.Write16(address, ToHalfWord(registers.R[instruction & 7])[0]);
            }
            else
            {
                // LDRH Rd, [Rb, #Imm]
                uint address = registers.R[(instruction >> 3) & 7] + registers.R[(instruction >> 6) & 7];
                mmu.Write8(address, BitConverter.GetBytes(registers.R[instruction & 7])[0]);
            }
        }

        public void SPRelativeLoadStore(ushort instruction)
        {
            int flag = (instruction >> 11) & 0b1;

            if (flag != 0)
            {
                // STR Rd, [SP, #Imm]
                uint address = (uint)(registers.R[(instruction >> 3) & 7] + (((instruction >> 6) & 31) << 2));
                mmu.Write32(address, registers.R[instruction & 7]);
            }
            else
            {
                // LDR Rd, [SP, #Imm]
                uint address = (uint)(registers.R[(instruction >> 3) & 7] + (((instruction >> 6) & 31) << 2));
                registers.R[instruction & 7] = mmu.Read32(address);
            }
        }

        public void LoadAddress(ushort instruction)
        {
            int flag = (instruction >> 11) & 0b1;

            if (flag != 0)
            {
                // ADD Rd, PC, #Imm
                registers.R[(instruction >> 8) & 7] = (uint)((registers.R[15] & 0xFFFFFFFC) + ((instruction & 255) << 2));
            }
            else
            {
                // ADD Rd, SP, #Imm
                registers.R[(instruction >> 8) & 7] = (uint)(registers.R[13] + ((instruction & 255) << 2));
            }
        }

        public void AddOffsetToStackPointer(ushort instruction)
        {
            int flag = (instruction >> 7) & 0b1;
            uint offset = (uint)((instruction & 127) << 2);

            if (flag != 0)
            {
                // ADD SP, #Imm
                registers.R[13] += offset;
            }
            else
            {
                // ADD SP, #-Imm
                registers.R[13] -= offset;
            }
        }

        public void PushPopRegisters(ushort instruction)
        {
            int flag = ((instruction >> 8) & 0b1) | ((instruction >> 1) & 0b10);

            switch (flag)
            {
                case 0b00:
                    {
                        // PUSH { Rlist }

                        uint temp = (uint)(registers.R[13] - 4 * cpuBitsSet[instruction & 0xFF]);
                        uint address = temp & 0xFFFFFFFC;

                        // Push the selected registers from R0-R7
                        PushReg(instruction, ref address, 1, 0);
                        PushReg(instruction, ref address, 2, 1);
                        PushReg(instruction, ref address, 4, 2);
                        PushReg(instruction, ref address, 8, 3);
                        PushReg(instruction, ref address, 16, 4);
                        PushReg(instruction, ref address, 32, 5);
                        PushReg(instruction, ref address, 64, 6);
                        PushReg(instruction, ref address, 128, 7);

                        // Set the new stack pointer
                        registers.R[13] = temp;
                        break;
                    }
                case 0b01:
                    {
                        // PUSH { Rlist, LR }

                        uint temp = (uint)(registers.R[13] - 4 - 4 * cpuBitsSet[instruction & 0xFF]);
                        uint address = temp & 0xFFFFFFFC;

                        // Push the selected registers from R0-R7 including the link register
                        PushReg(instruction, ref address, 1, 0);
                        PushReg(instruction, ref address, 2, 1);
                        PushReg(instruction, ref address, 4, 2);
                        PushReg(instruction, ref address, 8, 3);
                        PushReg(instruction, ref address, 16, 4);
                        PushReg(instruction, ref address, 32, 5);
                        PushReg(instruction, ref address, 64, 6);
                        PushReg(instruction, ref address, 128, 7);
                        PushReg(instruction, ref address, 256, 14);

                        // Set the new stack pointer
                        registers.R[13] = temp;
                        break;
                    }
                case 0b10:
                    {
                        // POP { Rlist }

                        uint address = registers.R[13] & 0xFFFFFFFC;
                        uint temp = (uint)(registers.R[13] + 4 * cpuBitsSet[instruction & 0xFF]);

                        // Pop each selected register (R0-R7) from the stack
                        PopReg(instruction, ref address, 1, 0);
                        PopReg(instruction, ref address, 2, 1);
                        PopReg(instruction, ref address, 4, 2);
                        PopReg(instruction, ref address, 8, 3);
                        PopReg(instruction, ref address, 16, 4);
                        PopReg(instruction, ref address, 32, 5);
                        PopReg(instruction, ref address, 64, 6);
                        PopReg(instruction, ref address, 128, 7);

                        registers.R[13] = temp;
                        break;
                    }
                case 0b11:
                    {
                        // POP { Rlist, PC }

                        uint address = registers.R[13] & 0xFFFFFFFC;
                        uint temp = (uint)(registers.R[13] + 4 + 4 * cpuBitsSet[instruction & 0xFF]);

                        PopReg(instruction, ref address, 1, 0);
                        PopReg(instruction, ref address, 2, 1);
                        PopReg(instruction, ref address, 4, 2);
                        PopReg(instruction, ref address, 8, 3);
                        PopReg(instruction, ref address, 16, 4);
                        PopReg(instruction, ref address, 32, 5);
                        PopReg(instruction, ref address, 64, 6);
                        PopReg(instruction, ref address, 128, 7);

                        registers.R[15] = (mmu.Read32(address) & 0xFFFFFFFE);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        registers.R[13] = temp;
                        Prefetch();
                        break;
                    }
                default:
                    throw new NotSupportedException("[!] The operation in the alu is unsupported! | PUSH POP, " + instruction);
            }
        }

        public void MultipleLoadStore(ushort instruction)
        {
            int flag = (instruction >> 11) & 0b1;

            if (flag != 0)
            {
                // STMIA Rb!, { Rlist }

                int reg = (instruction >> 8) & 7;

                uint address = registers.R[reg] & 0xFFFFFFFC;
                uint temp = (uint)(registers.R[reg] + 4 * cpuBitsSet[instruction & 0xff]);

                ThumbStmReg(instruction, ref address, 1, 0);
                ThumbStmReg(instruction, ref address, 2, 1);
                ThumbStmReg(instruction, ref address, 4, 2);
                ThumbStmReg(instruction, ref address, 8, 3);
                ThumbStmReg(instruction, ref address, 16, 4);
                ThumbStmReg(instruction, ref address, 32, 5);
                ThumbStmReg(instruction, ref address, 64, 6);
                ThumbStmReg(instruction, ref address, 128, 7);

                registers.R[reg] = temp;
            }
            else
            {
                // LDMIA Rb!, { Rlist }
                int reg = (instruction >> 8) & 7;
                uint address = registers.R[reg] & 0xFFFFFFFC;

                ThumbLdmReg(instruction, ref address, 1, 0);
                ThumbLdmReg(instruction, ref address, 2, 1);
                ThumbLdmReg(instruction, ref address, 4, 2);
                ThumbLdmReg(instruction, ref address, 8, 3);
                ThumbLdmReg(instruction, ref address, 16, 4);
                ThumbLdmReg(instruction, ref address, 32, 5);
                ThumbLdmReg(instruction, ref address, 64, 6);
                ThumbLdmReg(instruction, ref address, 128, 7);

                if ((instruction & (1 << reg)) == 0)
                {
                    registers.R[reg] = address;
                }
            }
        }

        public void ConditionalBranch(ushort instruction)
        {
            int flag = (instruction >> 8) & FLAG_MASK_4;
            sbyte offset = (sbyte)(instruction & 0xFF);

            switch (flag)
            {
                case 0b0000:
                    // BEQ label
                    if (registers.C)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0001:
                    // BNE label
                    if (!registers.Z)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0010:
                    // BCS label
                    if (registers.C)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0011:
                    // BCC label
                    if (!registers.C)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0100:
                    // BMI label
                    if (registers.N)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0101:
                    // BPL label
                    if (!registers.N)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0110:
                    // BVS label
                    if (registers.V)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b0111:
                    // BVC label
                    if (!registers.V)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1000:
                    // BHI label
                    if (registers.C && !registers.Z)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1001:
                    // BLS label
                    if (!registers.C || registers.Z)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1010:
                    // BGE label
                    if (registers.N == registers.V)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1011:
                    // BLT label
                    if (registers.N != registers.V)
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1100:
                    // BGT label
                    if (!registers.Z && (registers.N == registers.V))
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                case 0b1101:
                    // BLE label
                    if (registers.Z || (registers.N != registers.V))
                    {
                        registers.R[15] += (uint)(offset << 1);
                        registers.PC = registers.R[15];
                        registers.R[15] += 2;
                        Prefetch();
                    }
                    break;
                default:
                    throw new NotSupportedException("The operation is unsupported! | CONDITION BRANCH, " + instruction);
            }
        }

        public void UnconditionalBranch(ushort instruction)
        {
            int offset = (instruction & 0x3FF) << 1;
            if ((instruction & 0x0400) != 0)
            {
                offset |= unchecked((int)0xFFFFF800);
            }

            registers.R[15] += (uint)offset;
            registers.PC = registers.R[15];
            registers.R[15] += 2;

            Prefetch();
        }

        public void LongBranchWithLink(uint instruction)
        {
            uint offset = (instruction & 0x7FF);

            // Backward or forward
            if ((offset & 0xF000) != 0)
            {
                registers.R[14] = registers.R[15] + (offset << 12);
            }
            else
            {
                registers.R[14] = registers.R[15] + ((offset << 12) | 0xFF800000);
            }

            // Grab the other part of the instruction
            instruction = instruction >> 16;

            offset = (instruction & 0x7FF);
            uint temp = registers.R[15] - 2;
            registers.R[15] = (registers.R[14] + (offset << 1)) & 0xFFFFFFFE;
            registers.PC = registers.R[15];
            registers.R[15] += 2;
            registers.R[14] = temp | 1;

            Prefetch();
        }
        #endregion
    }
}
