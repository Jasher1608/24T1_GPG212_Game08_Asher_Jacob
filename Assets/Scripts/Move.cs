using Unity.VisualScripting;

namespace Chess
{
    public readonly struct Move
    {
        public int StartSquare { get; }
        public int TargetSquare { get; }
        public bool DoublePush { get; }
        public bool IsEnPassant { get; }
        public bool IsPromotion { get; }
        public int PromotionPiece { get; }

        // Constructor for regular moves
        public Move(int startSquare, int targetSquare)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            DoublePush = false;
            IsEnPassant = false;
            IsPromotion = false;
            PromotionPiece = Piece.None;
        }

        // Constructor for special moves (en passant, promotion)
        public Move(int startSquare, int targetSquare, bool doublePush = false, bool isEnPassant = false, bool isPromotion = false, int promotionPiece = Piece.None)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            DoublePush = doublePush;
            IsEnPassant = isEnPassant;
            IsPromotion = isPromotion;
            PromotionPiece = promotionPiece;
        }

        // Implementing equality methods
        public override bool Equals(object obj)
        {
            if (obj is Move other)
            {
                return StartSquare == other.StartSquare && TargetSquare == other.TargetSquare && DoublePush == other.DoublePush &&
                       IsEnPassant == other.IsEnPassant && IsPromotion == other.IsPromotion &&
                       PromotionPiece == other.PromotionPiece;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + StartSquare.GetHashCode();
            hash = hash * 31 + TargetSquare.GetHashCode();
            hash = hash * 31 + DoublePush.GetHashCode();
            hash = hash * 31 + IsEnPassant.GetHashCode();
            hash = hash * 31 + IsPromotion.GetHashCode();
            hash = hash * 31 + PromotionPiece.GetHashCode();
            return hash;
        }

        public static bool operator ==(Move left, Move right) => left.Equals(right);
        public static bool operator !=(Move left, Move right) => !(left == right);
    }
}
