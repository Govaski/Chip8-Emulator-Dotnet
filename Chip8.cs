using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace Emulator {
    class Chip8{
        byte[] font = {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        //0x0..0x200 - font
        byte[] memory = new byte[4096];
        Stack<short> stack = new Stack<short>();
        public byte[] display = new byte[64*32];
        byte[] register = new byte[16]; //V0..VF 
        public bool[] keys = new bool[16];
        short I = 0; //12 bit addres register
        public byte soundTimer = 0; //60hz
        public byte delayTimer = 0; //60hz
        public int clockSpeed = 700; //1Mhz
        short pc = 0x200;
        //TODO: INPUT STUFF\\

        //opcodes
        void OP0NNN (){
            //System.Console.WriteLine("TODO: 0NNN");
        }
        void OP00E0 (){
            for (int i = 0; i < 64*32; i++) {
                display[i] = 0;
            }
        }
        void OP00EE (){
            pc = stack.Peek();
            stack.Pop();
        }
        void OP1NNN (short nnn){
            pc = nnn;
        }
        void OP2NNN (short nnn){
            stack.Push(pc);
            pc = nnn;
        }
        void OP3XNN (byte x, byte nn){
            if (register[x] == nn)
                pc += 2;
        }
        void OP4XNN (byte x, byte nn){
            if (register[x] != nn)
                pc += 2;
        }
        void OP5XY0 (byte x, byte y){
            if (register[x] == register[y]){
                pc += 2;
            }
        }
        void OP6XNN (byte x, byte nn){
            register[x] = nn;
        }
        void OP7XNN (byte x, byte nn){
            register[x] += nn;
        }
        void OP8XY0 (byte x, byte y){
            register[x] = register[y];
        }
        void OP8XY1 (byte x, byte y){
            register[x] |= register[y];
        }
        void OP8XY2 (byte x, byte y){
            register[x] &= register[y];
        }
       void OP8XY3 (byte x, byte y){
            register[x] ^= register[y];
        }
        void OP8XY4 (byte x, byte y){
            int oldX = register[x];
            int oldY = register[y];
            register[x] += register[y];
            if ((oldX + oldY) > 255) {
                register[0xF] = 1;
            } else {
                register[0xF] = 0;
            }    
        }
        void OP8XY5 (byte x, byte y){
            int oldX = register[x];
            int oldY = register[y];
            register[x] -= register[y]; 
            if (oldX - oldY < 0) {
                register[0xF] = 0;
            } else {
                register[0xF] = 1;
            }     
        }
        void OP8XY6 (byte x, byte y){
            register[0xF] = (byte)(register[x] & 0b00000001);
            register[x] >>= 1;
        }
        void OP8XY7 (byte x, byte y){
            register[x] = (byte) (register[y] - register[x]);
            if (register[y] < register[x]) {
                register[0xF] = 0;
            } else {
                register[0xF] = 1;
            }
        }
        void OP8XYE (byte x, byte y){
            register[0xF] = (byte)(register[x] & 0b10000000);
            register[x] <<= 1;
        }
        void OP9XY0 (byte x, byte y){
            if (register[x] != register[y]){
                pc += 2;
            }
        }
        void OPANNN (short nnn){
            I = nnn;
        }
        void OPBNNN (short nnn){
            pc = (short) (register[0] + nnn);
        }
        void OPCXNN (byte x, byte nn){
            register[x] = (byte) (new Random().Next() & nn);
            System.Console.WriteLine("VERIFY CXNN - RNG: {0}", register[x]);
        }
        void OPDXYN (byte x, byte y, byte n){
            register[0xF] = 0;

            byte px = (byte) (register[x] %64);
            byte py = (byte) (register[y] %32);
            
            for(int rows = 0; rows < n; rows++)
            {
                byte pixel = memory[I + rows];
                for(int cols = 0; cols < 8; cols++)
                {
                    if((pixel & (0x80 >> cols)) != 0)
                    {
                        if(display[px+cols+((py+rows)*64)] == 1) {
                            register[0xF] = 1;
                        }
                        display[px + cols + ((py + rows) * 64)] ^= 1;
                    }
                }
            }
            //System.Console.WriteLine("Drawing...!");
        }
        void OPEX9E (byte x){
            if (keys[register[x]] == true) {
                pc += 2;
            }

        }
        void OPEXA1 (byte x){
            if(!keys[register[x]]){
                pc+=2;
            }
        }
        void OPFX07 (byte x){
            register[x] = delayTimer;
        }
        void OPFX0A (byte x){
                for (byte j = 0; j < 16; j++){
                    if (keys[j] == true){
                        register[x] = j;
                        return;
                    }
                }
                pc -=2;
        }
        void OPFX15 (byte x){
            delayTimer = register[x];
        }
        void OPFX18 (byte x){
            soundTimer = register[x];
        }
        void OPFX1E (byte x){
            I += register[x];
        }
        void OPFX29 (byte x){
            System.Console.WriteLine("VERIFY FX29");
            I = (short) (register[x]*5);
        }
        void OPFX33 (byte x){
            byte num = register[x];
            memory[I] = (byte)(num/100);
            memory[I+1] = (byte)((num%100)/10);
            memory[I+2] = (byte)(num%10);
        }
        void OPFX55 (byte x){
            for (int j = 0; j <= x; j++) {
                memory[I+j] = register[j];
            }
        }
        void OPFX65 (byte x){
            for (int j = 0; j <= x; j++){
                register[j] = memory[I+j];
            }
        }

        void fetch (){
            byte inst1, inst2;
            inst1 = memory[pc];
            inst2 = memory[pc+1];
            pc+=2;
            DecodeAndExe (inst1, inst2);
        }
        void DecodeAndExe (byte inst1, byte inst2){
            short inst = (short) (inst1 << 8 | inst2);
            byte x = (byte) (inst1 & 0x0F);
            byte y = (byte) ((inst2 & 0xF0) >> 4);
            byte n = (byte) (inst2 & 0x0F);
            byte nn = inst2;
            short nnn = (short) (x<<8|nn);        
            
            //System.Console.WriteLine("{0:X}", inst);

            switch ((byte) (inst1 & 0xF0)>>4) {
                case 0x0:
                    if (inst == 0x00E0)
                        OP00E0();
                    else if (inst == 0x00EE)
                        OP00EE();
                    else 
                        OP0NNN();
                break;
                case 0x1:
                    OP1NNN(nnn);
                break;
                case 0x2:
                    OP2NNN(nnn);
                break;
                case 0x3:
                    OP3XNN(x, nn);
                break;
                case 0x4:
                    OP4XNN(x, nn);
                break;
                case 0x5:
                    OP5XY0(x, y);
                break;
                case 0x6:
                    OP6XNN(x, nn);
                break;
                case 0x7:
                    OP7XNN(x, nn);
                break;
                case 0x8:
                    if (n == 0)
                        OP8XY0(x, y); 
                    else if(n == 1)
                        OP8XY1(x, y);
                    else if (n == 2)
                        OP8XY2(x, y);
                    else if(n == 3)
                        OP8XY3(x, y);
                    else if (n == 4)
                        OP8XY4(x, y);
                    else if(n == 5)
                        OP8XY5(x, y);
                    else if (n == 6)
                        OP8XY6(x, y);
                    else if(n == 7)
                        OP8XY7(x, y);
                    else if (n == 0xE)
                        OP8XYE(x, y);
                    else
                        System.Console.WriteLine("Uknown OP 8XY_?: {0:x}", inst);

                break;
                case 0x9:
                    OP9XY0(x, y);
                break;
                case 0xA:
                    OPANNN(nnn);
                break;
                case 0xB:
                    OPBNNN(nnn);
                break;
                case 0xC:
                    OPCXNN(x, nn);
                break;
                case 0xD:
                    OPDXYN(x, y, n);
                break;
                case 0xE:
                    if (nn == 0x9E) {
                        OPEX9E(x);
                    } else if (nn == 0xA1) {
                        OPEXA1(x);
                    } else {
                        System.Console.WriteLine("Unkown {0}", inst);
                    }
                break;
                case 0xF:
                    if (nn == 0x33) {
                        OPFX33(x);
                    }
                    else if (nn == 0x55) {
                        OPFX55(x);
                    } else if (nn == 0x1E) {
                        OPFX1E(x);
                    } else if (nn == 0x15){
                        OPFX15(x);
                    } else if (nn == 0x18){
                        OPFX18(x);
                    } else if (nn == 0x29){
                        OPFX29(x);
                    } else if (nn == 0x65){
                        OPFX65(x);
                    } else if (nn == 0x07) {
                        OPFX07(x);
                    } else if (nn == 0x0A){
                        OPFX0A(x);
                    } else {
                        System.Console.WriteLine("Unkown FX__: {0:x}", inst);
                    }
                break;

            }    
            
        }

        public void LoadRom(string romPath){
            byte[] file = File.ReadAllBytes(romPath);
            file.CopyTo(memory, 0x200);
            font.CopyTo(memory, 0x000);
        }

        public void step (){
            for (int j = 0; j < clockSpeed/60; j++){
                fetch(); 
            }
        }

    }
}



