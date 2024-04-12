namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Board
    {
        public static int[] square = new int[64];

        public static int colourToMove = Piece.White;

        public static int enPassantTarget = -1;
        private static int oldEnPassantTarget = -1;

        public static bool pendingPromotion = false;
        private static bool oldPendingPromotion = false;

        public static bool CanCastleKingsideWhite = true;
        public static bool CanCastleQueensideWhite = true;
        public static bool CanCastleKingsideBlack = true;
        public static bool CanCastleQueensideBlack = true;
        private static bool oldCanCastleKingsideWhite = true;
        private static bool oldCanCastleQueensideWhite = true;
        private static bool oldCanCastleKingsideBlack = true;
        private static bool oldCanCastleQueensideBlack = true;

        public static int whitePawnCount;
        public static int whiteBishopCount;
        public static int whiteKnightCount;
        public static int whiteRookCount;
        public static int whiteQueenCount;
        public static int oldWhitePawnCount;
        public static int oldWhiteBishopCount;
        public static int oldWhiteKnightCount;
        public static int oldWhiteRookCount;
        public static int oldWhiteQueenCount;

        public static int blackPawnCount;
        public static int blackBishopCount;
        public static int blackKnightCount;
        public static int blackRookCount;
        public static int blackQueenCount;
        public static int oldBlackPawnCount;
        public static int oldBlackBishopCount;
        public static int oldBlackKnightCount;
        public static int oldBlackRookCount;
        public static int oldBlackQueenCount;

        public static Stack<MoveHistoryItem> moveHistory = new Stack<MoveHistoryItem>();

        public static void LoadPositionFromFen(string fen)
        {
            // Reset the board state
            Array.Fill(square, Piece.None);
            colourToMove = Piece.White;
            enPassantTarget = -1;
            CanCastleKingsideWhite = CanCastleQueensideWhite = false;
            CanCastleKingsideBlack = CanCastleQueensideBlack = false;

            var pieceTypeFromSymbol = new Dictionary<char, int>
            {
                ['k'] = Piece.King,
                ['p'] = Piece.Pawn,
                ['n'] = Piece.Knight,
                ['b'] = Piece.Bishop,
                ['r'] = Piece.Rook,
                ['q'] = Piece.Queen
            };

            string[] fenParts = fen.Split(' ');
            string fenBoard = fenParts[0];
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

            // Parse active color
            colourToMove = (fenParts[1] == "w") ? Piece.White : Piece.Black;

            // Parse castling availability
            CanCastleKingsideWhite = fenParts[2].Contains('K');
            CanCastleQueensideWhite = fenParts[2].Contains('Q');
            CanCastleKingsideBlack = fenParts[2].Contains('k');
            CanCastleQueensideBlack = fenParts[2].Contains('q');

            if (fen == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
            {
                CanCastleKingsideWhite = true;
                CanCastleQueensideWhite = true;
                CanCastleKingsideBlack = true;
                CanCastleQueensideBlack = true;
            }

            // Parse en passant target square
            if (fenParts[3] != "-")
            {
                enPassantTarget = (fenParts[3][1] - '1') * 8 + (fenParts[3][0] - 'a');
            }
        }

        public static void ToggleColourToMove(bool generateMoves = true)
        {
            colourToMove = (colourToMove == Piece.White) ? Piece.Black : Piece.White;
            if (generateMoves)
            {
                LegalMoveGenerator.legalMoves = LegalMoveGenerator.GenerateLegalMoves();
            }
            AIController.isCalculatingMove = false;
        }

        public static void ApplyMove(int startingIndex, int piece)
        {
            // If a king moves, disable castling on both sides for that color
            oldCanCastleKingsideWhite = CanCastleKingsideWhite;
            oldCanCastleQueensideWhite = CanCastleQueensideWhite;
            oldCanCastleKingsideBlack = CanCastleKingsideBlack;
            oldCanCastleQueensideBlack = CanCastleQueensideBlack;

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

        public static void MakeMove(Move move, bool generateMoves = false, bool toggleColour = true)
        {
            oldEnPassantTarget = enPassantTarget;
            oldPendingPromotion = pendingPromotion;

            oldWhitePawnCount = whitePawnCount;
            oldWhiteBishopCount = whiteBishopCount;
            oldWhiteKnightCount = whiteKnightCount;
            oldWhiteRookCount = whiteRookCount;
            oldWhiteQueenCount = whiteQueenCount;

            oldBlackPawnCount = blackPawnCount;
            oldBlackBishopCount = blackBishopCount;
            oldBlackKnightCount = blackKnightCount;
            oldBlackRookCount = blackRookCount;
            oldBlackQueenCount = blackQueenCount;

            int capturedPiece = square[move.TargetSquare];
            int movedPiece = square[move.StartSquare];

            // Apply the move
            square[move.TargetSquare] = movedPiece;
            square[move.StartSquare] = Piece.None;

            // Special handling for captures, specifically rook captures affecting castling rights
            if (capturedPiece != Piece.None)
            {
                // Check if a rook at its original position is captured
                if (move.TargetSquare == 0) CanCastleQueensideWhite = false;
                else if (move.TargetSquare == 7) CanCastleKingsideWhite = false;
                else if (move.TargetSquare == 56) CanCastleQueensideBlack = false;
                else if (move.TargetSquare == 63) CanCastleKingsideBlack = false;

                // Update piece count
                if (Piece.IsType(capturedPiece, Piece.Pawn))
                {
                    if (Piece.IsColour(capturedPiece, Piece.White))
                    {
                        whitePawnCount--;
                    }
                    else
                    {
                        blackPawnCount--;
                    }
                }
                else if (Piece.IsType(capturedPiece, Piece.Bishop))
                {
                    if (Piece.IsColour(capturedPiece, Piece.White))
                    {
                        whiteBishopCount--;
                    }
                    else
                    {
                        blackBishopCount--;
                    }
                }
                else if (Piece.IsType(capturedPiece, Piece.Knight))
                {
                    if (Piece.IsColour(capturedPiece, Piece.White))
                    {
                        whiteKnightCount--;
                    }
                    else
                    {
                        blackKnightCount--;
                    }
                }
                else if (Piece.IsType(capturedPiece, Piece.Rook))
                {
                    if (Piece.IsColour(capturedPiece, Piece.White))
                    {
                        whiteRookCount--;
                    }
                    else
                    {
                        blackRookCount--;
                    }
                }
                else if (Piece.IsType(capturedPiece, Piece.Queen))
                {
                    if (Piece.IsColour(capturedPiece, Piece.White))
                    {
                        whiteQueenCount--;
                    }
                    else
                    {
                        blackQueenCount--;
                    }
                }
            }

            // En passant capture
            if (move.IsEnPassant)
            {
                int pawnCaptureSquare = (move.StartSquare & ~0x07) + (move.TargetSquare & 0x07);
                capturedPiece = square[pawnCaptureSquare]; // Capture the piece
                square[pawnCaptureSquare] = Piece.None; // Remove the pawn that was captured en passant
            }
            // Castling move
            else if (move.IsCastling)
            {
                bool isKingside = move.TargetSquare > move.StartSquare;
                int rookStartSquare = isKingside ? move.TargetSquare + 1 : move.TargetSquare - 2;
                int rookTargetSquare = isKingside ? move.TargetSquare - 1 : move.TargetSquare + 1;

                square[rookTargetSquare] = square[rookStartSquare]; // Move the rook
                square[rookStartSquare] = Piece.None;
            }

            // Save the move to the history for undoing
            moveHistory.Push(new MoveHistoryItem(move, capturedPiece));

            // Update castling rights
            ApplyMove(move.StartSquare, movedPiece);


            if (toggleColour)
            {
                ToggleColourToMove(generateMoves: generateMoves); // Change the active player
            }
        }

        public static void UnmakeMove(bool toggleColour = true)
        {
            if (moveHistory.Count == 0) return;

            MoveHistoryItem lastMoveHistory = moveHistory.Pop();
            Move lastMove = lastMoveHistory.MoveMade;
            int capturedPiece = lastMoveHistory.CapturedPiece;

            // Reverse the move
            square[lastMove.StartSquare] = square[lastMove.TargetSquare];
            square[lastMove.TargetSquare] = capturedPiece; // Restore the captured piece, if any

            // Restore castling rights if they were changed by this move
            CanCastleKingsideWhite = oldCanCastleKingsideWhite;
            CanCastleQueensideWhite = oldCanCastleQueensideWhite;
            CanCastleKingsideBlack = oldCanCastleKingsideBlack;
            CanCastleQueensideBlack = oldCanCastleQueensideBlack;

            enPassantTarget = oldEnPassantTarget;
            pendingPromotion = oldPendingPromotion;

            whitePawnCount = oldWhitePawnCount;
            whiteBishopCount = oldWhiteBishopCount;
            whiteKnightCount = oldWhiteKnightCount;
            whiteRookCount = oldWhiteRookCount;
            whiteQueenCount = oldWhiteQueenCount;

            blackPawnCount = oldBlackPawnCount;
            blackBishopCount = oldBlackBishopCount;
            blackKnightCount = oldBlackKnightCount;
            blackRookCount = oldBlackRookCount;
            blackQueenCount = oldBlackQueenCount;

            if (toggleColour)
            {
                ToggleColourToMove(generateMoves: false); // Change the active player back
            }
        }

        public static void CountPieces()
        {
            whitePawnCount = 0;
            whiteBishopCount = 0;
            whiteKnightCount = 0;
            whiteRookCount = 0;
            whiteQueenCount = 0;
            blackPawnCount = 0;
            blackBishopCount = 0;
            blackKnightCount = 0;
            blackRookCount = 0;
            blackQueenCount = 0;

            foreach (int i in square)
            {
                if (Piece.IsType(i, Piece.Pawn))
                {
                    if (Piece.IsColour(i, Piece.White))
                    {
                        whitePawnCount++;
                    }
                    else
                    {
                        blackPawnCount++;
                    }
                }
                else if (Piece.IsType(i, Piece.Bishop))
                {
                    if (Piece.IsColour(i, Piece.White))
                    {
                        whiteBishopCount++;
                    }
                    else
                    {
                        blackBishopCount++;
                    }
                }
                else if (Piece.IsType(i, Piece.Knight))
                {
                    if (Piece.IsColour(i, Piece.White))
                    {
                        whiteKnightCount++;
                    }
                    else
                    {
                        blackKnightCount++;
                    }
                }
                else if (Piece.IsType(i, Piece.Rook))
                {
                    if (Piece.IsColour(i, Piece.White))
                    {
                        whiteRookCount++;
                    }
                    else
                    {
                        blackRookCount++;
                    }
                }
                else if (Piece.IsType(i, Piece.Queen))
                {
                    if (Piece.IsColour(i, Piece.White))
                    {
                        whiteQueenCount++;
                    }
                    else
                    {
                        blackQueenCount++;
                    }
                }
            }
        }
    }
}