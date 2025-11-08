
using System;
using SDL2;
using System.IO;
using System.Text;
using System.Reflection;

namespace SDLTetris
{
    class StandByMode : IGameMode
    {
        public StandByMode(Game g)
        {
            game = g;
        }

        public override bool ProcessEvent(ref SDL.SDL_Event e)
        {

            if (game is not Game g) return false;

            bool quit = false;

            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    quit = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat == 0)
                    {
                        switch (e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                quit = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                g.SetPlayMode();
                                g.NewTetromino();
                                break;
                        }
                    }
                    break;
            }
            return quit;

        }

        public override void Draw()
        {

            if (game is not Game g) return;

            var fg = new SDL.SDL_Color
            {
                r = 255,
                g = 255,
                b = 0,
                a = 255
            };

            var yLine = Globals.TOP + (Globals.NB_ROWS / 4) * Globals.cellSize;
            var txtScore = "Tetris powered by SDL2";
            var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(g.tt_font, txtScore, fg);
            if (surfScore != IntPtr.Zero)
            {

                var textureSCore = SDL.SDL_CreateTextureFromSurface(g.renderer, surfScore);
                if (textureSCore != IntPtr.Zero)
                {
                    uint format;
                    int access, w, h;
                    SDL.SDL_QueryTexture(textureSCore, out format, out access, out w, out h);
                    var desRect = new SDL.SDL_Rect
                    {
                        x = Globals.LEFT + (Globals.NB_COLUMNS / 2) * Globals.cellSize - w / 2,
                        y = yLine,
                        w = w,
                        h = h
                    };
                    SDL.SDL_RenderCopy(g.renderer, textureSCore, IntPtr.Zero, ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                    yLine += 2 * h + 4;
                }

                SDL.SDL_free(surfScore);
            }

            txtScore = "Press SPACE to Start";
            surfScore = SDL_ttf.TTF_RenderUTF8_Blended(g.tt_font, txtScore, fg);
            if (surfScore != IntPtr.Zero)
            {

                var textureSCore = SDL.SDL_CreateTextureFromSurface(g.renderer, surfScore);
                if (textureSCore != IntPtr.Zero)
                {
                    uint format;
                    int access, w, h;
                    SDL.SDL_QueryTexture(textureSCore, out format, out access, out w, out h);

                    SDL.SDL_Rect desRect;
                    desRect.x = Globals.LEFT + (Globals.NB_COLUMNS / 2) * Globals.cellSize - w / 2;
                    desRect.y = yLine;
                    desRect.w = w;
                    desRect.h = h;
                    SDL.SDL_RenderCopy(g.renderer, textureSCore, IntPtr.Zero, ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                }

                SDL.SDL_free(surfScore);
            }

        }

    }
    
}
