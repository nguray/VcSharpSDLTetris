using System;
using SDL2;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics.Contracts;
using Microsoft.VisualBasic.FileIO;

namespace SDLTetris
{
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

    class Game : System.IDisposable
    {
        const string TITLE = "C# SDL2 Tetris";
        const string SANSATION_TTF = "sansation.ttf";
        const string TETRIS_WAV = "Tetris.wav";
        const string SUCCES_WAV = "109662__grunz__success.wav";

        public int VelH = 0;
        public int curScore = 0;
        public Tetromino? curTetromino;
        public Tetromino? nextTetromino;

        public bool fFastDown = false;
        public bool fDrop = false;
        public Int32 nbCompletedLines = 0;

        public int[] board = new int[Globals.NB_COLUMNS * Globals.NB_ROWS];

        public Int32 horizontalMove = 0;
        public Int32 horizontalStartColumn = 0;

        StandByMode? standByMode;
        PlayMode? playMode;
        HighSCoresMode? highScoresMode;
        GameOverMode? gameOverMode;

        public IGameMode? curMode;

        public ulong startTimeR = 0;

        public Int32 idTetrominoBag = 14;
        public Int32[] tetrominoBag = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };

        public List<HighScore> highScores = new List<HighScore>() {
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
        public Int32 idHighScore = -1;
        public string playerName = "";
        public Int32 iColorHighScore = 0;

        public nint renderer;
        public nint tt_font;
        public nint music;
        public nint sound;

        static void getEmbeddedResource(string resourceName)
        {
            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"SDLTetris.res.{resourceName}"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream(resourceName, FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }

        }

        public Game()
        {

            LoadResources();

            standByMode = new(this);
            playMode = new(this);
            highScoresMode = new(this);
            gameOverMode = new(this);
            SetStandByMode();

            InitGame();

            loadHighScores();

        }

        void LoadResources()
        {
            //--Extract embedded ressources
            getEmbeddedResource(SANSATION_TTF);
            getEmbeddedResource(SUCCES_WAV);
            getEmbeddedResource(TETRIS_WAV);

            //var curDir = Directory.GetCurrentDirectory();
            string filePathFont = SANSATION_TTF;
            tt_font = SDL_ttf.TTF_OpenFont(filePathFont, 18);
            if (tt_font != IntPtr.Zero)
            {
                SDL_ttf.TTF_SetFontStyle(tt_font, SDL_ttf.TTF_STYLE_BOLD | SDL_ttf.TTF_STYLE_ITALIC);
            }
            string filePathMusic = TETRIS_WAV;
            SDL_mixer.Mix_OpenAudio(44100, SDL_mixer.MIX_DEFAULT_FORMAT, SDL_mixer.MIX_DEFAULT_CHANNELS, 1024);
            music = SDL_mixer.Mix_LoadMUS(filePathMusic);
            if (music != IntPtr.Zero)
            {
                SDL_mixer.Mix_PlayMusic(music, -1);
                SDL_mixer.Mix_VolumeMusic(20);
            }
            string filePathSound = SUCCES_WAV;
            sound = SDL_mixer.Mix_LoadWAV(filePathSound);
            if (sound != IntPtr.Zero)
            {
                SDL_mixer.Mix_Volume(-1, 10);
            }
            
        }

        void FreeResources()
        {
            //--
            if (tt_font != IntPtr.Zero)
            {
                SDL_ttf.TTF_CloseFont(tt_font);
                tt_font = IntPtr.Zero;
            }
            if (music != IntPtr.Zero)
            {
                SDL_mixer.Mix_FreeMusic(music);
                music = IntPtr.Zero;
            }
            if (sound != IntPtr.Zero)
            {
                SDL_mixer.Mix_FreeChunk(sound);
                sound = IntPtr.Zero;
            }

            //-- 
            if (File.Exists(SANSATION_TTF))
            {
                File.Delete(SANSATION_TTF);                
            }

            if (File.Exists(SUCCES_WAV))
            {
                File.Delete(SUCCES_WAV);                
            }

            if (File.Exists(TETRIS_WAV))
            {
                File.Delete(TETRIS_WAV);                
            }

            Console.WriteLine(">>>FreeResources<<<<");

        }

        public void Dispose()
        {
            FreeResources();
        }

        ~Game()
        {
            Dispose();
        }

        public void InitGame()
        {

            curScore = 0;

            for (int i = 0; i < board.Length; i++)
            {
                board[i] = 0;
            }

        }

        public bool isGameOver()
        {
            //----------------------------------------
            for (int c = 0; c < Globals.NB_COLUMNS; c++)
            {
                if (board[c] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetStandByMode()
        {
            curMode = standByMode;
            curMode?.init();
            InitGame();

        }

        public void SetPlayMode()
        {
            curMode = playMode;
            curMode?.init();
        }
        public void SetHighScoresMode()
        {
            curMode = highScoresMode;
            curMode?.init();

        }
        public void SetGameOverMode()
        {
            curMode = gameOverMode;
            curMode?.init();

        }

        public (string Name, int score) ParseHighScore(string line)
        {
            //------------------------------------------------------                
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = line.Split(delimiterChars);
            string n = words[0];
            int s = int.Parse(words[1]);
            return (n, s);

        }

        public void loadHighScores()
        {
            int iLine = 0;
            string name;
            int score;
            string path = @"HighScores.txt";
            //------------------------------------------------------
            if (File.Exists(path))
            {
                try
                {

                    highScores.Clear();

                    foreach (string line in System.IO.File.ReadLines(path))
                    {
                        //--
                        (name, score) = ParseHighScore(line);
                        highScores.Add(new HighScore(name, score));
                        //--
                        iLine++;
                        if (iLine > 9) break;

                    }
                }
                catch (FileNotFoundException uAEx)
                {
                    Console.WriteLine(uAEx.Message);
                }
            }
        }

        public void WriteLine(FileStream fs, string value)
        {
            //------------------------------------------------------
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        public void saveHighScores()
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
                foreach (var h in highScores)
                {
                    lin = String.Format("{0},{1}\n", h.Name, h.Score);
                    WriteLine(fs, lin);
                }

            }


        }

        public void insertHighScore(int id, String name, int score)
        {
            if ((id >= 0) && (id < 10))
            {
                highScores.Insert(id, new HighScore(name, score));
                if (highScores.Count > 10)
                {
                    highScores.RemoveAt(highScores.Count - 1);
                }
            }
        }

        public Int32 IsHighScore(int score)
        {
            //---------------------------------------------------
            for (int i = 0; i < 10; i++)
            {
                if (score > highScores[i].Score)
                {
                    return i;
                }
            }
            return -1;
        }

        public void DrawBoard(IntPtr renderer)
        {

            SDL.SDL_Rect rect;
            var a = Globals.cellSize - 2;
            rect.w = a;
            rect.h = a;
            for (int l = 0; l < Globals.NB_ROWS; l++)
            {
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    var typ = board[l * Globals.NB_COLUMNS + c];
                    if (typ != 0)
                    {
                        var color = Tetromino.Colors[typ];
                        SDL.SDL_SetRenderDrawColor(renderer, color.red, color.green, color.blue, 255);
                        rect.x = c * Globals.cellSize + Globals.LEFT + 1;
                        rect.y = l * Globals.cellSize + Globals.TOP + 1;
                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }
                }
            }

        }

        public Int32 TetrisRandomizer()
        {
            Int32 iSrc;
            Int32 iTyp;
            if (idTetrominoBag < 14)
            {
                iTyp = tetrominoBag[idTetrominoBag];
                idTetrominoBag++;
            }
            else
            {
                //-- Shuttle bag
                if (Globals.rand is not null)
                {
                    for (int i = 0; i < tetrominoBag.Length; i++)
                    {
                        iSrc = Globals.rand.Next(0, 14);
                        iTyp = tetrominoBag[iSrc];
                        tetrominoBag[iSrc] = tetrominoBag[0];
                        tetrominoBag[0] = iTyp;
                    }
                    
                }
                iTyp = tetrominoBag[0];
                idTetrominoBag = 1;

            }
            return iTyp;
        }

        public void NewTetromino()
        {

            if (nextTetromino == null)
            {
                nextTetromino = new Tetromino(TetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);
            }
            curTetromino = nextTetromino;
            curTetromino.x = 6 * Globals.cellSize;
            curTetromino.y = 0;
            curTetromino.y = -curTetromino.MaxY1() * Globals.cellSize;
            nextTetromino = new Tetromino(TetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);

        }

        public Int32 ComputeScore(Int32 nbLines)
        {
            return nbLines switch
            {
                0 => 0,
                1 => 40,
                2 => 100,
                3 => 300,
                4 => 1200,
                _ => 2000,
            };
        }


        public Int32 ComputeCompledLines()
        {

            Int32 nbLines = 0;
            bool fCompleted = false;
            for (int r = 0; r < Globals.NB_ROWS; r++)
            {
                fCompleted = true;
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    if (board[r * Globals.NB_COLUMNS + c] == 0)
                    {
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted)
                {
                    nbLines++;
                }
            }
            return nbLines;
        }

        public void EraseFirstCompletedLine()
        {
            //---------------------------------------------------
            bool fCompleted = false;
            for (int r = 0; r < Globals.NB_ROWS; r++)
            {
                fCompleted = true;
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    if (board[r * Globals.NB_COLUMNS + c] == 0)
                    {
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted)
                {
                    //-- Décaler d'une ligne le plateau
                    for (int r1 = r; r1 > 0; r1--)
                    {
                        for (int c1 = 0; c1 < Globals.NB_COLUMNS; c1++)
                        {
                            board[r1 * Globals.NB_COLUMNS + c1] = board[(r1 - 1) * Globals.NB_COLUMNS + c1];
                        }
                    }
                    return;
                }
            }
        }

        public void FreezeCurTetromino()
        {
            //----------------------------------------------------
            if (curTetromino != null)
            {
                var ix = (curTetromino.x + 1) / Globals.cellSize;
                var iy = (curTetromino.y + 1) / Globals.cellSize;
                foreach (var v in curTetromino.vectors)
                {
                    var x = v.x + ix;
                    var y = v.y + iy;
                    if ((x >= 0) && (x < Globals.NB_COLUMNS) && (y >= 0) && (y < Globals.NB_ROWS))
                    {
                        board[y * Globals.NB_COLUMNS + x] = curTetromino.type;
                    }
                }
                //--
                nbCompletedLines = ComputeCompledLines();
                if (nbCompletedLines > 0)
                {
                    curScore += ComputeScore(nbCompletedLines);

                }
            }

        }

        public void DrawScore(IntPtr renderer, IntPtr tt_font)
        {

            SDL.SDL_Color fg;
            fg.r = 255;
            fg.g = 255;
            fg.b = 0;
            fg.a = 255;
            var txtScore = String.Format("SCORE : {0:00000}", curScore);
            var surfScore = SDL_ttf.TTF_RenderUTF8_Blended(tt_font, txtScore, fg);
            if (surfScore != IntPtr.Zero)
            {

                var textureSCore = SDL.SDL_CreateTextureFromSurface(renderer, surfScore);
                if (textureSCore != IntPtr.Zero)
                {
                    uint format;
                    int access, w, h;
                    SDL.SDL_QueryTexture(textureSCore, out format, out access, out w, out h);

                    SDL.SDL_Rect desRect;
                    desRect.x = Globals.LEFT;
                    desRect.y = (Globals.NB_ROWS + 1) * Globals.cellSize;
                    desRect.w = w;
                    desRect.h = h;
                    SDL.SDL_RenderCopy(renderer, textureSCore, IntPtr.Zero, ref desRect);
                    SDL.SDL_DestroyTexture(textureSCore);
                }

                SDL.SDL_free(surfScore);
            }



        }

        public void Draw()
        {
            SDL.SDL_Rect rect;

            SDL.SDL_SetRenderDrawColor(renderer, 48, 48, 255, 255);
            SDL.SDL_RenderClear(renderer);

            rect.x = Globals.LEFT;
            rect.y = Globals.TOP;
            rect.w = Globals.cellSize * Globals.NB_COLUMNS;
            rect.h = Globals.cellSize * Globals.NB_ROWS;
            SDL.SDL_SetRenderDrawColor(renderer, 10, 10, 100, 255);
            SDL.SDL_RenderFillRect(renderer, ref rect);

            //--
            DrawBoard(renderer);

            //--
            DrawScore(renderer, tt_font);

            //--
            if (nextTetromino != null)
            {
                nextTetromino.Draw(renderer);
            }

            //--
            curMode?.Draw();

            //--
            SDL.SDL_RenderPresent(renderer);

        }

        public void Update()
        {
            if (curMode != null)
            {
                curMode.Update();            
            }

            if (nextTetromino != null)
            {
                ulong curTime = SDL.SDL_GetTicks64();
                if ((curTime - startTimeR) > 500)
                {
                    startTimeR = curTime;
                    nextTetromino.RotateLeft();
                }
            }

            //-- Check Game Over
            if (isGameOver())
            {
                idHighScore = IsHighScore(curScore);
                if (idHighScore >= 0)
                {
                    insertHighScore(idHighScore, playerName, curScore);
                    SetHighScoresMode();
                    InitGame();
                }
                else
                {
                    SetGameOverMode();
                }

            }

        }

        public void StopGame()
        {

            idHighScore = IsHighScore(curScore);
            if (idHighScore >= 0)
            {
                insertHighScore(idHighScore, playerName, curScore);
                SetHighScoresMode();
                InitGame();
            }
            else
            {
                SetStandByMode();
                InitGame();
            }

        }


    }


}