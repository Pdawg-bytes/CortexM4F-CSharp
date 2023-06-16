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

        public void InitMemory()
        {
            memoryObj.memory[0] = (byte)'H';
            memoryObj.memory[1] = (byte)'e';
            memoryObj.memory[2] = (byte)'l';
            memoryObj.memory[3] = (byte)'l';

            memoryObj.memory[4] = (byte)'l';
            memoryObj.memory[5] = (byte)' ';
            memoryObj.memory[6] = (byte)'W';
            memoryObj.memory[7] = (byte)'o';

            memoryObj.memory[8] = (byte)'r';
            memoryObj.memory[9] = (byte)'l';
            memoryObj.memory[10] = (byte)'d';
            memoryObj.memory[11] = (byte)'!';

            memoryObj.memory[12] = MOV;
            memoryObj.memory[13] = 0;
            memoryObj.memory[14] = memoryObj.memory[0];
            memoryObj.memory[15] = 0;

            memoryObj.memory[16] = MOV;
            memoryObj.memory[17] = 0;
            memoryObj.memory[18] = memoryObj.memory[1];
            memoryObj.memory[19] = 0;

            memoryObj.memory[20] = MOV;
            memoryObj.memory[21] = 0;
            memoryObj.memory[22] = memoryObj.memory[2];
            memoryObj.memory[23] = 0;

            memoryObj.memory[24] = MOV;
            memoryObj.memory[25] = 0;
            memoryObj.memory[26] = memoryObj.memory[3];
            memoryObj.memory[27] = 0;

            memoryObj.memory[28] = MOV;
            memoryObj.memory[29] = 0;
            memoryObj.memory[30] = memoryObj.memory[4];
            memoryObj.memory[31] = 0;

            memoryObj.memory[32] = MOV;
            memoryObj.memory[33] = 0;
            memoryObj.memory[34] = memoryObj.memory[5];
            memoryObj.memory[35] = 0;

            memoryObj.memory[36] = MOV;
            memoryObj.memory[37] = 0;
            memoryObj.memory[38] = memoryObj.memory[6];
            memoryObj.memory[39] = 0;

            memoryObj.memory[40] = MOV;
            memoryObj.memory[41] = 0;
            memoryObj.memory[42] = memoryObj.memory[7];
            memoryObj.memory[43] = 0;

            memoryObj.memory[44] = MOV;
            memoryObj.memory[45] = 0;
            memoryObj.memory[46] = memoryObj.memory[8];
            memoryObj.memory[47] = 0;

            memoryObj.memory[48] = MOV;
            memoryObj.memory[49] = 0;
            memoryObj.memory[50] = memoryObj.memory[9];
            memoryObj.memory[51] = 0;

            memoryObj.memory[52] = MOV;
            memoryObj.memory[53] = 0;
            memoryObj.memory[54] = memoryObj.memory[10];
            memoryObj.memory[55] = 0;

            memoryObj.memory[56] = MOV;
            memoryObj.memory[57] = 0;
            memoryObj.memory[58] = memoryObj.memory[11];
            memoryObj.memory[59] = 0;
        }

        public void Run()
        {
            InitMemory();
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
            uint rd = buffer[1]; // Destination Register
            uint rn = buffer[2]; // Source register #1
            uint rm = buffer[3]; // Source register #2

            switch (opcode)
            {
                case ADD:
                    Add(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[!] ADD | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case SUB:
                    Subtract(registers, rd, registers.R[rn], registers.R[rm]);
                    Debug.WriteLine("[!] SUB | " + "(Param 1: " + registers.R[rn] + ", Register " + rn + "), " + "(Param 2: " + registers.R[rm] + ", Register " + rm + ")" + " | Dest. Register Val: " + registers.R[rd]);
                    break;
                case MOV:
                    Move(registers, rd, rn);
                    Debug.WriteLine("[!] MOV | " + "To register: " + rd + ", From value: " + rn + " | Dest. Register Val: " + (char)registers.R[rd]);
                    break;
                default:
                    Debug.WriteLine("[*] Unknown opcode: " + opcode);
                    break;
            }
        }
    }
}
