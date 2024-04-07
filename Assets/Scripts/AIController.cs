namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AIController : MonoBehaviour
    {
        public bool isAIWhite = false;
        public bool isAIBothInspector = false;
        public static bool isAIBoth = false;
        public static int aiColour = Piece.Black;
        public static bool isCalculatingMove = false;
        public static Move aiMove;

        [SerializeField] private BoardUI boardUI;

        private void Start()
        {
            if (isAIWhite)
            {
                aiColour = Piece.None;
            }
            else
            {
                aiColour = Piece.None;
            }

            isAIBoth = isAIBothInspector;

            boardUI = GameObject.FindWithTag("GameController").GetComponent<BoardUI>();
        }

        private void Update()
        {
            if (((aiColour == Board.colourToMove) || isAIBoth) && !isCalculatingMove)
            {
                isCalculatingMove = true;
                aiMove = Move.ChooseComputerMove();
                Debug.Log(aiMove.StartSquare);
                Debug.Log(aiMove.TargetSquare);

                if ((Board.square[aiMove.TargetSquare] != Piece.None) && aiMove.TargetSquare != aiMove.StartSquare)
                {
                    int x = aiMove.TargetSquare % 8;
                    int y = aiMove.TargetSquare / 8;
                    if (boardUI.pieceGameObjects[x, y] != null)
                    {
                        Destroy(boardUI.pieceGameObjects[x, y]);
                        boardUI.pieceGameObjects[x, y] = null;
                    }
                }

                if (aiMove.IsEnPassant)
                {
                    int newIndex = aiMove.TargetSquare;
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

                boardUI.UpdateBoardState(aiMove.StartSquare, aiMove.TargetSquare, aiMove);
                Board.ToggleColourToMove();
            }
        }
    }
}