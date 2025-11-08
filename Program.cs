/*--------------------------------------------------------------*\
	Simple Tetris using sdl2 in C#
                
    Raymong NGUYEN THANH
	2022-10-06                            First version
    2025-11-08                            Refactoring with OOP 
\*--------------------------------------------------------------*/

using System;
using SDL2;
using System.IO;
using System.Text;
using System.Reflection;


namespace SDLTetris
{

    static class Globals{

        public const int WIN_WIDTH = 480;
        public const int WIN_HEIGHT = 560;
  
        public const int NB_ROWS = 20;
        public const int NB_COLUMNS = 12;

        public const int LEFT = 10;
        public const int TOP = 10;

        public static int cellSize = Globals.WIN_WIDTH / (Globals.NB_COLUMNS + 7);

        public static Random? rand;

    }


    class Vector2i{
        public int x;
        public int y;
        public Vector2i(int x,int y){
            this.x = x;
            this.y = y;
        }
    }

    class Color{
        public byte red;
        public byte green;
        public byte blue;
        public Color(byte r,byte g,byte b){
            red = r;
            green = g;
            blue = b;
        }
    }

    class HighScore
    {

        public string Name { get; set; }
        public int Score { get; set; }

        public HighScore(string name, int score)
        {
            Name = name;
            Score = score;
        }

    }

    abstract class IGameMode
    {
        public Game? game;

        public abstract bool ProcessEvent(ref SDL.SDL_Event e);
        
        public virtual void Draw() { }

        public virtual void Update() { }

        public virtual void init() { }
        
    }


    
    class Program
    {

        const string TITLE = "C# SDL2 Tetris";


        static void Main(string[] args)
        {


            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) < 0)
            {
                Console.WriteLine($"There was an issue initializing SDL. {SDL.SDL_GetError()}");
            }

            SDL_ttf.TTF_Init();


            var myGame = new Game();

            var names =
                System
                .Reflection
                .Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames();

            foreach (var name in names)
            {
                Console.WriteLine(name);
            }


            var window = SDL.SDL_CreateWindow(TITLE, SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED, Globals.WIN_WIDTH, Globals.WIN_HEIGHT, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);


            myGame.renderer = SDL.SDL_CreateRenderer(window, -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);


            DateTime randSeed = DateTime.Now;
            Globals.rand = new Random(randSeed.Millisecond);


            SDL.SDL_Event e;

            bool fStopGame = false;

            while (!fStopGame)
            {



                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    fStopGame = myGame.curMode.ProcessEvent(ref e);

                }

                myGame.Draw();

                myGame.curMode.Update();


                myGame.curMode.Draw();


                //--
                SDL.SDL_RenderPresent(myGame.renderer);

            }

            SDL.SDL_DestroyWindow(window);

            myGame.Dispose();

            SDL_ttf.TTF_Quit();

            SDL_mixer.Mix_Quit();

            SDL.SDL_Quit();


        }

    }
}