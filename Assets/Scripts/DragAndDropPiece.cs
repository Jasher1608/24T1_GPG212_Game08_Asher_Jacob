using UnityEngine;

public class DragAndDropPiece : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;
    private bool isDragging = false;
    private Transform selectedPiece;
    private float zCoordinate;
    private float squareSize = 1.0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        zCoordinate = Mathf.Abs(mainCamera.transform.position.z - 1);
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
            selectedPiece.position = SnapToSquareCenter(selectedPiece.position);
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

        Vector3 snappedPosition = new Vector3(
            xCount * squareSize + offsetX,
            yCount * squareSize + offsetY,
            originalPosition.z
        );

        return snappedPosition;
    }
}
