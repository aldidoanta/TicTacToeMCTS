using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour
{

    public const char SQUARE_EMPTY = '0';
    public const char SQUARE_X = '1';
    public const char SQUARE_O = '2';

    public int posX, posY;
    public char status;

    public Board board;
    public Sprite squareemptySprite, squarexSprite, squareoSprite;

    public MCTSAI mctsai;
    public TextMesh uctValue;

    // Use this for initialization
    void Start()
    {
        status = SQUARE_EMPTY;
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

        //update and set UCTValue visibility
        if (status == SQUARE_EMPTY)
        {
            uctValue.text = string.Format("{0:0.00}", mctsai.uctValues[posX][posY]);
            //uctValue.SetActive(true);
        }
        else
        {
            uctValue.gameObject.SetActive(false);
        }

    }

    void OnMouseDown()
    {
        board.selectSquare(posX, posY);
    }

}
