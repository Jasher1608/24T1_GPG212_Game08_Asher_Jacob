using System.Collections.Generic;

namespace Chess
{
    public class MoveGenerator
    {
        List<Move> moves;

        public List<Move> GenerateMoves()
        {
            moves = new List<Move>();

            for (int startSquare = 0; startSquare < 64; startSquare++)
            {
                int piece = Board.square[startSquare];

                if (Piece.IsColour(piece, Board.colourToMove))
                {
                    if (Piece.IsSlidingPiece(piece))
                    {
                        //GenerateSlidingMoves(startSquare, piece);
                    }
                }
            }

            return moves;
        }
    }
}