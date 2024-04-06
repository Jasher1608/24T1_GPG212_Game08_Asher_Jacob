namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BoardUI : MonoBehaviour
    {
        [SerializeField] private List<Sprite> pieceSprites = new List<Sprite>();
        [SerializeField] private GameObject highlightPrefab;

        public GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private List<GameObject> highlightedSquares = new List<GameObject>();

        [SerializeField] private GameObject promotionUI;

        private const string startingPosition = "r3k2r/8/4q3/8/2Q5/8/8/R3K2R";

        void Start()
        {
            Board.LoadPositionFromFen(startingPosition);
            CreateVisualBoard();
            MoveGenerator.moves = MoveGenerator.GenerateMoves();
        }

        void CreateVisualBoard()
        {
            for (int i = 0; i < 64; i++)
            {
                int pieceCode = Board.square[i];
                int pieceType = pieceCode & 7; // Get the first three bits to determine the piece type
                int pieceColor = pieceCode & 24; // Get the fourth and fifth bits to determine the color

                if (pieceType != Piece.None)
                {
                    Vector3 position = new Vector2((i % 8) - 3.5f, (i / 8) - 3.5f);
                    pieceGameObjects[i % 8, i / 8] = InstantiatePieceAtPosition(pieceType, pieceColor, position);
                }
            }
        }

        public GameObject InstantiatePieceAtPosition(int pieceType, int pieceColor, Vector2 position)
        {
            GameObject pieceObject = new GameObject($"Piece_{pieceType}_{pieceColor}");
            SpriteRenderer renderer = pieceObject.AddComponent<SpriteRenderer>();

            // Determine the sprite index
            int spriteIndex = pieceType - 1 + (pieceColor == Piece.White ? 0 : 6);
            renderer.sprite = pieceSprites[spriteIndex];
            renderer.sortingOrder = 8;

            // Set the position of the piece
            pieceObject.transform.position = position;
            pieceObject.transform.localScale = new Vector2(0.35f, 0.35f);

            // Set the chessboard as the parent
            pieceObject.transform.SetParent(transform, false);

            pieceObject.AddComponent<DragAndDropPiece>();

            return pieceObject;
        }

        public void HighlightLegalMoves(List<Move> legalMoves, int startingIndex)
        {
            ClearHighlightedSquares();

            foreach (Move move in legalMoves)
            {
                if (move.StartSquare == startingIndex)
                {
                    GameObject highlight = Instantiate(highlightPrefab, IndexToPosition(move.TargetSquare), Quaternion.identity, transform);
                    highlightedSquares.Add(highlight);
                }
            }
        }

        public void ClearHighlightedSquares()
        {
            foreach (GameObject highlight in highlightedSquares)
            {
                Destroy(highlight);
            }
            highlightedSquares.Clear();
        }

        private Vector3 IndexToPosition(int index)
        {
            int x = index % 8;
            int y = index / 8;
            return new Vector3(x - 3.5f, y - 3.5f, 0);
        }

        public void UpdateBoardState(int startingIndex, int targetIndex, Move move)
        {
            int movedPiece = Board.square[startingIndex];
            Board.square[targetIndex] = movedPiece;
            if (startingIndex != targetIndex)
            {
                Board.square[startingIndex] = Piece.None;

                // Modify target index for en passant
                if (move.IsEnPassant)
                {
                    if (Board.colourToMove == Piece.White)
                    {
                        targetIndex -= 8;
                    }
                    else if (Board.colourToMove == Piece.Black)
                    {
                        targetIndex += 8;
                    }

                    Board.square[targetIndex] = Piece.None;
                }
                // Modify target square for promotion
                else if (move.IsPromotion)
                {
                    if (move.PromotionPiece == Piece.Queen)
                    {
                        Board.square[targetIndex] = Board.colourToMove | Piece.Queen;
                    }
                    else if (move.PromotionPiece == Piece.Rook)
                    {
                        Board.square[targetIndex] = Board.colourToMove | Piece.Rook;
                    }
                    else if (move.PromotionPiece == Piece.Knight)
                    {
                        Board.square[targetIndex] = Board.colourToMove | Piece.Knight;
                    }
                    else if (move.PromotionPiece == Piece.Bishop)
                    {
                        Board.square[targetIndex] = Board.colourToMove | Piece.Bishop;
                    }
                }

                if (move.IsCastling)
                {
                    // Kingside
                    int kingsideOffset = 2;
                    int queensideOffset = -2;

                    if (targetIndex == startingIndex + kingsideOffset)
                    {
                        int rookStartIndex = startingIndex + 3;
                        int rookTargetIndex = targetIndex - 1;

                        Board.square[rookTargetIndex] = Board.square[rookStartIndex];
                        Board.square[rookStartIndex] = Piece.None;
                        UpdatePieceGameObject(rookStartIndex, rookTargetIndex);

                        // Toggle castling availability
                        if (Board.colourToMove == Piece.White)
                        {
                            Board.CanCastleKingsideWhite = false;
                            Board.CanCastleQueensideWhite = false;
                        }
                        else
                        {
                            Board.CanCastleKingsideBlack = false;
                            Board.CanCastleQueensideBlack = false;
                        }
                    }
                    // Queenside
                    else if (targetIndex == startingIndex + queensideOffset)
                    {
                        int rookStartIndex = startingIndex - 4;
                        int rookTargetIndex = targetIndex + 1;

                        Board.square[rookTargetIndex] = Board.square[rookStartIndex];
                        Board.square[rookStartIndex] = Piece.None;
                        UpdatePieceGameObject(rookStartIndex, rookTargetIndex);

                        // Toggle castling availability
                        if (Board.colourToMove == Piece.White)
                        {
                            Board.CanCastleKingsideWhite = false;
                            Board.CanCastleQueensideWhite = false;
                        }
                        else
                        {
                            Board.CanCastleKingsideBlack = false;
                            Board.CanCastleQueensideBlack = false;
                        }
                    }
                }
            }

            for (int i = 0; i < 64; i++)
            {
                int x = i % 8;
                int y = i / 8;

                // If there's a discrepancy between the board state and the UI, update the UI
                if ((Board.square[i] != Piece.None && pieceGameObjects[x, y] == null) ||
                    (Board.square[i] == Piece.None && pieceGameObjects[x, y] != null))
                {
                    if (pieceGameObjects[x, y] != null)
                    {
                        Destroy(pieceGameObjects[x, y]);
                        pieceGameObjects[x, y] = null;
                    }

                    if (Board.square[i] != Piece.None)
                    {
                        Vector3 position = new Vector3(x - 3.5f, y - 3.5f, 0);
                        pieceGameObjects[x, y] = InstantiatePieceAtPosition(Board.square[i] & 7, Board.square[i] & 24, position);
                    }
                }
            }
            // Special rules (en passant, castling, promotion)
            if (move.DoublePush)
            {
                if (Board.colourToMove == Piece.White)
                {
                    Board.enPassantTarget = targetIndex - 8;
                }
                else if (Board.colourToMove == Piece.Black)
                {
                    Board.enPassantTarget = targetIndex + 8;
                }
            }
            else
            {
                Board.enPassantTarget = -1;
            }

            Board.ApplyMove(startingIndex, movedPiece); // Disables castling flags
        }

        private void UpdatePieceGameObject(int startSquare, int targetSquare)
        {
            GameObject piece = pieceGameObjects[startSquare % 8, startSquare / 8];
            pieceGameObjects[targetSquare % 8, targetSquare / 8] = piece;
            pieceGameObjects[startSquare % 8, startSquare / 8] = null;

            if (piece != null)
            {
                piece.transform.position = IndexToPosition(targetSquare);
            }
        }

        public void ShowPromotionUI(int pawnSquare, int targetSquare, bool isWhite)
        {
            promotionUI.SetActive(true);

            var buttons = promotionUI.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                button.onClick.RemoveAllListeners();
            }

            buttons[0].onClick.AddListener(() => PromotePawn(pawnSquare, targetSquare, Piece.Queen, isWhite));
            buttons[1].onClick.AddListener(() => PromotePawn(pawnSquare, targetSquare, Piece.Rook, isWhite));
            buttons[2].onClick.AddListener(() => PromotePawn(pawnSquare, targetSquare, Piece.Knight, isWhite));
            buttons[3].onClick.AddListener(() => PromotePawn(pawnSquare, targetSquare, Piece.Bishop, isWhite));
        }

        private void PromotePawn(int pawnSquare, int targetSquare, int promotionPiece, bool isWhite)
        {
            Board.square[targetSquare] = (isWhite ? Piece.White : Piece.Black) | promotionPiece;
            Board.square[pawnSquare] = Piece.None;

            UpdateBoardState(pawnSquare, targetSquare, new Move(pawnSquare, targetSquare, isPromotion: true, promotionPiece: promotionPiece));

            promotionUI.SetActive(false);

            Board.pendingPromotion = false;

            Board.ToggleColourToMove();
        }
    }
}