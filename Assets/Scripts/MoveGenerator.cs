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
                    else if (Piece.IsType(piece, Piece.Knight))
                    {
                        GenerateKnightMoves(startSquare, piece);
                    }
                    else if (Piece.IsType(piece, Piece.Pawn))
                    {
                        GeneratePawnMoves(startSquare, piece);
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

        private static void GenerateKnightMoves(int startSquare, int piece)
        {
            int startSquareRank = startSquare / 8;
            int startSquareFile = startSquare % 8;

            foreach (int offset in knightOffsets)
            {
                int targetSquare = startSquare + offset;
                int targetSquareRank = targetSquare / 8;
                int targetSquareFile = targetSquare % 8;

                // Check if the move stays within the bounds of the board
                if (targetSquare >= 0 && targetSquare < 64)
                {
                    // Ensure the move does not "wrap" around the board
                    if (System.Math.Abs(targetSquareFile - startSquareFile) <= 2 && System.Math.Abs(targetSquareRank - startSquareRank) <= 2)
                    {
                        // Check if the target square is not occupied by a friendly piece
                        if (!Piece.IsColour(Board.square[targetSquare], friendlyColour))
                        {
                            moves.Add(new Move(startSquare, targetSquare));
                        }
                    }
                }
            }
        }

        private static void GeneratePawnMoves(int startSquare, int pawn)
        {
            int direction = Piece.IsColour(pawn, Piece.White) ? 1 : -1;
            int startRank = Piece.IsColour(pawn, Piece.White) ? 1 : 6;
            int enPassantRank = Piece.IsColour(pawn, Piece.White) ? 4 : 3; // Rank from which a pawn can perform en passant

            // Single move forward
            int forwardSquare = startSquare + 8 * direction;
            if (IsSquareOnBoard(forwardSquare) && Board.square[forwardSquare] == Piece.None)
            {
                AddPawnMove(startSquare, forwardSquare, forwardSquare, pawn);
                // Double move forward
                int doubleForwardSquare = startSquare + 16 * direction;
                if ((startSquare / 8) == startRank && Board.square[doubleForwardSquare] == Piece.None)
                {
                    moves.Add(new Move(startSquare, doubleForwardSquare, doublePush: true));
                }
            }

            // Captures to the left and right, including en passant
            int[] captureOffsets = { -1, 1 }; // Relative file changes for captures
            foreach (int fileChange in captureOffsets)
            {
                int captureSquare = startSquare + direction * 8 + fileChange;
                bool isCaptureSquareOnBoard = captureSquare >= 0 && captureSquare < 64 &&
                    (startSquare % 8 + fileChange >= 0) && (startSquare % 8 + fileChange < 8);

                if (isCaptureSquareOnBoard)
                {
                    if (Piece.IsColour(Board.square[captureSquare], opponentColour))
                    {
                        AddPawnMove(startSquare, captureSquare, forwardSquare, pawn);
                    }
                    else if (startSquare / 8 == enPassantRank && captureSquare == Board.enPassantTarget)
                    {
                        // En passant move
                        moves.Add(new Move(startSquare, captureSquare, isEnPassant: true));
                    }
                }
            }
        }

        private static void AddPawnMove(int startSquare, int targetSquare, int forwardSquare, int pawn)
        {
            // Add promotion moves or a regular pawn move
            int promotionRank = Piece.IsColour(pawn, Piece.White) ? 7 : 0;
            if (targetSquare / 8 == promotionRank)
            {
                // Add all promotion options here if needed
                moves.Add(new Move(startSquare, targetSquare, isPromotion: true, promotionPiece: Piece.Queen));
                moves.Add(new Move(startSquare, targetSquare, isPromotion: true, promotionPiece: Piece.Rook));
                moves.Add(new Move(startSquare, targetSquare, isPromotion: true, promotionPiece: Piece.Knight));
                moves.Add(new Move(startSquare, targetSquare, isPromotion: true, promotionPiece: Piece.Bishop));
            }
            else
            {
                moves.Add(new Move(startSquare, targetSquare));
            }
        }

        private static bool IsSquareOnBoard(int square)
        {
            return square >= 0 && square < 64;
        }
    }
}