using Emulator;
using Raylib_cs;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0){
            Console.WriteLine("Usage:");
            Console.WriteLine("Run with:\n  dotnet run <rom> <clock>\n");
            Console.WriteLine("The default clock speed is 700, which is equal to 1Mhz.\nThe minimum clock speed is 60, which is equal to 1hz");
            return;
        }

        int scale = 10;

        Chip8 emu = new Chip8();
        emu.LoadRom(args[0]);

        if (args.Length > 1){
            emu.clockSpeed =  int.Parse(args[1]);
        }

        Raylib.InitWindow(64 * scale, 32 * scale, "Chip8");
        Raylib.InitAudioDevice();
        Raylib.SetTargetFPS(60);
        Sound beepSound = Raylib.LoadSound("beeep.ogg");

        System.Console.WriteLine("===========================================================");
        
        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyDown(KeyboardKey.One)) emu.keys[0x1] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.Two)) emu.keys[0x2] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.Three)) emu.keys[0x3] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.Four)) emu.keys[0xC] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.Q)) emu.keys[0x4] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.W)) emu.keys[0x5] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.E)) emu.keys[0x6] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.R)) emu.keys[0xD] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.A)) emu.keys[0x7] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.S)) emu.keys[0x8] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.D)) emu.keys[0x9] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.F)) emu.keys[0xE] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.Z)) emu.keys[0xA] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.X)) emu.keys[0x0] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.C)) emu.keys[0xB] = true; 
            if (Raylib.IsKeyDown(KeyboardKey.V)) emu.keys[0xF] = true; 

            if (Raylib.IsKeyUp(KeyboardKey.One)) emu.keys[0x1] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.Two)) emu.keys[0x2] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.Three)) emu.keys[0x3] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.Four)) emu.keys[0xC] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.Q)) emu.keys[0x4] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.W)) emu.keys[0x5] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.E)) emu.keys[0x6] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.R)) emu.keys[0xD] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.A)) emu.keys[0x7] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.S)) emu.keys[0x8] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.D)) emu.keys[0x9] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.F)) emu.keys[0xE] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.Z)) emu.keys[0xA] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.X)) emu.keys[0x0] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.C)) emu.keys[0xB] = false; 
            if (Raylib.IsKeyUp(KeyboardKey.V)) emu.keys[0xF] = false; 

            if (emu.delayTimer > 0) {
                emu.delayTimer -= 1;
            } 
            if (emu.soundTimer > 0) {
                emu.soundTimer -= 1;
                System.Console.WriteLine("BEEP! :D");
                Raylib.PlaySound(beepSound);
            }
            
            emu.step();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            if (emu.display != null)
            {
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        if (emu.display[y * 64 + x] > 0)
                        {
                            Raylib.DrawRectangle(x * scale, y * scale, 1 * scale, 1 * scale, Color.Green);
                        }
                    }
                }
                Raylib.EndDrawing();
            }
        }
        Raylib.CloseWindow();
    }

}