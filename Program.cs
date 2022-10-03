﻿
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

    class Program
    {

        enum GameMode {
            STANDBY=0,
            PLAY,
            HIGHT_SCORES,
            GAME_OVER
        }

        const string TITLE = "C# SDL2 Tetris";

        static int VelH = 0;
        static GameMode gameMode = GameMode.PLAY;

        static int curScore = 0;
        public static Tetromino? curTetromino;
        public static Tetromino? nextTetromino;

        public delegate bool ProcessEvent(ref SDL.SDL_Event e);
        static ProcessEvent? processEvent;

        //---------------------------------------------------------------------
        //-- Play Mode
        static bool processPlayEvent(ref SDL.SDL_Event e){
            bool quit = false;
            switch(e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    quit = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat==0){
                        switch (e.key.keysym.sym){
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                gameMode = GameMode.STANDBY;
                                processEvent = processStandByEvent;
                                break;
                            case SDL.SDL_Keycode.SDLK_LEFT:
                                VelH = -1;
                                break;
                            case SDL.SDL_Keycode.SDLK_RIGHT:
                                VelH = 1;                            
                                break;
                            case SDL.SDL_Keycode.SDLK_UP:
                                if (curTetromino!=null){
                                    curTetromino.RotateLeft();
                                }
                                break;

                        }
                    }
                    break;
               case SDL.SDL_EventType.SDL_KEYUP:
                    switch (e.key.keysym.sym){
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            VelH = 0;
                            break;
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            VelH = 0;                            
                            break;
                    }
                    break;
            }
            return quit;
        }

        static void DrawScore(IntPtr renderer,IntPtr tt_font){

            SDL.SDL_Color fg;
            fg.r = 255;
            fg.g = 255;
            fg.b = 0;
            fg.a = 255;
            var txtScore = String.Format("SCORE : {0:00000}", curScore);
            var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtScore,fg);
            if (surfScore!=IntPtr.Zero){

                var textureSCore =  SDL.SDL_CreateTextureFromSurface(renderer,surfScore);
                if (textureSCore!=IntPtr.Zero){
                    uint format;
                    int access,w,h;
                    SDL.SDL_QueryTexture(textureSCore,out format,out access,out w,out h);

                    SDL.SDL_Rect desRect;
                    desRect.x = Globals.LEFT;
                    desRect.y = (Globals.NB_ROWS+1)*Globals.cellSize;
                    desRect.w = w;
                    desRect.h = h;
                    SDL.SDL_RenderCopy(renderer,textureSCore, IntPtr.Zero ,ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                }

                SDL.SDL_free(surfScore);
            }



        }

        //---------------------------------------------------------------------
        //-- StandBy Mode
        static bool processStandByEvent(ref SDL.SDL_Event e){
            bool quit = false;
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
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                gameMode = GameMode.PLAY;
                                processEvent = processPlayEvent;
                                NewTetromino();
                                break;
                        }
                    }
                    break;
            }
            return quit;
        }

        static void DrawStandBy(IntPtr renderer,IntPtr tt_font){
            SDL.SDL_Color fg;
            fg.r = 255;
            fg.g = 255;
            fg.b = 0;
            fg.a = 255;

            var yLine = Globals.TOP + (Globals.NB_ROWS/4)*Globals.cellSize;
            var txtScore = "Tetris powered by SDL2";
            var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtScore,fg);
            if (surfScore!=IntPtr.Zero){

                var textureSCore =  SDL.SDL_CreateTextureFromSurface(renderer,surfScore);
                if (textureSCore!=IntPtr.Zero){
                    uint format;
                    int access,w,h;
                    SDL.SDL_QueryTexture(textureSCore,out format,out access,out w,out h);
                    SDL.SDL_Rect desRect;
                    desRect.x = Globals.LEFT + (Globals.NB_COLUMNS/2)*Globals.cellSize - w/2;
                    desRect.y = yLine;
                    desRect.w = w;
                    desRect.h = h;
                    SDL.SDL_RenderCopy(renderer,textureSCore, IntPtr.Zero ,ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                    yLine += 2*h + 4;
                }

                SDL.SDL_free(surfScore);
            }

            txtScore = "Press SPACE to Start";
            surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtScore,fg);
            if (surfScore!=IntPtr.Zero){

                var textureSCore =  SDL.SDL_CreateTextureFromSurface(renderer,surfScore);
                if (textureSCore!=IntPtr.Zero){
                    uint format;
                    int access,w,h;
                    SDL.SDL_QueryTexture(textureSCore,out format,out access,out w,out h);

                    SDL.SDL_Rect desRect;
                    desRect.x = Globals.LEFT + (Globals.NB_COLUMNS/2)*Globals.cellSize - w/2;
                    desRect.y = yLine;
                    desRect.w = w;
                    desRect.h = h;
                    SDL.SDL_RenderCopy(renderer,textureSCore, IntPtr.Zero ,ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                }

                SDL.SDL_free(surfScore);
            }


        }

        //---------------------------------------------------------------------
        //--

        static void InitGame(){

            curScore = 0;

            curTetromino = null;

            nextTetromino = new Tetromino(Globals.rand.Next(1,8),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

        }

        static void NewTetromino(){

            curTetromino = nextTetromino;
            curTetromino.x = 6 * Globals.cellSize;
            curTetromino.y = 0;
            curTetromino.y = -curTetromino.MaxY1() * Globals.cellSize;
            nextTetromino = new Tetromino(Globals.rand.Next(1,8),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

        }

        static void Main(string[] args)
        {

			SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

            SDL_ttf.TTF_Init();

            var curDir = Directory.GetCurrentDirectory();
            string filePathFont = curDir + "\\sansation.ttf";
            var tt_font = SDL_ttf.TTF_OpenFont(filePathFont, 18);
            SDL_ttf.TTF_SetFontStyle(tt_font,SDL_ttf.TTF_STYLE_BOLD|SDL_ttf.TTF_STYLE_ITALIC);


            var window = IntPtr.Zero;
            
            window = SDL.SDL_CreateWindow(TITLE,SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED,Globals.WIN_WIDTH,Globals.WIN_HEIGHT,SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);


            var renderer = SDL.SDL_CreateRenderer(window,-1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED|SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
			

            DateTime randSeed = DateTime.Now;
            Globals.rand = new Random(randSeed.Millisecond);

            InitGame();

            gameMode = GameMode.STANDBY;
            processEvent = processStandByEvent;

            UInt64 startTime = SDL.SDL_GetTicks64();
            UInt64 startTimeH = startTime;

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
                    quit = processEvent(ref e);
                }

                if (gameMode == GameMode.PLAY){

                    curTime = SDL.SDL_GetTicks64();
                    if ((curTime-startTime)>30){
                        startTime = curTime;
                        if (curTetromino!=null){
                            curTetromino.y += 3; 
                        }

                    }

                    if ((curTime - startTimeH) > 30)
                    {
                        startTimeH = curTime;
                        if (VelH != 0)
                        {
                            if (curTetromino != null)
                            {
                                curTetromino.x += VelH;
                            }
                        }

                    }

                    //--
                    if (curTetromino!=null){
                        curTetromino.Draw(renderer);
                    }

                }else if (gameMode == GameMode.STANDBY){

                    DrawStandBy(renderer,tt_font);

                }


                
                if (nextTetromino!=null){
                    nextTetromino.Draw(renderer);
                }
                
                DrawScore(renderer,tt_font);

                //--
                SDL.SDL_RenderPresent(renderer);

            }

            SDL.SDL_DestroyWindow(window);

            SDL_ttf.TTF_CloseFont(tt_font);

            SDL_ttf.TTF_Quit();

			SDL.SDL_Quit();
            

        }
	
	}
}