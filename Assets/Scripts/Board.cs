namespace Chess
{
    public static class Board
    {
        public static int[] Square;

        static Board()
        {
            Square = new int[64];

            Square[0] = Piece.White | Piece.Bishop;
            Square[63] = Piece.Black | Piece.Queen;
            Square[7] = Piece.Black | Piece.Knight;
        }
    }
}