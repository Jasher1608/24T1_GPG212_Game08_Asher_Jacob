namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BoardUI : MonoBehaviour
    {
        [SerializeField] private List<Sprite> pieceSprites = new List<Sprite>();
        [SerializeField] private GameObject highlightPrefab;

        public GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private List<GameObject> highlightedSquares = new List<GameObject>();

        private const string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq";

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
    }
}