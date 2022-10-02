
using System;
using SDL2;

namespace SfmlTetris
{
    class Program
    {
        static void Main(string[] args)
        {

			SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            var window = IntPtr.Zero;

            window = SDL.SDL_CreateWindow("SDL2 Tetris",SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED,800,600,SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
			
            SDL.SDL_Event e;

            bool quit = false;
            while(!quit)
            {
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
            }

            SDL.SDL_DestroyWindow(window);

			SDL.SDL_Quit();
            

        }
	
	}
}