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
                startingPosition = transform.position;
                startingIndex = PositionToIndex(startingPosition);
                if (IsCorrectColourToMove())
                {
                    StartDragging(mouseWorldPos);
                }
            }
        }

        if (isDragging && selectedPiece)
        {
            selectedPiece.position = new Vector3(mouseWorldPos.x + offset.x, mouseWorldPos.y + offset.y, selectedPiece.position.z);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    private void StartDragging(Vector3 mouseWorldPos)
    {
        isDragging = true;
        selectedPiece = transform;
        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sortingOrder += 1;
        offset = selectedPiece.position - mouseWorldPos;
        // Highlight legal moves
        boardUI.HighlightLegalMoves(MoveGenerator.moves, startingIndex);
    }

    private void StopDragging()
    {
        isDragging = false;
        Vector3 newPosition = SnapToSquareCenter(selectedPiece.position);
        int newIndex = PositionToIndex(newPosition);

        Move attemptedMove = new Move(startingIndex, newIndex);
        if (MoveGenerator.moves.Contains(attemptedMove))
        {
            TryCapturePieceAt(newIndex);
            UpdateBoardState(startingIndex, newIndex);
            Board.ToggleColourToMove();
        }
        else
        {
            selectedPiece.position = startingPosition;
            Debug.Log("Illegal move");
        }

        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sortingOrder -= 1;
        selectedPiece = null;
        boardUI.ClearHighlightedSquares();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoordinate;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private bool IsMouseOverPiece(Vector3 mousePosition)
    {
        SpriteRenderer mySpriteRenderer = GetComponent<SpriteRenderer>();
        Bounds myBounds = mySpriteRenderer.bounds;
        if (!myBounds.Contains(new Vector3(mousePosition.x, mousePosition.y, transform.position.z)))
        {
            return false;
        }

        var allPieces = FindObjectsOfType<DragAndDropPiece>();

        float minDistance = float.MaxValue;
        DragAndDropPiece closestPiece = null;

        foreach (var piece in allPieces)
        {
            float distance = Vector3.Distance(mousePosition, piece.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPiece = piece;
            }
        }

        return closestPiece == this;
    }

    private bool IsCorrectColourToMove()
    {
        int piece = Board.square[startingIndex];
        return (piece & (Piece.White | Piece.Black)) == Board.colourToMove;
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
        if ((Board.square[newIndex] != Piece.None) && newIndex != startingIndex)
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
        int movedPiece = Board.square[startingIndex];
        Board.square[targetIndex] = movedPiece;
        if (startingIndex != targetIndex)
        {
            Board.square[startingIndex] = Piece.None;
        }

        for (int i = 0; i < 64; i++)
        {
            int x = i % 8;
            int y = i / 8;

            // If there's a discrepancy between the board state and the UI, update the UI
            if ((Board.square[i] != Piece.None && boardUI.pieceGameObjects[x, y] == null) ||
                (Board.square[i] == Piece.None && boardUI.pieceGameObjects[x, y] != null))
            {
                if (boardUI.pieceGameObjects[x, y] != null)
                {
                    Destroy(boardUI.pieceGameObjects[x, y]);
                    boardUI.pieceGameObjects[x, y] = null;
                }

                if (Board.square[i] != Piece.None)
                {
                    Vector3 position = new Vector3(x - 3.5f, y - 3.5f, 0);
                    boardUI.pieceGameObjects[x, y] = boardUI.InstantiatePieceAtPosition(Board.square[i] & 7, Board.square[i] & 24, position);
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
