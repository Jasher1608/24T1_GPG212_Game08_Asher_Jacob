namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;

    public struct MoveHistoryItem
    {
        public Move MoveMade { get; }
        public int CapturedPiece { get; }

        public MoveHistoryItem(Move moveMade, int capturedPiece)
        {
            MoveMade = moveMade;
            CapturedPiece = capturedPiece;
        }
    }
}