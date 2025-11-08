using System;
using SDL2;
using System.IO;
using System.Text;
using System.Reflection;

namespace SDLTetris
{
    class HighSCoresMode : IGameMode
    {

        ulong startTimeH = 0;

        Dictionary<SDL.SDL_Keycode, char> keys = new Dictionary<SDL.SDL_Keycode, char>{
            {SDL.SDL_Keycode.SDLK_a,'A'},
            {SDL.SDL_Keycode.SDLK_b,'B'},
            {SDL.SDL_Keycode.SDLK_c,'C'},
            {SDL.SDL_Keycode.SDLK_d,'D'},
            {SDL.SDL_Keycode.SDLK_e,'E'},
            {SDL.SDL_Keycode.SDLK_f,'F'},
            {SDL.SDL_Keycode.SDLK_i,'I'},
            {SDL.SDL_Keycode.SDLK_j,'J'},
            {SDL.SDL_Keycode.SDLK_k,'K'},
            {SDL.SDL_Keycode.SDLK_l,'L'},
            {SDL.SDL_Keycode.SDLK_m,'M'},
            {SDL.SDL_Keycode.SDLK_n,'N'},
            {SDL.SDL_Keycode.SDLK_o,'O'},
            {SDL.SDL_Keycode.SDLK_p,'P'},
            {SDL.SDL_Keycode.SDLK_q,'Q'},
            {SDL.SDL_Keycode.SDLK_r,'R'},
            {SDL.SDL_Keycode.SDLK_s,'S'},
            {SDL.SDL_Keycode.SDLK_t,'T'},
            {SDL.SDL_Keycode.SDLK_u,'U'},
            {SDL.SDL_Keycode.SDLK_v,'V'},
            {SDL.SDL_Keycode.SDLK_w,'W'},
            {SDL.SDL_Keycode.SDLK_x,'X'},
            {SDL.SDL_Keycode.SDLK_y,'Y'},
            {SDL.SDL_Keycode.SDLK_z,'Z'},
            {SDL.SDL_Keycode.SDLK_KP_0,'0'},
            {SDL.SDL_Keycode.SDLK_KP_1,'1'},
            {SDL.SDL_Keycode.SDLK_KP_2,'2'},
            {SDL.SDL_Keycode.SDLK_KP_3,'3'},
            {SDL.SDL_Keycode.SDLK_KP_4,'4'},
            {SDL.SDL_Keycode.SDLK_KP_5,'5'},
            {SDL.SDL_Keycode.SDLK_KP_6,'6'},
            {SDL.SDL_Keycode.SDLK_KP_7,'7'},
            {SDL.SDL_Keycode.SDLK_KP_8,'8'},
            {SDL.SDL_Keycode.SDLK_KP_9,'9'},
            {SDL.SDL_Keycode.SDLK_0,'0'},
            {SDL.SDL_Keycode.SDLK_1,'1'},
            {SDL.SDL_Keycode.SDLK_2,'2'},
            {SDL.SDL_Keycode.SDLK_3,'3'},
            {SDL.SDL_Keycode.SDLK_4,'4'},
            {SDL.SDL_Keycode.SDLK_5,'5'},
            {SDL.SDL_Keycode.SDLK_6,'6'},
            {SDL.SDL_Keycode.SDLK_7,'7'},
            {SDL.SDL_Keycode.SDLK_8,'8'},
            {SDL.SDL_Keycode.SDLK_9,'9'}
        };

