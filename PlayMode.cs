using System;
using SDL2;
using System.IO;
using System.Text;
using System.Reflection;

namespace SDLTetris
{
    class PlayMode : IGameMode
    {

        delegate bool IsOutLimit_t();
        IsOutLimit_t? IsOutLimit;

        ulong startTimeV = 0;
        ulong startTimeH = 0;

        public PlayMode(Game g)
        {
            game = g;
        }

        public override bool ProcessEvent(ref SDL.SDL_Event e)
        {
            if (game is not Game g) return false;
            if (g.curTetromino is not Tetromino curTetro) return false;

            bool fExitGame = false;
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    fExitGame = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat == 0)
                    {
                        switch (e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                g.StopGame();
                                break;
                            case SDL.SDL_Keycode.SDLK_LEFT:
                                g.VelH = -1;
                                IsOutLimit = curTetro.IsOutLeft;
                                break;
                            case SDL.SDL_Keycode.SDLK_RIGHT:
                                g.VelH = 1;
                                IsOutLimit = curTetro.IsOutRight;
                                break;
                            case SDL.SDL_Keycode.SDLK_UP:
                                if (curTetro != null)
                                {
                                    curTetro.RotateLeft();
                                    if (curTetro.HitGround(g.board))
                                    {
                                        //-- Undo Rotate
                                        curTetro.RotateRight();
                                    }
                                    else if (curTetro.IsOutRight())
                                    {
                                        var backupX = curTetro.x;
                                        //-- Move Inside board
                                        while (curTetro.IsOutRight())
                                        {
                                            curTetro.x--;
                                        }
                                        if (curTetro.HitGround(g.board))
                                        {
                                            curTetro.x = backupX;
                                            //-- Undo Rotate
                                            curTetro.RotateRight();

                                        }
                                    }
                                    else if (curTetro.IsOutLeft())
                                    {
                                        var backupX = curTetro.x;
                                        //-- Move Inside Board
                                        while (curTetro.IsOutLeft())
                                        {
                                            curTetro.x++;
                                        }
                                        if (curTetro.HitGround(g.board))
                                        {
                                            curTetro.x = backupX;
                                            //-- Undo Rotate
                                            curTetro.RotateRight();

                                        }

                                    }

                                }
                                break;
                            case SDL.SDL_Keycode.SDLK_DOWN:
                                g.fFastDown = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                g.fDrop = true;
                                break;
                        }
                    }
                    break;
                case SDL.SDL_EventType.SDL_KEYUP:
                    switch (e.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            g.VelH = 0;
                            break;
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            g.VelH = 0;
                            break;
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            g.fFastDown = false;
                            break;
                    }
                    break;
            }
            return fExitGame;

        }

        public override void Draw()
        {
            //--
            if (game is not Game g) return;
            if (g.curTetromino is not Tetromino curTetro) return;

            curTetro.Draw(g.renderer);

        }

        public override void init()
        {
            startTimeH = 0;
            startTimeV = 0;
        }


        public override void Update()
        {
            if (game is not Game g) return;
            if (g.curTetromino is not Tetromino curTetro) return;

            ulong curTime;

            if (g.nbCompletedLines > 0)
            {
                curTime = SDL.SDL_GetTicks64();
                if ((curTime - startTimeV) > 200)
                {
                    startTimeV = curTime;
                    g.nbCompletedLines--;
                    g.EraseFirstCompletedLine();
                    if (g.sound != IntPtr.Zero)
                    {
                        SDL_mixer.Mix_PlayChannel(SDL_mixer.MIX_DEFAULT_CHANNELS, g.sound, 0);
                    }
                }

            }
            else if (g.horizontalMove != 0)
            {

                curTime = SDL.SDL_GetTicks64();

                if ((curTime - startTimeH) > 20)
                {
                    for (int i = 0; i < 5; i++)
                    {

                        var backupX = curTetro.x;
                        curTetro.x += g.horizontalMove;
                        //Console.WriteLine(horizontalMove);
                        if (IsOutLimit())
                        {
                            curTetro.x = backupX;
                            g.horizontalMove = 0;
                            break;
                        }
                        else
                        {
                            if (curTetro.HitGround(g.board))
                            {
                                curTetro.x = backupX;
                                g.horizontalMove = 0;
                                break;
                            }
                        }

                        if (g.horizontalMove != 0)
                        {
                            startTimeH = curTime;
                            if (g.horizontalStartColumn != curTetro.Column())
                            {
                                curTetro.x = backupX;
                                g.horizontalMove = 0;
                                break;
                            }

                        }

                    }

                }

            }
            else if (g.fDrop)
            {

                curTime = SDL.SDL_GetTicks64();
                if ((curTime - startTimeV) > 10)
                {
                    startTimeV = curTime;
                    for (int i = 0; i < 6; i++)
                    {
                        //-- Move down to Check
                        curTetro.y++;
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.y--;
                            g.FreezeCurTetromino();
                            g.NewTetromino();
                            g.fDrop = false;
                            break;
                        }
                        else if (curTetro.IsOutBottom())
                        {
                            curTetro.y--;
                            g.FreezeCurTetromino();
                            g.NewTetromino();
                            g.fDrop = false;
                            break;
                        }
                        if (g.VelH != 0)
                        {
                            if ((curTime - startTimeH) > 15)
                            {
                                var backupX = curTetro.x;
                                curTetro.x += g.VelH;
                                if (IsOutLimit())
                                {
                                    curTetro.x = backupX;
                                }
                                else
                                {
                                    if (curTetro.HitGround(g.board))
                                    {
                                        curTetro.x = backupX;
                                    }
                                    else
                                    {
                                        g.horizontalMove = g.VelH;
                                        g.horizontalStartColumn = curTetro.Column();
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }

            }
            else
            {
                curTime = SDL.SDL_GetTicks64();

                ulong limitElapse;
                if (g.fFastDown)
                {
                    limitElapse = 10;
                }
                else
                {
                    limitElapse = 25;
                }

                if ((curTime - startTimeV) > limitElapse)
                {
                    startTimeV = curTime;

                    for (int i = 0; i < 4; i++)
                    {
                        //-- Move down to check
                        curTetro.y++;
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.y--;
                            g.FreezeCurTetromino();
                            g.NewTetromino();
                            break;
                        }
                        else if (curTetro.IsOutBottom())
                        {
                            curTetro.y--;
                            g.FreezeCurTetromino();
                            g.NewTetromino();
                            break;
                        }

                        if (g.VelH != 0)
                        {
                            if ((curTime - startTimeH) > 15)
                            {

                                var backupX = curTetro.x;
                                curTetro.x += g.VelH;

                                if (IsOutLimit())
                                {
                                    curTetro.x = backupX;
                                }
                                else
                                {
                                    if (curTetro.HitGround(g.board))
                                    {
                                        curTetro.x -= g.VelH;
                                    }
                                    else
                                    {
                                        startTimeH = curTime;
                                        g.horizontalMove = g.VelH;
                                        g.horizontalStartColumn = curTetro.Column();
                                        break;
                                    }
                                }

                            }

                        }

                    }
                }
            }


            //-- Check Game Over
            if (g.isGameOver())
            {
                g.idHighScore = g.IsHighScore(g.curScore);
                if (g.idHighScore >= 0)
                {
                    g.insertHighScore(g.idHighScore, g.playerName, g.curScore);
                    g.SetHighScoresMode();
                    g.InitGame();
                }
                else
                {
                    g.SetGameOverMode();
                }

            }


        }

    }    
}
