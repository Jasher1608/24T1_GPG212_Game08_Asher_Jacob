namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BoardUI : MonoBehaviour
    {
        [SerializeField] private List<Sprite> pieceSprites = new List<Sprite>();

        private GameObject[,] pieceGameObjects = new GameObject[8, 8];

        void Start()
        {
            CreateVisualBoard();
        }

        void CreateVisualBoard()
        {
            // Assuming Board.Square has already been initialized
            for (int i = 0; i < 64; i++)
            {
                int pieceCode = Board.Square[i];
                int pieceType = pieceCode & 7; // Get the last three bits to determine the piece type
                int pieceColor = pieceCode & 24; // Get the fourth and fifth bits to determine the color

                if (pieceType != Piece.None)
                {
                    // Calculate the position based on the index
                    Vector3 position = new Vector2((i % 8) - 3.5f, (i / 8) - 3.5f);
                    // Instantiate the piece GameObject
                    pieceGameObjects[i % 8, i / 8] = InstantiatePieceAtPosition(pieceType, pieceColor, position);
                }
            }
        }

        GameObject InstantiatePieceAtPosition(int pieceType, int pieceColor, Vector2 position)
        {
            GameObject pieceObject = new GameObject($"Piece_{pieceType}_{pieceColor}");
            SpriteRenderer renderer = pieceObject.AddComponent<SpriteRenderer>();

            // Determine the sprite index
            int spriteIndex = pieceType - 1 + (pieceColor == Piece.White ? 0 : 6); // Adjust this based on your sprite ordering
            renderer.sprite = pieceSprites[spriteIndex];
            renderer.sortingOrder = 8;

            // Set the position of the piece
            pieceObject.transform.position = position;
            pieceObject.transform.localScale = new Vector2(0.35f, 0.35f); // Set the scale as needed

            // Set the chessboard as the parent
            pieceObject.transform.SetParent(transform, false);

            return pieceObject;
        }
    }
}