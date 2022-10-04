
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

        public static bool fFastDown = false;
        public static bool fDrop = false;
        public static Int32 nbCompletedLines = 0;

        public static int[] board = new int[Globals.NB_COLUMNS*Globals.NB_ROWS];

        public static Int32 horizontalMove = 0;
        public static Int32 horizontalStartColumn = 0;

        public delegate bool ProcessEvent(ref SDL.SDL_Event e);
        static ProcessEvent? processEvent;

        static public Int32 idTetrominoBag = 14;
        static public Int32[] tetrominoBag = {1,2,3,4,5,6,7,1,2,3,4,5,6,7};  

        static public IntPtr sound = IntPtr.Zero;

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
                            case SDL.SDL_Keycode.SDLK_DOWN:
                                fFastDown = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                fDrop = true;
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
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            fFastDown = false;
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

            var fg = new SDL.SDL_Color {
                r = 255,
                g = 255,
                b = 0,
                a = 255
            };

            var yLine = Globals.TOP + (Globals.NB_ROWS/4)*Globals.cellSize;
            var txtScore = "Tetris powered by SDL2";
            var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtScore,fg);
            if (surfScore!=IntPtr.Zero){

                var textureSCore =  SDL.SDL_CreateTextureFromSurface(renderer,surfScore);
                if (textureSCore!=IntPtr.Zero){
                    uint format;
                    int access,w,h;
                    SDL.SDL_QueryTexture(textureSCore,out format,out access,out w,out h);
                    var desRect = new SDL.SDL_Rect {
                        x = Globals.LEFT + (Globals.NB_COLUMNS/2)*Globals.cellSize - w/2, 
                        y = yLine,
                        w = w,
                        h = h
                    };
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

            for(int i=0;i<board.Length;i++){
                board[i] = 0;
            }

            curTetromino = null;
            nextTetromino = new Tetromino(TetrisRandomizer(),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

        }

        static void NewTetromino(){

            curTetromino = nextTetromino;
            curTetromino.x = 6 * Globals.cellSize;
            curTetromino.y = 0;
            curTetromino.y = -curTetromino.MaxY1() * Globals.cellSize;
            nextTetromino = new Tetromino(TetrisRandomizer(),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

        }

        static void DrawBoard(IntPtr renderer){

            SDL.SDL_Rect rect;
            var a = Globals.cellSize - 2;
            rect.w = a;
            rect.h = a;
             for(int l=0;l<Globals.NB_ROWS;l++){
                for(int c=0;c<Globals.NB_COLUMNS;c++){
                    var typ = board[l*Globals.NB_COLUMNS+c];
                    if (typ!=0){
                        var color = Tetromino.Colors[typ];
                        SDL.SDL_SetRenderDrawColor(renderer, color.red, color.green, color.blue, 255);
                        rect.x = c*Globals.cellSize + Globals.LEFT + 1;
                        rect.y = l*Globals.cellSize + Globals.TOP + 1;
                        SDL.SDL_RenderFillRect(renderer,ref rect);
                    }
                }
            }

        }

        static Int32 ComputeScore(Int32 nbLines){
            switch(nbLines){
                case 0 :
                    return 0;
                case 1 :
                    return 40;
                case 2 :
                    return 100;
                case 3 :
                    return 300;
                case 4 :
                    return 1200;
                default:
                    return 2000; 
            }
        }


        static Int32 ComputeCompledLines(){

            Int32 nbLines = 0;
            bool fCompleted = false;
            for(int r=0;r<Globals.NB_ROWS;r++){
                fCompleted = true;
                for(int c=0;c<Globals.NB_COLUMNS;c++){
                    if (board[r*Globals.NB_COLUMNS+c]==0){
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted){
                    nbLines++;
                }
            }
            return nbLines;
        }

        static void EraseFirstCompletedLine(){
            //---------------------------------------------------
            bool fCompleted = false;
            for(int r=0;r<Globals.NB_ROWS;r++){
                fCompleted = true;
                for(int c=0;c<Globals.NB_COLUMNS;c++){
                    if (board[r*Globals.NB_COLUMNS+c]==0){
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted){
                    //-- Décaler d'une ligne le plateau
                    for(int r1=r;r1>0;r1--){
                        for(int c1=0;c1<Globals.NB_COLUMNS;c1++){
                            board[r1*Globals.NB_COLUMNS+c1] = board[(r1-1)*Globals.NB_COLUMNS+c1];
                        }
                    }
                    return;
                }
            }
        } 

        static void FreezeCurTetromino(){
            //----------------------------------------------------
            if (curTetromino!=null){
                var ix = (curTetromino.x + 1) / Globals.cellSize;
                var iy = (curTetromino.y + 1) / Globals.cellSize;
                foreach(var v in curTetromino.vectors){
                    var x = v.x + ix;
                    var y = v.y + iy;
                    if ((x>=0) && (x<Globals.NB_COLUMNS) && (y>=0) && (y<Globals.NB_ROWS)){
                        board[y*Globals.NB_COLUMNS+x] = curTetromino.type;
                    }
                }
                //--
                nbCompletedLines = ComputeCompledLines();
                if (nbCompletedLines>0){
                    curScore += ComputeScore(nbCompletedLines);
                    
                } 
            }

        }

        static Int32 TetrisRandomizer(){
            Int32 iSrc;
            Int32 iTyp=0;
            if (idTetrominoBag<14){
                iTyp = tetrominoBag[idTetrominoBag];
                idTetrominoBag++;
            }else{
                //-- Shuttle bag
                for(int i=0;i<tetrominoBag.Length;i++){
                    iSrc = Globals.rand.Next(0,14);
                    iTyp = tetrominoBag[iSrc];
                    tetrominoBag[iSrc] = tetrominoBag[0];
                    tetrominoBag[0] = iTyp;
                }
                iTyp =tetrominoBag[0];
                idTetrominoBag = 1;

            }
            return iTyp;
        }

        static void Main(string[] args)
        {

			SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

            SDL_ttf.TTF_Init();

            var curDir = Directory.GetCurrentDirectory();
            string filePathFont = curDir + "\\sansation.ttf";
            var tt_font = SDL_ttf.TTF_OpenFont(filePathFont, 18);
            SDL_ttf.TTF_SetFontStyle(tt_font,SDL_ttf.TTF_STYLE_BOLD|SDL_ttf.TTF_STYLE_ITALIC);

            string filePathMusic = curDir + "\\Tetris.wav";
            SDL_mixer.Mix_OpenAudio(44100,SDL_mixer.MIX_DEFAULT_FORMAT,SDL_mixer.MIX_DEFAULT_CHANNELS,1024);
            var music = SDL_mixer.Mix_LoadMUS(filePathMusic);
            if (music!=null){
                SDL_mixer.Mix_PlayMusic(music,-1);
                SDL_mixer.Mix_VolumeMusic(20);
            }

            string filePathSound = curDir + "\\109662__grunz__success.wav";
            sound = SDL_mixer.Mix_LoadWAV(filePathSound);
            if (sound!=null){
                SDL_mixer.Mix_Volume(-1,10);
            }

            var window = IntPtr.Zero;
            
            window = SDL.SDL_CreateWindow(TITLE,SDL.SDL_WINDOWPOS_CENTERED,
                        SDL.SDL_WINDOWPOS_CENTERED,Globals.WIN_WIDTH,Globals.WIN_HEIGHT,SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);


            var renderer = SDL.SDL_CreateRenderer(window,-1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED|SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
			

            DateTime randSeed = DateTime.Now;
            Globals.rand = new Random(randSeed.Millisecond);

            InitGame();

            // for(int i = 0;i<10;i++){
            //     board[Globals.NB_COLUMNS + i] = 2;
            // }

            gameMode = GameMode.STANDBY;
            processEvent = processStandByEvent;

            UInt64 startTimeV = SDL.SDL_GetTicks64();
            UInt64 startTimeH = startTimeV;

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

                //--
                DrawBoard(renderer);


                while (SDL.SDL_PollEvent(out e)!=0)
                {
                    quit = processEvent(ref e);
                }

                if (gameMode == GameMode.PLAY){

                    if (curTetromino!=null){

                        if (nbCompletedLines>0){
                            curTime = SDL.SDL_GetTicks64();
                            if ((curTime - startTimeV) > 200){
                                startTimeV = curTime;
                                nbCompletedLines--;
                                EraseFirstCompletedLine();
                                SDL_mixer.Mix_PlayChannel(SDL_mixer.MIX_DEFAULT_CHANNELS,sound,0);
                            }

                        }else if (horizontalMove!=0){
                            
                            curTime = SDL.SDL_GetTicks64();
                            
                            if ((curTime - startTimeH) > 20){
                                startTimeH = curTime;

                                for(int i=0;i<4;i++){
                                    curTetromino.x += horizontalMove;
                                    if (horizontalMove<0){
                                        if (curTetromino.IsOutLeft()){
                                            curTetromino.x -= horizontalMove;
                                            horizontalMove = 0;
                                            break; 
                                        }else{
                                            var idHit = curTetromino.HitGround(board);
                                            if (idHit>=0){
                                                curTetromino.x -= horizontalMove;
                                                horizontalMove = 0;
                                                break;
                                            }
                                        }

                                    }else if (horizontalMove>0){
                                        if (curTetromino.IsOutRight()){
                                            curTetromino.x -= horizontalMove;
                                            horizontalMove = 0;
                                            break; 
                                        }else{
                                            var idHit = curTetromino.HitGround(board);
                                            if (idHit>=0){
                                                curTetromino.x -= horizontalMove;
                                                horizontalMove = 0;
                                                break;
                                            }
                                        }

                                    }

                                    if (horizontalMove!=0){
                                        if (horizontalStartColumn!= curTetromino.Column()){
                                            curTetromino.x -= horizontalMove;
                                            horizontalMove = 0;
                                            break;
                                        }

                                    }

                                }

                            }
 
                        }else if (fDrop){
                            curTime = SDL.SDL_GetTicks64();
                            if ((curTime - startTimeV) > 10){
                                startTimeV = curTime;
                                for(int i=0;i<6;i++){
                                    //-- Move down to Check
                                    curTetromino.y++;
                                    var idHit = curTetromino.HitGround(board);
                                    if (idHit>=0){
                                        curTetromino.y--;
                                        FreezeCurTetromino();
                                        NewTetromino();
                                        fDrop = false;
                                    }else if (curTetromino.IsOutBottom()){
                                        curTetromino.y--;
                                        FreezeCurTetromino();
                                        NewTetromino();
                                        fDrop = false;
                                    }
                                    if (fDrop){

                                        if ((curTime-startTimeH)>20){
                                            curTetromino.x += VelH;
                                            if (VelH<0){
                                                if (curTetromino.IsOutLeft()){
                                                    curTetromino.x -= VelH;
                                                }else{
                                                    idHit = curTetromino.HitGround(board);
                                                    if (idHit>=0){
                                                        curTetromino.x -= VelH;
                                                    }else{
                                                        startTimeH = curTime;
                                                        horizontalMove = VelH;
                                                        horizontalStartColumn = curTetromino.Column();
                                                        break;
                                                    }

                                                }

                                            }else if (VelH>0){
                                                if (curTetromino.IsOutRight()){
                                                    curTetromino.x -= VelH;
                                                }else{
                                                    idHit = curTetromino.HitGround(board);
                                                    if (idHit>=0){
                                                        curTetromino.x -= VelH;
                                                    }else{
                                                        startTimeH = curTime;
                                                        horizontalMove = VelH;
                                                        horizontalStartColumn = curTetromino.Column();
                                                        break;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }

                        }else{
                            curTime = SDL.SDL_GetTicks64();

                            ulong limitElapse;
                            if (fFastDown) {
                                limitElapse = 10;
                            }else{
                                limitElapse = 25;
                            }

                            if ( (curTime-startTimeV)>limitElapse ){
                                startTimeV = curTime;
                                
                                for(int i=0;i<3;i++){
                                    //-- Move down to check
                                    curTetromino.y++;
                                    var fMove = true;
                                    var idHit = curTetromino.HitGround(board);
                                    if (idHit>=0){
                                        curTetromino.y--;
                                        FreezeCurTetromino();
                                        NewTetromino();
                                        fMove = false;
                                    }else if (curTetromino.IsOutBottom()){
                                        curTetromino.y--;
                                        FreezeCurTetromino();
                                        NewTetromino();
                                        fMove = false;
                                    }

                                    if (fMove){
                                        if (VelH!=0){
                                            if ((curTime-startTimeH)>15){
                                                startTimeH = curTime;
                                                curTetromino.x += VelH;
                                                if (VelH<0){
                                                    if (curTetromino.IsOutLeft()){
                                                        curTetromino.x -= VelH;
                                                    }else{
                                                        idHit = curTetromino.HitGround(board);
                                                        if (idHit>=0){
                                                            curTetromino.x -= VelH;
                                                        }else{
                                                            horizontalMove = VelH;
                                                            horizontalStartColumn = curTetromino.Column();
                                                            break;
                                                        }
                                                    }

                                                }else if (VelH>0){
                                                    if (curTetromino.IsOutRight()){
                                                        curTetromino.x -= VelH;
                                                    }else{
                                                        idHit = curTetromino.HitGround(board);
                                                        if (idHit>=0){
                                                            curTetromino.x -= VelH;
                                                        }else{
                                                            horizontalMove = VelH;
                                                            horizontalStartColumn = curTetromino.Column();
                                                            break;
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                    }

                                }
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
                

                //--
                DrawScore(renderer,tt_font);

                //--
                SDL.SDL_RenderPresent(renderer);

            }

            SDL.SDL_DestroyWindow(window);

            SDL_ttf.TTF_CloseFont(tt_font);

            SDL_ttf.TTF_Quit();

            if (music!=null){
                SDL_mixer.Mix_FreeMusic(music);
            }
            if (sound!=null){
                SDL_mixer.Mix_FreeChunk(sound);
            }
            SDL_mixer.Mix_Quit();

			SDL.SDL_Quit();
            

        }
	
	}
}