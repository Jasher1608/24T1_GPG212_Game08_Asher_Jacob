namespace Chess
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using static MoveGenerator;

    public static class LegalMoveGenerator
    {
        public static List<Move> legalMoves = new List<Move>();

        public static List<Move> GenerateLegalMoves()
        {
            List<Move> pseudoLegalMoves = MoveGenerator.GenerateMoves();
            List<Move> legalMoves = new List<Move>();

            foreach (Move move in pseudoLegalMoves)
            {
                // Apply the move to see if it results in a check
                Board.MakeMove(move, false, false); // Assume MakeMove temporarily applies a move

                // Find the king's square after the move
                int kingSquare = FindKingSquare(Board.colourToMove);

                if (!IsSquareAttacked(kingSquare, opponentColour))
                {
                    legalMoves.Add(move);
                }

                Board.UnmakeMove(false); // Revert the move
            }
            return legalMoves;
        }

        private static int FindKingSquare(int colour)
        {
            for (int i = 0; i < 64; i++)
            {
                if (Board.square[i] == (colour | Piece.King))
                {
                    return i;
                }
            }
            throw new Exception("King not found. Invalid board state.");
        }

    }
}