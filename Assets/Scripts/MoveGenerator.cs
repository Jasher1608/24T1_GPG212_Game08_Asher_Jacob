using System.Collections.Generic;

namespace Chess
{
    using static PrecomputedMoveData;

    public static class MoveGenerator
    {
        public static List<Move> moves;

        public static int friendlyColour;
        public static int opponentColour;

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
                    else if (Piece.IsType(piece, Piece.King))
                    {
                        GenerateKingMoves(startSquare, piece);
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
                    // Ensure the move does not wrap around the board
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
                AddPawnMove(startSquare, forwardSquare, pawn);
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
                        AddPawnMove(startSquare, captureSquare, pawn);
                    }
                    else if (startSquare / 8 == enPassantRank && captureSquare == Board.enPassantTarget)
                    {
                        // En passant move
                        moves.Add(new Move(startSquare, captureSquare, isEnPassant: true));
                    }
                }
            }
        }

        private static void GenerateKingMoves(int startSquare, int piece)
        {
            foreach (int offset in kingOffsets)
            {
                int targetSquare = startSquare + offset;
                if (IsSquareOnBoard(targetSquare) && System.Math.Abs((targetSquare % 8) - (startSquare % 8)) <= 1)
                {
                    if (Piece.IsColour(Board.square[targetSquare], opponentColour) || Board.square[targetSquare] == Piece.None)
                    {
                        moves.Add(new Move(startSquare, targetSquare));
                    }
                }
            }

            // Castling conditions
            if ((Board.colourToMove == Piece.White && startSquare == 4) || (Board.colourToMove == Piece.Black && startSquare == 60))
            {
                // Kingside castling
                if (Board.colourToMove == Piece.White && Board.CanCastleKingsideWhite && IsCastlingPathClear(4, true) && !IsSquareAttacked(4, opponentColour) && !IsSquareAttacked(5, opponentColour) && !IsSquareAttacked(6, opponentColour))
                {
                    moves.Add(new Move(startSquare, startSquare + 2, isCastling: true));
                }
                else if (Board.colourToMove == Piece.Black && Board.CanCastleKingsideBlack && IsCastlingPathClear(60, true) && !IsSquareAttacked(60, opponentColour) && !IsSquareAttacked(61, opponentColour) && !IsSquareAttacked(62, opponentColour))
                {
                    moves.Add(new Move(startSquare, startSquare + 2, isCastling: true));
                }

                // Queenside castling
                if (Board.colourToMove == Piece.White && Board.CanCastleQueensideWhite && IsCastlingPathClear(4, false) && !IsSquareAttacked(4, opponentColour) && !IsSquareAttacked(3, opponentColour) && !IsSquareAttacked(2, opponentColour))
                {
                    moves.Add(new Move(startSquare, startSquare - 2, isCastling: true));
                }
                else if (Board.colourToMove == Piece.Black && Board.CanCastleQueensideBlack && IsCastlingPathClear(60, false) && !IsSquareAttacked(60, opponentColour) && !IsSquareAttacked(59, opponentColour) && !IsSquareAttacked(58, opponentColour))
                {
                    moves.Add(new Move(startSquare, startSquare - 2, isCastling: true));
                }
            }
        }

        private static void AddPawnMove(int startSquare, int targetSquare, int pawn)
        {
            // Add promotion moves or a regular pawn move
            int promotionRank = Piece.IsColour(pawn, Piece.White) ? 7 : 0;
            if (targetSquare / 8 == promotionRank)
            {
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

        private static bool IsSquareAttacked(int square, int attackerColour)
        {
            // Pawns
            int pawnDirection = attackerColour == Piece.White ? -1 : 1;
            int[] pawnOffsets = { -1, 1 };
            foreach (var offset in pawnOffsets)
            {
                int potentialPawnSquare = square + pawnDirection * 8 + offset;
                if (IsSquareOnBoard(potentialPawnSquare) &&
                    Board.square[potentialPawnSquare] == (Piece.Pawn | attackerColour))
                {
                    return true;
                }
            }

            // Knights
            foreach (var offset in knightOffsets)
            {
                int potentialKnightSquare = square + offset;
                if (IsSquareOnBoard(potentialKnightSquare) &&
                    Board.square[potentialKnightSquare] == (Piece.Knight | attackerColour))
                {
                    return true;
                }
            }

            // Bishops and Queens (diagonal attacks)
            foreach (var offset in bishopOffsets)
            {
                for (int n = 1; n < 8; n++)
                {
                    int potentialBishopSquare = square + n * offset;
                    if (!IsSquareOnBoard(potentialBishopSquare)) break;

                    if (Board.square[potentialBishopSquare] != Piece.None)
                    {
                        if (Board.square[potentialBishopSquare] == (Piece.Bishop | attackerColour) ||
                            Board.square[potentialBishopSquare] == (Piece.Queen | attackerColour))
                        {
                            return true;
                        }
                        break; // Block by other pieces
                    }
                }
            }

            // Rooks and Queens (straight attacks)
            foreach (var offset in rookOffsets)
            {
                for (int n = 1; n < 8; n++)
                {
                    int potentialRookSquare = square + n * offset;
                    if (!IsSquareOnBoard(potentialRookSquare)) break;

                    if (Board.square[potentialRookSquare] != Piece.None)
                    {
                        if (Board.square[potentialRookSquare] == (Piece.Rook | attackerColour) ||
                            Board.square[potentialRookSquare] == (Piece.Queen | attackerColour))
                        {
                            return true;
                        }
                        break; // Block by other pieces
                    }
                }
            }

            // Kings
            foreach (var offset in kingOffsets)
            {
                int potentialKingSquare = square + offset;
                if (IsSquareOnBoard(potentialKingSquare) &&
                    Board.square[potentialKingSquare] == (Piece.King | attackerColour))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCastlingPathClear(int kingStartSquare, bool isKingside)
        {
            int direction = isKingside ? 1 : -1;
            for (int offset = 1; offset <= 2; offset++)
            {
                int squareToCheck = kingStartSquare + direction * offset;
                if (Board.square[squareToCheck] != Piece.None || IsSquareAttacked(squareToCheck, opponentColour))
                {
                    return false;
                }
            }
            // Additional check for queenside castling path (square d1 or d8 for white and black respectively)
            if (!isKingside)
            {
                int queensideExtraSquare = kingStartSquare - 3;
                if (Board.square[queensideExtraSquare] != Piece.None || IsSquareAttacked(queensideExtraSquare, opponentColour))
                {
                    return false;
                }
            }
            return true;
        }
    }
}