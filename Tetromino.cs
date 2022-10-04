using SDL2;

namespace SDLTetris
{

    class Tetromino{
        public int type = 0;
        public int x = 0;
        public int y = 0;
        public Color color = new Color(0,0,0);
        public List<Vector2i> vectors = new List<Vector2i>();

        static Vector2i[] TypeTetrominos = {
            new Vector2i(0,0),  new Vector2i(0,0),   new Vector2i(0,0),new Vector2i(0,0),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(-1,0),new Vector2i(-1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(0,1),new Vector2i(0,2),
            new Vector2i(-1,0), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(0,1),
            new Vector2i(0,0),  new Vector2i(1,0), new Vector2i(0,1),new Vector2i(1,1),
            new Vector2i(-1,-1),new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1),
            new Vector2i(1,-1), new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1)        
        };

        public static Color[] Colors = {
            new Color(0,0,0),
            new Color(0,255,0),
            new Color(255,0,0),
            new Color(255,0,255),
            new Color(0,255,255),
            new Color(0,0,255),
            new Color(255,128,0),
            new Color(255,255,0)
        };

        public Tetromino(int type,int x,int y){
            this.type = type;
            this.x = x;
            this.y = y;
            var id = type*4;
            for (int i=0;i<4;i++){
                var v = TypeTetrominos[id+i];
                vectors.Add(new Vector2i(v.x,v.y));
            }
            color = Colors[type];
            
        }

        public void Draw(IntPtr renderer){
            
            SDL.SDL_Rect rect;

            SDL.SDL_SetRenderDrawColor(renderer, color.red, color.green, color.blue, 255);
            var a = Globals.cellSize - 2;

            rect.w = a;
            rect.h = a;
            foreach (var v in vectors) {
                rect.x = v.x*Globals.cellSize + this.x + Globals.LEFT + 1;
                rect.y = v.y*Globals.cellSize + this.y + Globals.TOP + 1;
                if (rect.y >= Globals.TOP) {
                    SDL.SDL_RenderFillRect(renderer,ref rect);
                }
            }
        }

        public void RotateRight()
        {
            int     x,y;
            //-------------------------------------------
            if (type!=5){
                for (int i=0;i<vectors.Count;i++){
                    var v = vectors[i];
                    x = -v.y;
                    y = v.x;
                    v.x = x;
                    v.y = y;
                }
            }

        }

        public void RotateLeft()
        {
            int     x,y;
            //-------------------------------------------
           if (type!=5){
                for (int i=0;i<vectors.Count;i++){
                    var v = vectors[i];
                    x = v.y;
                    y = -v.x;
                    v.x = x;
                    v.y = y;
                }
           }            
        }

        public int MaxY1() {
            int y; 
            var maxY = vectors[0].y;
            for (int i=1;i<vectors.Count;i++){
                y = vectors[i].y;
                if (y > maxY) {
                    maxY = y;
                }
            }
            return maxY;
        }

        public int MinX1() {
            int x; 
            var minX = vectors[0].x;
            for (int i=1;i<vectors.Count;i++){
                x = vectors[i].x;
                if (x < minX) {
                    minX = x;
                }
            }
            return minX;
        }

        public int MaxX1() {
            int x; 
            var maxX = vectors[0].x;
            for (int i=1;i<vectors.Count;i++){
                x = vectors[i].x;
                if (x > maxX) {
                    maxX = x;
                }
            }
            return maxX;
        }

        public bool IsOutLeft(){
            var l = MinX1()*Globals.cellSize + x;
            return (l < 0);
        }

        public bool IsOutRight(){
            var r = MaxX1()*Globals.cellSize + Globals.cellSize + x;
            return (r > Globals.NB_COLUMNS*Globals.cellSize);
        }

        public bool IsOutBottom(){
            var b = MaxY1()*Globals.cellSize + Globals.cellSize + y;
            return (b>Globals.NB_ROWS*Globals.cellSize);
        }

        public Int32 HitGround(int[] board){
            Int32 ix,iy;
            Int32 x,y;
            Int32 iHit;

            foreach(var v in vectors){

                x = v.x*Globals.cellSize + this.x + 1;
                y = v.y*Globals.cellSize + this.y + 1;
                ix = x / Globals.cellSize;
                iy = y / Globals.cellSize;

                if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                    iHit = iy*Globals.NB_COLUMNS + ix;
                    if (board[iHit] != 0) {
                        return iHit;
                    }

                }

                x = v.x*Globals.cellSize + Globals.cellSize - 1 + this.x;
                y = v.y*Globals.cellSize + this.y + 1;
                ix = x / Globals.cellSize;
                iy = y / Globals.cellSize;

                if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                    iHit = iy*Globals.NB_COLUMNS + ix;
                    if (board[iHit] != 0) {
                        return iHit;
                    }

                }

                x = v.x*Globals.cellSize + Globals.cellSize - 1 + this.x;
                y = v.y*Globals.cellSize + Globals.cellSize - 1 + this.y;
                ix = x / Globals.cellSize;
                iy = y / Globals.cellSize;

                if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                    iHit = iy*Globals.NB_COLUMNS + ix;
                    if (board[iHit] != 0) {
                        return iHit;
                    }

                }

                x = v.x*Globals.cellSize + this.x + 1;
                y = v.y*Globals.cellSize + Globals.cellSize - 1 + this.y;
                ix = x / Globals.cellSize;
                iy = y / Globals.cellSize;

                if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                    iHit = iy*Globals.NB_COLUMNS + ix;
                    if (board[iHit] != 0) {
                        return iHit;
                    }

                }

            }


            return -1;
        }

        public Int32 Column(){
            return x / Globals.cellSize;
        }

    }

}