using System;
using System.Collections.Generic;

public class State
{
    public int[][] boardState;
    public Point lastPos;
    public int stateResult;
    public int pieceNumber; //number of pieces placed on the board -- removes the need to count manually

    public State(int[][] prevBoardState, Point lastPos, int pieceNumber)
    {
        this.lastPos = lastPos;
        stateResult = Board.RESULT_NONE;
        this.pieceNumber = pieceNumber;

        if (prevBoardState != null) //no previous moves
        {
            boardState = Util.deepcloneArray(prevBoardState);
        }
        else
        {
            boardState = new int[Board.BOARD_SIZE][];
            for (int i = 0; i < boardState.Length; i++)
            {
                boardState[i] = new int[Board.BOARD_SIZE];
                for (int j = 0; j < boardState[i].Length; j++)
                {
                    boardState[i][j] = Square.SQUARE_EMPTY;
                }
            }
        }
    }
}
