namespace SDLTetris
{

    class Tetromino{
        public int type = 0;
        public int x = 0;
        public int y = 0;
        public Color color = new Color(0,0,0);
        public List<Vector2i> m_vectors = new List<Vector2i>();

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
                m_vectors.Add(new Vector2i(v.x,v.y));
            }
            color = Colors[type];
            
        }

    }

}