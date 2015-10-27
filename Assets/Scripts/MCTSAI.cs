using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class MCTSAI : MonoBehaviour
{
    public static char myTurn = Board.TURN_X;
    public Board board;
    public int iterationNumber;
    [HideInInspector] public TreeNode tn;
    [HideInInspector] public double[][] uctValues;

    // Use this for initialization
    void Start()
    {
        initAI();
    }

    // Update is called once per frame
    void Update()
    {
        if (board.isStarted)
        {
            if ((board.currentTurn == myTurn) && (board.result == Board.RESULT_NONE))
            {
                bool flag = false;
                if (tn.children.Count > 0)
                {
                    foreach (TreeNode child in tn.children)
                    {
                        if ((child.state.lastPos.isEqual(board.lastPos))
                            && (child.state.lastOPos.isEqual(board.lastOPos)))
                        {
                            tn = child; //use the child as current tree
                            flag = true;
                            break;
                        }
                    }
                    if (!flag) UnityEngine.Debug.Log("unreachable code");

                }
                else
                {
                    tn = new TreeNode(new State(board.boardState, board.currentTurn, board.lastPos, board.lastOPos, board.pieceNumber)); //create a new TreeNode
                }

                var watch = Stopwatch.StartNew();
                for (int i = 0; i < iterationNumber; i++)
                {
                    tn.iterateMCTS();
                }
                watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //UnityEngine.Debug.Log("time elapsed for iterateMCTS() = " + elapsedMs + " ms");

                tn = tn.select();

                //shows uctValue for each possible next move taken by the opponent
                updateUCTValues();

                if (myTurn == Board.TURN_X)
                {
                    board.selectSquare(tn.state.lastPos.x, tn.state.lastPos.y);
                }
                else //myTurn == Board.TURN_O
                {
                    board.selectSquare(tn.state.lastOPos.x, tn.state.lastOPos.y);
                }
            }
        }
    }

    public void initAI()
    {
        tn = new TreeNode(new State(board.boardState, board.currentTurn, board.lastPos, board.lastOPos, board.pieceNumber)); //create a new TreeNode

        uctValues = new double[Board.BOARD_SIZE][];
        for (int i = 0; i < uctValues.Length; i++)
        {
            uctValues[i] = new double[Board.BOARD_SIZE];
            for (int j = 0; j < uctValues[i].Length; j++)
            {
                uctValues[i][j] = double.MinValue;
            }
        }
    }

    //Shows UCTValue for each child. A child is one of the possible moves for the opponent.
    void updateUCTValues()
    {
        foreach (TreeNode child in tn.children)
        {
            int lastPosX = myTurn == Board.TURN_X ? child.state.lastOPos.x : child.state.lastPos.x;
            int lastPosY = myTurn == Board.TURN_X ? child.state.lastOPos.y : child.state.lastPos.y;
            uctValues[lastPosX][lastPosY] = child.uctValue;
        }
    }
}
