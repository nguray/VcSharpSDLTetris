
using System;
using SDL2;

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

        public static Tetromino? curTetromino;
        public static Tetromino? nextTetromino;

        public static Random rand;

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

    class Program
    {


       const string TITLE = "C# SDL2 Tetris";


        static void Main(string[] args)
        {

			SDL.SDL_Init(SDL.SDL_INIT_VIDEO);


            var window = IntPtr.Zero;
            

            window = SDL.SDL_CreateWindow(TITLE,SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED,Globals.WIN_WIDTH,Globals.WIN_HEIGHT,SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);


            var renderer = SDL.SDL_CreateRenderer(window,-1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED|SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
			

            DateTime randSeed = DateTime.Now;
            Globals.rand = new Random(randSeed.Millisecond);

            Globals.curTetromino = new Tetromino(Globals.rand.Next(1,8),6*Globals.cellSize+Globals.LEFT,3*Globals.cellSize+Globals.TOP);


            UInt64 startTime = SDL.SDL_GetTicks64();
            UInt64 curTime = 0;

            SDL.SDL_Event e;
            SDL.SDL_Rect rect;

            bool quit = false;
            while(!quit)
            {

                SDL.SDL_SetRenderDrawColor(renderer,48,48,255,255);
                SDL.SDL_RenderClear(renderer);

                rect.x = Globals.LEFT;
                rect.y = Globals.TOP;
                rect.w = Globals.cellSize*Globals.NB_COLUMNS;
                rect.h = Globals.cellSize*Globals.NB_ROWS;
                SDL.SDL_SetRenderDrawColor(renderer,10,10,100,255);
                SDL.SDL_RenderFillRect(renderer,ref rect);



                while (SDL.SDL_PollEvent(out e)!=0)
                {
                    switch(e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (e.key.repeat==0){
                                switch (e.key.keysym.sym){
                                    case SDL.SDL_Keycode.SDLK_ESCAPE:
                                        quit = true;
                                        break;
                                    case SDL.SDL_Keycode.SDLK_LEFT:
                                    
                                        break;
                                    case SDL.SDL_Keycode.SDLK_RIGHT:
                                    
                                        break;
                                    case SDL.SDL_Keycode.SDLK_UP:

                                        break;

                                }
                            }
                            break;

                    }
                }

                curTime = SDL.SDL_GetTicks64();
                if ((curTime-startTime)>30){
                    startTime = curTime;
                    if (Globals.curTetromino!=null){
                        Globals.curTetromino.y += 5; 
                    }

                }



                //--
                if (Globals.curTetromino!=null){
                    Globals.curTetromino.Draw(renderer);
                }
                
                if (Globals.nextTetromino!=null){
                    Globals.nextTetromino.Draw(renderer);
                }
                
                //--
                SDL.SDL_RenderPresent(renderer);

            }

            SDL.SDL_DestroyWindow(window);

			SDL.SDL_Quit();
            

        }
	
	}
}