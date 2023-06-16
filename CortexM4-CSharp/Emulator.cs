using System;
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

        public void Run()
        {
            memoryObj.memory[0] = MOV;
            memoryObj.memory[1] = 1;
            memoryObj.memory[2] = 124;
            memoryObj.memory[3] = 0;

            memoryObj.memory[4] = MOV;
            memoryObj.memory[5] = 2;
            memoryObj.memory[6] = 12;
            memoryObj.memory[7] = 0;

            memoryObj.memory[8] = ADD;
            memoryObj.memory[9] = 0;
            memoryObj.memory[10] = 1;
            memoryObj.memory[11] = 2;

            memoryObj.memory[12] = SUB;
            memoryObj.memory[13] = 0;
            memoryObj.memory[14] = 1;
            memoryObj.memory[15] = 2;

            while (registers.PC <= 508)
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
            uint rd = buffer[1]; // Destination Register
            uint rn = buffer[2]; // Source register #1
            uint rm = buffer[3]; // Source register #2

            switch (opcode)
            {
                case ADD:
                    Add(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[!] ADD new value | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case SUB:
                    Subtract(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[!] SUB new value | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case MOV:
                    Move(registers, rd, rn);
                    Debug.WriteLine("[!] MOV new value | " + "To register: " + rd + ", From value: " + rn + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                default:
                    Debug.WriteLine("[*] Unknown opcode: " + opcode);
                    break;
            }
        }
    }
}
