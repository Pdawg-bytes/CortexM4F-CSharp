using System;
using System.Collections;
using System.Diagnostics;
using static CortexM4_CSharp.Instructions;
using static CortexM4_CSharp.OpCodes;

namespace CortexM4_CSharp
{
    internal class Emulator
    {
        private Memory memoryObj;
        private RegisterSet registers;

        public Emulator()
        {
            memoryObj = new Memory();
            registers = new RegisterSet();
        }

        public void InitMemory()
        {
            memoryObj.memory[0] = memoryObj.Reg32ToReg8(277)[0];
            memoryObj.memory[1] = memoryObj.Reg32ToReg8(277)[1];
            memoryObj.memory[2] = memoryObj.Reg32ToReg8(277)[2];
            memoryObj.memory[3] = memoryObj.Reg32ToReg8(277)[3];

            memoryObj.memory[4] = MOV;
            memoryObj.memory[5] = 0;
            memoryObj.memory[6] = (byte)memoryObj.Reg8ToReg32(memoryObj.ReadWord(0));
            memoryObj.memory[7] = 0;
        }

        public void Run()
        {
            InitMemory();
            byte[] bytes = memoryObj.Reg32ToReg8(int.MaxValue);
            Debug.WriteLine("[*] 32to8 (Little Endian): " + BitConverter.ToString(bytes));
            Debug.WriteLine("[*] 8to32: " + memoryObj.Reg8ToReg32(bytes));
            
            while (registers.PC <= 60)
            {
                uint instruction = FetchInstruction();
                DecodeAndExecute(registers, instruction);
            }
            Debug.WriteLine("[*] Execution complete.");
        }

        private uint FetchInstruction()
        {
            uint instruction = registers.PC;
            registers.PC += 4;
            return instruction;
        }

        private void DecodeAndExecute(RegisterSet registers, uint instruction)
        {
            byte[] buffer = memoryObj.ReadWord(instruction); // Execution Block

            uint opcode = buffer[0]; // Instruction OpCode
            int rd = buffer[1];     // Destination Register
            int rn = buffer[2];     // Source register #1
            int rm = buffer[3];     // Source register #2

            switch (opcode)
            {
                case ADD:
                    Add(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[+] ADD | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case SUB:
                    Subtract(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[+] SUB | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case MOV:
                    Move(registers, rd, rn);
                    Debug.WriteLine("[+] MOV | " + "To register: " + rd + ", From value: " + rn + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                default:
                    Debug.WriteLine("[!] Unknown opcode: " + opcode);
                    break;
            }
        }
    }
}
