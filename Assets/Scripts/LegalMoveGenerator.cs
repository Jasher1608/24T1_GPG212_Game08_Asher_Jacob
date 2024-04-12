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
            List<Move> pseudoLegalMoves = GenerateMoves();
            legalMoves = new List<Move>();

            foreach (Move moveToVerify in pseudoLegalMoves)
            {
                Board.MakeMove(moveToVerify);
                List<Move> opponentResponses = GenerateMoves();

                if (opponentResponses.Any(response => Board.square[response.TargetSquare] == (Piece.King | opponentColour)))
                {

                }
                else
                {
                    legalMoves.Add(moveToVerify);
                }

                Board.UnmakeMove();
            }

            if (legalMoves.Count == 0)
            {
                UnityEngine.Debug.Log("No legal moves available for colour " + Board.colourToMove);
            }
            return legalMoves;
        }

        /* Bugs
        public static List<Move> GenerateLegalMoves()
        {
            List<Move> pseudoLegalMoves = GenerateMoves();
            legalMoves = new List<Move>();

            foreach (Move moveToVerify in pseudoLegalMoves)
            {
                
                Board.MakeMove(moveToVerify, false, false);
                int kingSquare = FindKingSquare(friendlyColour);

                if (!IsSquareAttacked(kingSquare, opponentColour, moveToVerify.TargetSquare))
                {
                    legalMoves.Add(moveToVerify);
                }

                Board.UnmakeMove(false);
            }

            if (legalMoves.Count == 0)
            {
                UnityEngine.Debug.Log("No legal moves available for colour " + Board.colourToMove);
            }
            return legalMoves;
        }
        */

        public static int FindKingSquare(int colour)
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