        public HighSCoresMode(Game g)
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
                            case SDL.SDL_Keycode.SDLK_BACKSPACE:
                                if (g.playerName.Length == 1)
                                {
                                    g.playerName = "";
                                }
                                else if (g.playerName.Length > 1)
                                {
                                    g.playerName = g.playerName.Substring(0, g.playerName.Length - 1);
                                }
                                g.highScores[g.idHighScore].Name = g.playerName;
                                break;
                            case SDL.SDL_Keycode.SDLK_RETURN or SDL.SDL_Keycode.SDLK_KP_ENTER:
                                if (g.playerName.Length == 0)
                                {
                                    g.playerName = "XXXXXX";
                                }
                                g.highScores[g.idHighScore].Name = g.playerName;
                                g.saveHighScores();
                                g.SetStandByMode();
                                break;
                            default:
                                char c;
                                if (keys.TryGetValue(e.key.keysym.sym, out c))
                                {
                                    if (g.playerName.Length < 8)
                                    {
                                        g.playerName += c;
                                    }
                                    g.highScores[g.idHighScore].Name = g.playerName;
                                    //Console.WriteLine(userName);
                                }
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
            SDL.SDL_Color fg;
            fg = new SDL.SDL_Color
            {
                r = 255,
                g = 255,
                b = 0,
                a = 255
            };
            var yLine = Globals.TOP + Globals.cellSize;
            var txtTitle = "HIGH SCORES";
            var surfTitle = SDL_ttf.TTF_RenderUTF8_Blended(g.tt_font, txtTitle, fg);
            if (surfTitle != IntPtr.Zero)
            {

                var textureSCore = SDL.SDL_CreateTextureFromSurface(g.renderer, surfTitle);
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
                    yLine += 3 * h;
                }

                SDL.SDL_free(surfTitle);
            }

            var xCol0 = Globals.LEFT + Globals.cellSize;
            var xCol1 = Globals.LEFT + (Globals.NB_COLUMNS / 2 + 2) * Globals.cellSize;
            for (int i = 0; i < g.highScores.Count; i++)
            {
                int access, width, heigh = 0;
                var h = g.highScores[i];

                if (i == g.idHighScore)
                {
                    if ((g.iColorHighScore % 2) > 0)
                    {
                        fg = new SDL.SDL_Color
                        {
                            r = 255,
                            g = 255,
                            b = 0,
                            a = 255
                        };
                    }
                    else
                    {
                        fg = new SDL.SDL_Color
                        {
                            r = 155,
                            g = 155,
                            b = 0,
                            a = 255
                        };
                    }
                }
                else
                {
                    fg = new SDL.SDL_Color
                    {
                        r = 255,
                        g = 255,
                        b = 0,
                        a = 255
                    };
                }

                var surfName = SDL_ttf.TTF_RenderUTF8_Blended(g.tt_font, h.Name, fg);
                if (surfName != IntPtr.Zero)
                {
                    var textureName = SDL.SDL_CreateTextureFromSurface(g.renderer, surfName);
                    if (textureName != IntPtr.Zero)
                    {
                        uint format;
                        SDL.SDL_QueryTexture(textureName, out format, out access, out width, out heigh);

                        SDL.SDL_Rect desRect;
                        desRect.x = xCol0;
                        desRect.y = yLine;
                        desRect.w = width;
                        desRect.h = heigh;
                        SDL.SDL_RenderCopy(g.renderer, textureName, IntPtr.Zero, ref desRect);
                        SDL.SDL_DestroyTexture(textureName);
                    }

                    SDL.SDL_free(surfName);

                }

                var txtScore = String.Format("{0:00000}", h.Score);
                var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(g.tt_font, txtScore, fg);
                if (surfScore != IntPtr.Zero)
                {
                    var textureScore = SDL.SDL_CreateTextureFromSurface(g.renderer, surfScore);
                    if (textureScore != IntPtr.Zero)
                    {
                        uint format;
                        SDL.SDL_QueryTexture(textureScore, out format, out access, out width, out heigh);

                        SDL.SDL_Rect desRect;
                        desRect.x = xCol1;
                        desRect.y = yLine;
                        desRect.w = width;
                        desRect.h = heigh;
                        SDL.SDL_RenderCopy(g.renderer, textureScore, IntPtr.Zero, ref desRect);
                        SDL.SDL_DestroyTexture(textureScore);
                    }

                    SDL.SDL_free(surfScore);

                }

                yLine += heigh + 8;

            }


        }

        public override void init()
        {
            startTimeH = 0;
        }

        public override void Update()
        {
            if (game is not Game g) return;
            ulong curTime = SDL.SDL_GetTicks64();
            if ((curTime - startTimeH) > 200)
            {
                startTimeH = curTime;
                g.iColorHighScore++;
            }

        }

    }    
}
