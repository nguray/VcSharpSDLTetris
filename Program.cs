/*--------------------------------------------*\
		Simple Tetris using sdl2
                
	Raymond NGUYEN THANH         2022-10-06
\*--------------------------------------------*/

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

    class HighScore{

        public string Name  {get; set;}
        public int    Score {get; set;}

        public HighScore(string name,int score)
        {
            Name = name;
            Score = score;
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

        public delegate bool IsOutLimit_t();
        static IsOutLimit_t? IsOutLimit;


        static public Int32 idTetrominoBag = 14;
        static public Int32[] tetrominoBag = {1,2,3,4,5,6,7,1,2,3,4,5,6,7};  

        static Dictionary<SDL.SDL_Keycode, char> keys = new Dictionary<SDL.SDL_Keycode, char>{
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

        public static List<HighScore> highScores = new List<HighScore>() {
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)},
            {new HighScore("XXXXXX", 0)}
        };
        public static Int32  idHighScore = -1;
        public static string playerName = "";
        static public Int32 iColorHighScore = 0;


        public static bool fExitProgram = false;

        //---------------------------------------------------------------------
        //-- Play Mode
        static bool processPlayEvent(ref SDL.SDL_Event e){
            bool fStopGame = false;
            switch(e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    fExitProgram = true;
                    fStopGame = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat==0){
                        switch (e.key.keysym.sym){
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                fStopGame = true;
                                gameMode = GameMode.STANDBY;
                                processEvent = processStandByEvent;
                                break;
                            case SDL.SDL_Keycode.SDLK_LEFT:
                                VelH = -1;
                                IsOutLimit = curTetromino.IsOutLeft;
                                break;
                            case SDL.SDL_Keycode.SDLK_RIGHT:
                                VelH = 1;                            
                                IsOutLimit = curTetromino.IsOutRight;
                                break;
                            case SDL.SDL_Keycode.SDLK_UP:
                                if (curTetromino!=null){
                                    curTetromino.RotateLeft();
                                    if (curTetromino.HitGround(board)){
                                        //-- Undo Rotate
                                        curTetromino.RotateRight();
                                    }else if (curTetromino.IsOutRight()){
                                        var backupX = curTetromino.x;
                                        //-- Move Inside board
                                        while (curTetromino.IsOutRight()){
                                            curTetromino.x--;
                                        }
                                        if (curTetromino.HitGround(board)){
                                            curTetromino.x = backupX;
                                            //-- Undo Rotate
                                            curTetromino.RotateRight();

                                        }
                                    }else if (curTetromino.IsOutLeft()){
                                        var backupX = curTetromino.x;
                                        //-- Move Inside Board
                                        while(curTetromino.IsOutLeft()){
                                            curTetromino.x++;
                                        }
                                        if (curTetromino.HitGround(board)){
                                            curTetromino.x = backupX;
                                            //-- Undo Rotate
                                            curTetromino.RotateRight();

                                        }

                                    }

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
            return fStopGame;
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
                    fExitProgram = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat==0){
                        switch (e.key.keysym.sym){
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                quit = true;
                                fExitProgram = true;
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
        //-- HighScores Mode
        static bool processHighScoresEvent(ref SDL.SDL_Event e){
            bool quit = false;
            switch(e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    fExitProgram = true;
                    quit = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat==0){
                        switch (e.key.keysym.sym){
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                quit = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_BACKSPACE:
                                if (playerName.Length==1){
                                    playerName = "";
                                }else if (playerName.Length>1){
                                    playerName = playerName.Substring(0,playerName.Length-1);
                                }
                                highScores[idHighScore].Name = playerName;
                                break;
                            case SDL.SDL_Keycode.SDLK_RETURN or SDL.SDL_Keycode.SDLK_KP_ENTER:
                                if (playerName.Length==0){
                                    playerName = "XXXXXX";
                                }
                                highScores[idHighScore].Name = playerName;
                                saveHighScores();
                                gameMode = GameMode.STANDBY;
                                processEvent = processStandByEvent;
                                break;
                            default:
                                char c;
                                if (keys.TryGetValue(e.key.keysym.sym,out c)){
                                    if (playerName.Length<8){
                                        playerName += c;
                                    }
                                    highScores[idHighScore].Name = playerName;
                                    //Console.WriteLine(userName);
                                }
                                break;
                        }
                    }
                    break;
            }
            return quit;
        }

        static void DrawHighScores(IntPtr renderer,IntPtr tt_font){
 
            SDL.SDL_Color fg;
            fg = new SDL.SDL_Color {
                r = 255,
                g = 255,
                b = 0,
                a = 255
            };
            var yLine = Globals.TOP + Globals.cellSize;
            var txtTitle = "HIGH SCORES";
            var surfTitle = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtTitle,fg);
            if (surfTitle!=IntPtr.Zero){

                var textureSCore =  SDL.SDL_CreateTextureFromSurface(renderer,surfTitle);
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
                    yLine += 3*h;
                }

                SDL.SDL_free(surfTitle);
            }

            var xCol0 = Globals.LEFT + Globals.cellSize;
            var xCol1 = Globals.LEFT + (Globals.NB_COLUMNS/2+2)*Globals.cellSize;
            for(int i=0;i<highScores.Count;i++){
                int access,width, heigh = 0;
                var h = highScores[i];

                 if (i==idHighScore){
                    if ((iColorHighScore % 2)>0){
                        fg = new SDL.SDL_Color {
                            r = 255,
                            g = 255,
                            b = 0,
                            a = 255
                        };
                    }else{
                        fg = new SDL.SDL_Color {
                            r = 155,
                            g = 155,
                            b = 0,
                            a = 255
                        };
                    }
                }else{
                    fg = new SDL.SDL_Color {
                        r = 255,
                        g = 255,
                        b = 0,
                        a = 255
                    };
                }

                var surfName = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,h.Name,fg);
                if (surfName!=IntPtr.Zero){
                    var textureName =  SDL.SDL_CreateTextureFromSurface(renderer,surfName);
                    if (textureName!=IntPtr.Zero){
                        uint format;
                        SDL.SDL_QueryTexture(textureName,out format,out access,out width,out heigh);

                        SDL.SDL_Rect desRect;
                        desRect.x = xCol0;
                        desRect.y = yLine;
                        desRect.w = width;
                        desRect.h = heigh;
                        SDL.SDL_RenderCopy(renderer,textureName, IntPtr.Zero ,ref desRect);
                        SDL.SDL_DestroyTexture(textureName);
                   }

                    SDL.SDL_free(surfName);
    
                }
                
                var txtScore = String.Format("{0:00000}", h.Score);
                var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font,txtScore,fg);
                if (surfScore!=IntPtr.Zero){
                    var textureScore =  SDL.SDL_CreateTextureFromSurface(renderer,surfScore);
                    if (textureScore!=IntPtr.Zero){
                        uint format;
                        SDL.SDL_QueryTexture(textureScore,out format,out access,out width,out heigh);

                        SDL.SDL_Rect desRect;
                        desRect.x = xCol1;
                        desRect.y = yLine;
                        desRect.w = width;
                        desRect.h = heigh;
                        SDL.SDL_RenderCopy(renderer,textureScore, IntPtr.Zero ,ref desRect);
                        SDL.SDL_DestroyTexture(textureScore);
                    }

                    SDL.SDL_free(surfScore);
    
                }

                yLine += heigh + 8;
                
            }

        }

        public static (string Name, int score) ParseHighScore(string line)
        {
            //------------------------------------------------------                
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = line.Split(delimiterChars);
            string n = words[0];
            int    s = int.Parse(words[1]);
            return (n,s);

        } 

        public static void loadHighScores()
        {
            int     iLine = 0;
            string  name;
            int     score;
            string  path = @"HighScores.txt";
            //------------------------------------------------------
            if (File.Exists(path))
            {
                try
                {
                    
                    highScores.Clear();

                    foreach (string line in System.IO.File.ReadLines(path))
                    {
                        //--
                        (name, score) = ParseHighScore( line);
                        highScores.Add(new HighScore(name,score));
                        //--
                        iLine++;
                        if (iLine>9) break;

                    }
                }
                catch (FileNotFoundException uAEx)
                {
                    Console.WriteLine(uAEx.Message);
                }
            }
        }

        public static void WriteLine(FileStream fs, string value)
        {
            //------------------------------------------------------
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        public static void saveHighScores()
        {
            string path = @"HighScores.txt";
            //------------------------------------------------------
            // Delete the file if it exists.
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (FileStream fs = File.Create(path))
            {
                String lin;
                foreach ( var h in highScores ){
                    lin = String.Format("{0},{1}\n", h.Name, h.Score);
                    WriteLine(fs, lin);
                }

            }

            
        }

        public static void insertHighScore(int id,String name,int score){
            if ((id>=0)&&(id<10)){
                highScores.Insert(id, new HighScore(name, score));
                if (highScores.Count>10){
                    highScores.RemoveAt(highScores.Count-1);
                }
            }
        }

        public static Int32 IsHighScore(int score)
        {
            //---------------------------------------------------
            for (int i=0;i<10;i++){
                if (score>highScores[i].Score){
                    return i;
                }
            }
            return -1;
        }


        //---------------------------------------------------------------------
        //-- Game Over Mode

        static bool processGameOverEvent(ref SDL.SDL_Event e){
            bool quit = false;
            switch(e.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    quit = true;
                    fExitProgram = true;
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat==0){
                        switch (e.key.keysym.sym){
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                                quit = true;
                                fExitProgram = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                gameMode = GameMode.STANDBY;
                                processEvent = processStandByEvent;
                                break;
                        }
                    }
                    break;
            }
            return quit;
        }

        static void DrawGameOver(IntPtr renderer,IntPtr tt_font){

            var fg = new SDL.SDL_Color {
                r = 255,
                g = 255,
                b = 0,
                a = 255
            };

            var yLine = Globals.TOP + (Globals.NB_ROWS/4)*Globals.cellSize;
            var txtScore = "Game Over";
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

            txtScore = "Press SPACE to Continue";
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

        public static bool isGameOver()
        {
            //----------------------------------------
            for(int c=0;c<Globals.NB_COLUMNS;c++){
                if (board[c]!=0){
                    return true;
                }
            }
            return false;
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

        static bool IsOutAlways(){
            return true;
        }

        static void Main(string[] args)
        {

            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

			if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING)<0){
                Console.WriteLine($"There was an issue initializing SDL. {SDL.SDL_GetError()}");
            }

            SDL_ttf.TTF_Init();

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

            //--Extract embedded ressources
            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"SDLTetris.sansation.ttf"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("sansation.ttf", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }
            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"SDLTetris.109662__grunz__success.wav"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("109662__grunz__success.wav", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }
            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"SDLTetris.Tetris.wav"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("Tetris.wav", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }
            

            //var curDir = Directory.GetCurrentDirectory();
            string filePathFont = "sansation.ttf";
            var tt_font = SDL_ttf.TTF_OpenFont(filePathFont, 18);
            if (tt_font!=IntPtr.Zero){
                SDL_ttf.TTF_SetFontStyle(tt_font,SDL_ttf.TTF_STYLE_BOLD|SDL_ttf.TTF_STYLE_ITALIC);
            }
            string filePathMusic = "Tetris.wav";
            SDL_mixer.Mix_OpenAudio(44100,SDL_mixer.MIX_DEFAULT_FORMAT,SDL_mixer.MIX_DEFAULT_CHANNELS,1024);
            var music = SDL_mixer.Mix_LoadMUS(filePathMusic);
            if (music!=IntPtr.Zero){
                SDL_mixer.Mix_PlayMusic(music,-1);
                SDL_mixer.Mix_VolumeMusic(20);
            }
            string filePathSound = "109662__grunz__success.wav";
            var sound = SDL_mixer.Mix_LoadWAV(filePathSound);
            if (sound!=IntPtr.Zero){
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

            loadHighScores();

            gameMode = GameMode.STANDBY;
            processEvent = processStandByEvent;
            IsOutLimit = IsOutAlways;

            UInt64 startTimeV = SDL.SDL_GetTicks64();
            UInt64 startTimeH = startTimeV;
            UInt64 startTimeR = startTimeV;

            UInt64 curTime = 0;

            SDL.SDL_Event e;
            SDL.SDL_Rect rect;

            bool fStopGame = false;

            while(!fStopGame)
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
                    fStopGame = processEvent(ref e);
                    
                    if (fExitProgram){
                        break;
                    }

                    if (fStopGame){

                        fStopGame = false;
                        idHighScore = IsHighScore(curScore);
                        if (idHighScore>=0){
                            insertHighScore(idHighScore,playerName,curScore);
                            gameMode = GameMode.HIGHT_SCORES;
                            processEvent = processHighScoresEvent;
                            InitGame();
                        }else{
                            InitGame();
                            gameMode = GameMode.STANDBY;
                            processEvent = processStandByEvent;
                        }

                    }
                    
                }

                if (gameMode == GameMode.PLAY){

                    if (curTetromino!=null){

                        if (nbCompletedLines>0){
                            curTime = SDL.SDL_GetTicks64();
                            if ((curTime - startTimeV) > 200){
                                startTimeV = curTime;
                                nbCompletedLines--;
                                EraseFirstCompletedLine();
                                if (sound!=IntPtr.Zero){
                                    SDL_mixer.Mix_PlayChannel(SDL_mixer.MIX_DEFAULT_CHANNELS,sound,0);
                                }
                            }

                        }else if (horizontalMove!=0){
                            
                            curTime = SDL.SDL_GetTicks64();
                            
                            if ((curTime - startTimeH) > 20){
                                for(int i=0;i<5;i++){

                                    var backupX = curTetromino.x;
                                    curTetromino.x += horizontalMove;
                                    //Console.WriteLine(horizontalMove);
                                    if (IsOutLimit()){
                                        curTetromino.x = backupX;
                                        horizontalMove = 0;
                                        break; 
                                    }else{
                                        if (curTetromino.HitGround(board)){
                                            curTetromino.x = backupX;
                                            horizontalMove = 0;
                                            break;
                                        }
                                    }

                                    if (horizontalMove!=0){
                                        startTimeH = curTime;
                                        if (horizontalStartColumn!= curTetromino.Column()){
                                            curTetromino.x = backupX;
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
                                    if (curTetromino.HitGround(board)){
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
                                    if ((fDrop)&&(VelH!=0)){
                                        if ((curTime-startTimeH)>15){
                                            var backupX = curTetromino.x;
                                            curTetromino.x += VelH;
                                            if (IsOutLimit()){
                                                curTetromino.x = backupX;
                                            }else{
                                                if (curTetromino.HitGround(board)){
                                                    curTetromino.x = backupX;
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
                                
                                for(int i=0;i<4;i++){
                                    //-- Move down to check
                                    curTetromino.y++;
                                    var fMove = true;
                                    if (curTetromino.HitGround(board)){
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

                                                var backupX = curTetromino.x;
                                                curTetromino.x += VelH;

                                                if (IsOutLimit()){
                                                    curTetromino.x = backupX;
                                                }else{
                                                    if (curTetromino.HitGround(board)){
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
                        }
                    
                    }

                    //--
                    if (curTetromino!=null){
                        curTetromino.Draw(renderer);
                    }

                    //-- Check Game Over
                    if (isGameOver()){
                        idHighScore = IsHighScore(curScore);
                        if (idHighScore>=0){
                            insertHighScore(idHighScore,playerName,curScore);
                            gameMode = GameMode.HIGHT_SCORES;
                            processEvent = processHighScoresEvent;
                            InitGame();
                        }else{
                            InitGame();
                            gameMode = GameMode.GAME_OVER;
                            processEvent = processGameOverEvent;
                        }

                    }


                }else if (gameMode == GameMode.STANDBY){

                    DrawStandBy(renderer,tt_font);

                }else if (gameMode == GameMode.HIGHT_SCORES){

                    curTime = SDL.SDL_GetTicks64();
                    if ((curTime-startTimeH)>200){
                        startTimeH = curTime;
                        iColorHighScore++;
                    }

                    DrawHighScores(renderer,tt_font);

                }else if (gameMode == GameMode.GAME_OVER){

                    DrawGameOver(renderer,tt_font);

                }
                
                if (nextTetromino!=null){
                    curTime = SDL.SDL_GetTicks64();
                    if ((curTime-startTimeR)>500){
                        startTimeR = curTime;
                        nextTetromino.RotateLeft();
                    }
                    nextTetromino.Draw(renderer);
                }
                

                //--
                DrawScore(renderer,tt_font);

                //--
                SDL.SDL_RenderPresent(renderer);

            }

            SDL.SDL_DestroyWindow(window);

            if (tt_font != IntPtr.Zero)
            {
                SDL_ttf.TTF_CloseFont(tt_font);
            }
            SDL_ttf.TTF_Quit();

            if (music != IntPtr.Zero)
            {
                SDL_mixer.Mix_FreeMusic(music);
            }
            if (sound != IntPtr.Zero)
            {
                SDL_mixer.Mix_FreeChunk(sound);
            }
            SDL_mixer.Mix_Quit();

            SDL.SDL_Quit();


        }
	
    }
}