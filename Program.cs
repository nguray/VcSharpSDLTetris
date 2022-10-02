
using System;
using SDL2;

namespace SDLTetris
{

    class Vector2i{
        public int x;
        public int y;
        public Vector2i(int x,int y){
            this.x = x;
            this.y = y;
        }
    }

    class Color{
        public int red;
        public int green;
        public int blue;
        public Color(int r,int g,int b){
            red = r;
            green = g;
            blue = b;
        }
    }

    class Program
    {
        const int WIDTH = 480;
        const int HEIGHT = 640;
        const string TITLE = "C# SDL2 Tetris";

        const int NB_ROWS = 20;
        const int NB_COLUMNS = 16;


        Tetromino? curTetromino;
        Tetromino? nextTetromino;

        static void Main(string[] args)
        {

			SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            var window = IntPtr.Zero;

            window = SDL.SDL_CreateWindow(TITLE,SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED,WIDTH,HEIGHT,SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);


            var renderer = SDL.SDL_CreateRenderer(window,-1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED|SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
			
            SDL.SDL_Event e;

            bool quit = false;
            while(!quit)
            {

                SDL.SDL_SetRenderDrawColor(renderer,48,48,255,255);
                SDL.SDL_RenderClear(renderer);

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

                //--
                SDL.SDL_RenderPresent(renderer);

            }

            SDL.SDL_DestroyWindow(window);

			SDL.SDL_Quit();
            

        }
	
	}
}