using Chess;
using UnityEngine;

public class DragAndDropPiece : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;
    private bool isDragging = false;
    private Transform selectedPiece;
    private Vector3 startingPosition;
    private int startingIndex;
    private float zCoordinate;
    private float squareSize = 1.0f;

    [SerializeField] private BoardUI boardUI;

    private void Awake()
    {
        mainCamera = Camera.main;
        zCoordinate = Mathf.Abs(mainCamera.transform.position.z - 1);
        boardUI = GameObject.FindWithTag("GameController").GetComponent<BoardUI>();
    }

    private void Update()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverPiece(mouseWorldPos))
            {
                isDragging = true;
                selectedPiece = transform;
                startingPosition = selectedPiece.position;
                startingIndex = PositionToIndex(startingPosition);
                selectedPiece.gameObject.GetComponent<SpriteRenderer>().sortingOrder += 1;
                offset = selectedPiece.position - mouseWorldPos;
            }
        }

        if (isDragging && selectedPiece)
        {
            selectedPiece.position = new Vector3(mouseWorldPos.x + offset.x, mouseWorldPos.y + offset.y, selectedPiece.position.z);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector3 newPosition = SnapToSquareCenter(selectedPiece.position);
            int newIndex = PositionToIndex(newPosition);
            TryCapturePieceAt(newIndex);
            UpdateBoardState(startingIndex, newIndex);
            selectedPiece.position = newPosition;
            selectedPiece.gameObject.GetComponent<SpriteRenderer>().sortingOrder -= 1;
            selectedPiece = null;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoordinate;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private bool IsMouseOverPiece(Vector3 position)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Bounds bounds = spriteRenderer.bounds;
        return bounds.Contains(new Vector3(position.x, position.y, transform.position.z));
    }

    private Vector3 SnapToSquareCenter(Vector3 originalPosition)
    {
        float offsetX = 0.5f * squareSize;
        float offsetY = 0.5f * squareSize;

        int xCount = Mathf.RoundToInt((originalPosition.x - offsetX) / squareSize);
        int yCount = Mathf.RoundToInt((originalPosition.y - offsetY) / squareSize);

        // If the position is outside the board boundaries, return the starting position
        if (xCount < -4 || xCount > 3 || yCount < -4 || yCount > 3)
        {
            return startingPosition;
        }

        Vector3 snappedPosition = new Vector3(
            xCount * squareSize + offsetX,
            yCount * squareSize + offsetY,
            originalPosition.z
        );

        return snappedPosition;
    }

    private void TryCapturePieceAt(int newIndex)
    {
        // If capturing another piece, remove it from the UI and game state
        if (Board.Square[newIndex] != Piece.None)
        {
            int x = newIndex % 8;
            int y = newIndex / 8;
            if (boardUI.pieceGameObjects[x, y] != null)
            {
                Destroy(boardUI.pieceGameObjects[x, y]);
                boardUI.pieceGameObjects[x, y] = null;
            }
        }
    }

    private void UpdateBoardState(int startingIndex, int targetIndex)
    {
        int movedPiece = Board.Square[startingIndex];
        Board.Square[targetIndex] = movedPiece;
        Board.Square[startingIndex] = Piece.None;

        for (int i = 0; i < 64; i++)
        {
            int x = i % 8;
            int y = i / 8;

            // If there's a discrepancy between the board state and the UI, update the UI
            if ((Board.Square[i] != Piece.None && boardUI.pieceGameObjects[x, y] == null) ||
                (Board.Square[i] == Piece.None && boardUI.pieceGameObjects[x, y] != null))
            {
                if (boardUI.pieceGameObjects[x, y] != null)
                {
                    Destroy(boardUI.pieceGameObjects[x, y]);
                    boardUI.pieceGameObjects[x, y] = null;
                }

                if (Board.Square[i] != Piece.None)
                {
                    Vector3 position = new Vector3(x - 3.5f, y - 3.5f, 0);
                    boardUI.pieceGameObjects[x, y] = boardUI.InstantiatePieceAtPosition(Board.Square[i] & 7, Board.Square[i] & 24, position);
                }
            }
        }
        // Special rules (en passant, castling, promotion)
    }

    private int PositionToIndex(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x + 3.5f);
        int y = Mathf.RoundToInt(position.y + 3.5f);
        return y * 8 + x;
    }
}
