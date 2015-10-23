using UnityEngine;
using UnityEngine.UI;
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
    public Text uctValue;

    public AudioPlayer audioplayer;

    // Use this for initialization
    void Start()
    {
        status = SQUARE_EMPTY;
    }

    // Update is called once per frame
    void Update()
    {
        //update square sprite
        switch (status)
        {
            case SQUARE_EMPTY:
            {
                transform.GetComponent<Image>().sprite = squareemptySprite;
                break;
            }
            case SQUARE_X:
            {
                transform.GetComponent<Image>().sprite = squarexSprite;
                break;
            }
            case SQUARE_O:
            {
                transform.GetComponent<Image>().sprite = squareoSprite;
                break;
            }
        }

        //update and set UCTValue visibility
        if (status == SQUARE_EMPTY)
        {
            if (mctsai.uctValues[posX][posY] == double.MinValue)
            {
                uctValue.text = "?"; //So the double.MinValue will not be shown
            }
            else
            {
                uctValue.text = string.Format("{0:0.00}", mctsai.uctValues[posX][posY]);
            }
        }
        else
        {
            uctValue.gameObject.SetActive(false);
        }

    }

    public void selectSquare()
    {
        if ((board.isStarted) && (!board.isLocked))
        {
            //play sound
            audioplayer.playSound(audioplayer.sounds[audioplayer.SQUARE]);
            board.selectSquare(posX, posY);
        }
    }

}
