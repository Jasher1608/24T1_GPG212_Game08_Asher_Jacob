namespace Chess
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static Evaluation;

    public static class Search
    {
        public static int SearchMoves(int depth, int alpha, int beta)
        {
            if (depth == 0)
            {
                return Evaluate();
            }

            List<Move> moves = LegalMoveGenerator.GenerateLegalMoves();
            if (moves.Count == 0) // Checkmate/stalemate
            {
                int kingSquare = LegalMoveGenerator.FindKingSquare(Board.colourToMove);
                int attackerColour;
                if (Board.colourToMove == Piece.White)
                {
                    attackerColour = Piece.Black;
                }
                else
                {
                    attackerColour = Piece.White;
                }

                if (MoveGenerator.IsSquareAttacked(kingSquare, attackerColour, kingSquare))
                {
                    return int.MinValue;
                }
                return 0;
            }

            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                int evaluation = -SearchMoves(depth - 1, -beta, -alpha);
                Board.UnmakeMove();
                if (evaluation >= beta)
                {
                    return beta;
                }
                alpha = Math.Max(alpha, evaluation);
            }

            return alpha;
        }
    }
}