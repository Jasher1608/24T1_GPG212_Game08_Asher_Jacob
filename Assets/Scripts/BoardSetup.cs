namespace Chess
{
    using UnityEngine;
    public class BoardSetup : MonoBehaviour
    {
        public Color lightSquareColor = Color.white;
        public Color darkSquareColor = Color.gray;
        public float squareSize = 1.0f;

        void Start()
        {
            CreateGraphicalBoard();
        }

        void CreateGraphicalBoard()
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    bool isLightSquare = (file + rank) % 2 != 0;
                    Color squareColor = isLightSquare ? lightSquareColor : darkSquareColor;
                    var position = new Vector2(-3.5f + file, -3.5f + rank);

                    DrawSquare(squareColor, position);
                }
            }
        }

        void DrawSquare(Color squareColor, Vector3 position)
        {
            GameObject square = new GameObject("Square");

            SpriteRenderer squareRenderer = square.AddComponent<SpriteRenderer>();

            squareRenderer.color = squareColor;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            squareRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), squareSize);

            square.transform.position = position;

            square.transform.SetParent(transform, false);

            square.transform.localScale = new Vector3(1, 1, 1) * squareSize / this.transform.localScale.x;
        }
    }
}
