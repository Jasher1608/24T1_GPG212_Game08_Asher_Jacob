namespace Chess
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Board
    {
        public static int[] square = new int[64];

        public static int colourToMove = Piece.White;

        public static int enPassantTarget = -1;

        public static bool pendingPromotion = false;

        public static bool CanCastleKingsideWhite = true;
        public static bool CanCastleQueensideWhite = true;
        public static bool CanCastleKingsideBlack = true;
        public static bool CanCastleQueensideBlack = true;

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

        public static void ApplyMove(int startingIndex, int piece)
        {
            // If a king moves, disable castling on both sides for that color
            if (Piece.IsType(piece, Piece.King))
            {
                if (Piece.IsColour(piece, Piece.White))
                {
                    CanCastleKingsideWhite = false;
                    CanCastleQueensideWhite = false;
                }
                else
                {
                    CanCastleKingsideBlack = false;
                    CanCastleQueensideBlack = false;
                }
            }
            // If a rook moves, disable castling on the relevant side
            else if (Piece.IsType(piece, Piece.Rook))
            {
                if (startingIndex == 0) CanCastleQueensideWhite = false;
                else if (startingIndex == 7) CanCastleKingsideWhite = false;
                else if (startingIndex == 56) CanCastleQueensideBlack = false;
                else if (startingIndex == 63) CanCastleKingsideBlack = false;
            }
        }
    }
}