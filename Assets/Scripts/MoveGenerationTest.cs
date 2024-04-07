namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class MoveGenerationTest
    {
        public static int TestMoveGeneration(int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            List<Move> moves = LegalMoveGenerator.GenerateLegalMoves();
            int numPositions = 0;

            foreach (Move move in moves)
            {
                Board.MakeMove(move);
                numPositions += TestMoveGeneration(depth - 1);
                Board.UnmakeMove();
            }
            return numPositions;
        }
    }
}