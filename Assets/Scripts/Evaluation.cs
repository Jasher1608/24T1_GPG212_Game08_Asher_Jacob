namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static Board;

    public static class Evaluation
    {
        public const int pawnValue = 100;
        public const int knightValue = 300;
        public const int bishopValue = 320;
        public const int rookValue = 500;
        public const int queenValue = 900;

        public static int Evaluate()
        {
            int whiteEval = CountMaterial(Piece.White);
            int blackEval = CountMaterial(Piece.Black);

            int evaluation = whiteEval - blackEval;

            int perspective;
            if (colourToMove == Piece.White)
            {
                perspective = 1;
            }
            else
            {
                perspective = -1;
            }

            return evaluation * perspective;
        }

        static int CountMaterial(int colourIndex)
        {
            int material = 0;

            if (colourIndex == Piece.White)
            {
                material += whitePawnCount * pawnValue;
                material += whiteBishopCount * bishopValue;
                material += whiteKnightCount * knightValue;
                material += whiteRookCount * rookValue;
                material += whiteQueenCount * queenValue;
            }
            else
            {
                material += blackPawnCount * pawnValue;
                material += blackBishopCount * bishopValue;
                material += blackKnightCount * knightValue;
                material += blackRookCount * rookValue;
                material += blackQueenCount * queenValue;
            }

            return material;
        }
    }
}