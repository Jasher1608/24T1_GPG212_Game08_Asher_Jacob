using System.Collections.Generic;

namespace Chess
{
    public static class Board
    {
        public static int[] square = new int[64];

        public static int colourToMove = Piece.White;

        public static int enPassantTarget = -1;

        public static void LoadPositionFromFen(string fen)
        {
            var pieceTypeFromSymbol = new Dictionary<char, int>
            {
                ['k'] = Piece.King,
                ['p'] = Piece.Pawn,
                ['n'] = Piece.Knight,
                ['b'] = Piece.Bishop,
                ['r'] = Piece.Rook,
                ['q'] = Piece.Queen
            };

            string fenBoard = fen.Split(' ')[0];
            int file = 0, rank = 7;

            foreach (char symbol in fenBoard)
            {
                if (symbol == '/')
                {
                    file = 0;
                    rank--;
                }
                else if (char.IsDigit(symbol))
                {
                    file += (int)char.GetNumericValue(symbol);
                }
                else
                {
                    int pieceColor = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    square[rank * 8 + file] = pieceType | pieceColor;
                    file++;
                }
            }
        }

        public static void ToggleColourToMove()
        {
            colourToMove = (colourToMove == Piece.White) ? Piece.Black : Piece.White;
            MoveGenerator.moves = MoveGenerator.GenerateMoves();
        }
    }
}