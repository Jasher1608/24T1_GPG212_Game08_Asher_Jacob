namespace Chess
{
    public static class Piece
    {
        public const int None = 0;
        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Rook = 5;
        public const int Queen = 6;

        public const int White = 8;
        public const int Black = 16;

        public static bool IsColour(int piece, int color)
        {
            return (piece & (White | Black)) == color;
        }

        public static bool IsType(int piece, int type)
        {
            return (piece & 7) == type;
        }

        public static bool IsSlidingPiece(int piece)
        {
            int type = piece & 7;
            return type == Bishop || type == Rook || type == Queen;
        }
    }
}