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
            memoryObj.memory[0] = MOV;      // Opcode for MOV immediate instruction (MOV R1, <immediate>)
            memoryObj.memory[1] = 0x01;     // Destination register (R1)
            memoryObj.memory[2] = 0x0A;     // Immediate value (LSB) for 463,126
            memoryObj.memory[3] = 0xE5;     // Immediate value - 2nd byte
            memoryObj.memory[4] = 0x07;     // Immediate value - 3rd byte
            memoryObj.memory[5] = 0x00;     // Immediate value (MSB) - 4th byte

            memoryObj.memory[6] = MOV;      // Opcode for MOV immediate instruction (MOV R2, <immediate>)
            memoryObj.memory[7] = 0x02;     // Destination register (R2)
            memoryObj.memory[8] = 0xC0;     // Immediate value (LSB) for 124,512
            memoryObj.memory[9] = 0x01;     // Immediate value - 2nd byte
            memoryObj.memory[10] = 0x02;    // Immediate value - 3rd byte
            memoryObj.memory[11] = 0x00;    // Immediate value (MSB) - 4th byte

            memoryObj.memory[12] = ADD;     // Opcode for ADD instruction (ADD R0, R1, R2)
            memoryObj.memory[13] = 0x20;    // Destination register (R0)
            memoryObj.memory[14] = 0x01;    // Operand register (R1)
            memoryObj.memory[15] = 0x02;    // Operand register (R
        }

        public void Run()
        {
            InitMemory();
            
            while (registers.PC <= 63)
            {
                uint instruction = FetchInstruction();
                DecodeAndExecute(registers, instruction);
            }
            Debug.WriteLine("[*] Execution complete.");
        }

        private uint FetchInstruction()
        {
            uint instruction = registers.PC;
            registers.PC += 1;
            return instruction;
        }

        private void DecodeAndExecute(RegisterSet registers, uint instruction)
        {
            byte[] buffer = memoryObj.ReadBlock(instruction); // Execution Block

            uint opcode = buffer[0]; // Instruction OpCode
            int rd = buffer[1];      // Destination Register
            int rn = buffer[2];      // Source register #1
            int rm = buffer[3];      // Source register #2

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
