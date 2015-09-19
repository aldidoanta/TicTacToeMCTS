using UnityEngine;
using System.Diagnostics;
using System.Collections;

//First attempt - Random movement
public class MCTSAI : MonoBehaviour
{
    [HideInInspector] public static int myTurn = Board.TURN_X;
    public Board board;
    public int iterationNumber;
    [HideInInspector] public TreeNode tn;

    // Use this for initialization
    void Start()
    {
        tn = new TreeNode(new State(board.boardState, board.lastPos, board.pieceNumber)); //create a new TreeNode
        iterationNumber = 50000;
    }

    // Update is called once per frame
    void Update()
    {
        if ((board.currentTurn == myTurn) && (board.result == Board.RESULT_NONE))
        {
            bool flag = false;
            if (tn.children.Count > 0)
            {
                foreach (TreeNode child in tn.children)
                {
                    if (child.state.lastPos.isEqual(board.lastPos))
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
                tn = new TreeNode(new State(board.boardState, board.lastPos, board.pieceNumber)); //create a new TreeNode
            }

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < iterationNumber; i++)
            {
                tn.iterateMCTS();
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            UnityEngine.Debug.Log("time elapsed for iterateMCTS() = " + elapsedMs + " ms");

            tn = tn.select();
            board.selectSquare(tn.state.lastPos.x, tn.state.lastPos.y);
        }
    }

}
