namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using static MoveGenerator;

    public static class LegalMoveGenerator
    {
        public static List<Move> legalMoves = new List<Move>();

        public static List<Move> GenerateLegalMoves()
        {
            List<Move> pseudoLegalMoves = GenerateMoves();
            legalMoves = new List<Move>();

            foreach (Move moveToVerify in pseudoLegalMoves)
            {
                Board.MakeMove(moveToVerify);
                List<Move> opponentResponses = GenerateMoves();

                if (opponentResponses.Any(response => Board.square[response.TargetSquare] == (Piece.King | opponentColour)))
                {

                }
                else
                {
                    legalMoves.Add(moveToVerify);
                }

                Board.UnmakeMove();
            }

            return legalMoves;
        }
    }
}