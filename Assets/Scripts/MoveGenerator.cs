using System.Collections.Generic;

namespace Chess
{
    using static PrecomputedMoveData;

    public static class MoveGenerator
    {
        public static List<Move> moves;

        private static int friendlyColour;
        private static int opponentColour;

        public static List<Move> GenerateMoves()
        {
            moves = new List<Move>();
            friendlyColour = Board.colourToMove;
            // Bad way of doing this, fix later
            if (friendlyColour == Piece.White)
            {
                opponentColour = Piece.Black;
            }
            else
            {
                opponentColour = Piece.White;
            }

            for (int startSquare = 0; startSquare < 64; startSquare++)
            {
                int piece = Board.square[startSquare];

                if (Piece.IsColour(piece, Board.colourToMove))
                {
                    if (Piece.IsSlidingPiece(piece))
                    {
                        GenerateSlidingMoves(startSquare, piece);
                    }
                }
            }

            return moves;
        }

        static void GenerateSlidingMoves(int startSquare, int piece)
        {
            // Change directions for rook and bishop
            int startDirIndex = (Piece.IsType(piece, Piece.Bishop)) ? 4 : 0;
            int endDirIndex = (Piece.IsType(piece, Piece.Rook)) ? 4 : 8;

            for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
            {
                for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
                {
                    int targetSquare = startSquare + directionOffsets[directionIndex] * (n + 1);
                    int pieceOnTargetSquare = Board.square[targetSquare];

                    // Blocked by piece of same colour, don't continue in this direction
                    if (Piece.IsColour(pieceOnTargetSquare, friendlyColour))
                    {
                        break;
                    }

                    moves.Add(new Move(startSquare, targetSquare));

                    // Can't move farther this direction after capturing piece
                    if (Piece.IsColour(pieceOnTargetSquare, opponentColour))
                    {
                        break;
                    }
                }
            }
        }
    }
}