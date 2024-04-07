namespace Chess
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Board
    {
        public static int[] square = new int[64];

        public static int colourToMove = Piece.White;

        public static int enPassantTarget = -1;

        public static bool pendingPromotion = false;

        public static bool CanCastleKingsideWhite = true;
        public static bool CanCastleQueensideWhite = true;
        public static bool CanCastleKingsideBlack = true;
        public static bool CanCastleQueensideBlack = true;
        private static bool oldCanCastleKingsideWhite = true;
        private static bool oldCanCastleQueensideWhite = true;
        private static bool oldCanCastleKingsideBlack = true;
        private static bool oldCanCastleQueensideBlack = true;

        public static Stack<MoveHistoryItem> moveHistory = new Stack<MoveHistoryItem>();


        public static void LoadPositionFromFen(string fen)
        {
            var pieceTypeFromSymbol = new Dictionary<char, int>
            {
                ['k'] = Piece.King,
                ['p'] = Piece.Pawn,
                ['n'] = Piece.Knight,
                ['b'] = Piece.Bishop,
                ['r'] = Piece.Rook,
                ['q'] = Piece.Queen
            };

            string fenBoard = fen.Split(' ')[0];
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
            int capturedPiece = square[move.TargetSquare];
            int movedPiece = square[move.StartSquare];

            // Apply the move
            square[move.TargetSquare] = movedPiece;
            square[move.StartSquare] = Piece.None;

            // Handle special moves
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

        public static void UnmakeMove()
        {
            if (moveHistory.Count == 0) return;

            MoveHistoryItem lastMoveHistory = moveHistory.Pop();
            Move lastMove = lastMoveHistory.MoveMade;
            int capturedPiece = lastMoveHistory.CapturedPiece;

            // Reverse the move
            square[lastMove.StartSquare] = square[lastMove.TargetSquare];
            square[lastMove.TargetSquare] = capturedPiece; // Restore the captured piece, if any

            // Handle reversing special moves
            if (lastMove.IsEnPassant)
            {
                int pawnCaptureSquare = (lastMove.TargetSquare & 0x07) + (lastMove.StartSquare & ~0x07);
                square[pawnCaptureSquare] = capturedPiece; // Restore the pawn captured en passant
                square[lastMove.TargetSquare] = Piece.None; // Clear the target square
            }
            else if (lastMove.IsCastling)
            {
                bool isKingside = lastMove.TargetSquare > lastMove.StartSquare;
                int rookStartSquare = isKingside ? lastMove.TargetSquare + 1 : lastMove.TargetSquare - 2;
                int rookTargetSquare = isKingside ? lastMove.TargetSquare - 1 : lastMove.TargetSquare + 1;

                square[rookStartSquare] = square[rookTargetSquare]; // Move the rook back
                square[rookTargetSquare] = Piece.None;
            }

            // Restore castling rights if they were changed by this move
            CanCastleKingsideWhite = oldCanCastleKingsideWhite;
            CanCastleQueensideWhite = oldCanCastleQueensideWhite;
            CanCastleKingsideBlack = oldCanCastleKingsideBlack;
            CanCastleQueensideBlack = oldCanCastleQueensideBlack;

            ToggleColourToMove(generateMoves: false); // Change the active player back
        }
    }

}