using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour
{

    public const int SQUARE_EMPTY = 0;
    public const int SQUARE_X = 1;
    public const int SQUARE_O = 2;

    public int posX, posY;
    public int status;

    public Board board;
    public Sprite squareemptySprite, squarexSprite, squareoSprite;

    // Use this for initialization
    void Start()
    {
        status = SQUARE_EMPTY;
        board = GameObject.Find("Board").GetComponent<Board>(); //TODO resolve hardcoded GameObject reference
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            case SQUARE_EMPTY:
            {
                transform.GetComponent<SpriteRenderer>().sprite = squareemptySprite;
                break;
            }
            case SQUARE_X:
            {
                transform.GetComponent<SpriteRenderer>().sprite = squarexSprite;
                break;
            }
            case SQUARE_O:
            {
                transform.GetComponent<SpriteRenderer>().sprite = squareoSprite;
                break;
            }
        }
    }

    void OnMouseDown()
    {
        board.selectSquare(posX, posY);
    }

}
