using UnityEngine;
using System.Collections;

//Manages the UI
public class UIManager : MonoBehaviour
{
    public Board board;
    public TextMesh textStatus;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //show current turn
        textStatus.text = (board.currentTurn == Board.TURN_X ? "X's turn to move" : "O's turn to move");
    }
}
