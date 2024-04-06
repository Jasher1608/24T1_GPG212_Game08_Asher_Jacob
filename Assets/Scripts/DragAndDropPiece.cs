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

        if (Input.GetMouseButtonDown(0) && !Board.pendingPromotion)
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
        boardUI.HighlightLegalMoves(LegalMoveGenerator.legalMoves, startingIndex);
    }

    private void StopDragging()
    {
        isDragging = false;
        Vector3 newPosition = SnapToSquareCenter(selectedPiece.position);
        int newIndex = PositionToIndex(newPosition);

        Move attemptedMove = new Move(startingIndex, newIndex);
        Move attemptedMoveDouble = new Move(startingIndex, newIndex, doublePush: true);
        Move attemptedMoveEnPassant = new Move(startingIndex, newIndex, isEnPassant: true);
        Move attemptedMoveQueenPromotion = new Move(startingIndex, newIndex, isPromotion: true, promotionPiece: Piece.Queen);
        Move attemptedMoveRookPromotion = new Move(startingIndex, newIndex, isPromotion: true, promotionPiece: Piece.Rook);
        Move attemptedMoveKnightPromotion = new Move(startingIndex, newIndex, isPromotion: true, promotionPiece: Piece.Knight);
        Move attemptedMoveBishopPromotion = new Move(startingIndex, newIndex, isPromotion: true, promotionPiece: Piece.Bishop);
        Move attemptedMoveCastle = new Move(startingIndex, newIndex, isCastling: true);
        if (LegalMoveGenerator.legalMoves.Contains(attemptedMove))
        {
            TryCapturePieceAt(newIndex);
            boardUI.UpdateBoardState(startingIndex, newIndex, attemptedMove);
            Board.ToggleColourToMove();
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveDouble))
        {
            boardUI.UpdateBoardState(startingIndex, newIndex, attemptedMoveDouble);
            Board.ToggleColourToMove();
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveEnPassant))
        {
            CapturePieceAtEnPassant(newIndex);
            boardUI.UpdateBoardState(startingIndex, newIndex, attemptedMoveEnPassant);
            Board.ToggleColourToMove();
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveQueenPromotion))
        {
            Board.pendingPromotion = true;
            TryCapturePieceAt(newIndex);
            bool isWhite = Board.square[startingIndex] == (Piece.Pawn | Piece.White);
            boardUI.ShowPromotionUI(startingIndex, newIndex, isWhite);
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveRookPromotion))
        {
            Board.pendingPromotion = true;
            TryCapturePieceAt(newIndex);
            bool isWhite = Board.square[startingIndex] == (Piece.Pawn | Piece.White);
            boardUI.ShowPromotionUI(startingIndex, newIndex, isWhite);
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveKnightPromotion))
        {
            Board.pendingPromotion = true;
            TryCapturePieceAt(newIndex);
            bool isWhite = Board.square[startingIndex] == (Piece.Pawn | Piece.White);
            boardUI.ShowPromotionUI(startingIndex, newIndex, isWhite);
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveBishopPromotion))
        {
            Board.pendingPromotion = true;
            TryCapturePieceAt(newIndex);
            bool isWhite = Board.square[startingIndex] == (Piece.Pawn | Piece.White);
            boardUI.ShowPromotionUI(startingIndex, newIndex, isWhite);
        }
        else if (LegalMoveGenerator.legalMoves.Contains(attemptedMoveCastle))
        {
            boardUI.UpdateBoardState(startingIndex, newIndex, attemptedMoveCastle);
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

    private void CapturePieceAtEnPassant(int newIndex)
    {
        if (Board.colourToMove == Piece.White)
        {
            newIndex -= 8;
        }
        else if (Board.colourToMove == Piece.Black)
        {
            newIndex += 8;
        }

        int x = newIndex % 8;
        int y = newIndex / 8;

        if (boardUI.pieceGameObjects[x, y] != null)
        {
            Destroy(boardUI.pieceGameObjects[x, y]);
            boardUI.pieceGameObjects[x, y] = null;
        }
    }

    private int PositionToIndex(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x + 3.5f);
        int y = Mathf.RoundToInt(position.y + 3.5f);
        return y * 8 + x;
    }

    private bool IsPromotionMove(int startingIndex, int targetIndex)
    {
        int promotionRank = Piece.IsColour(Board.square[startingIndex] & 7, Piece.White) ? 7 : 0;

        if (targetIndex / 8 == promotionRank)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
