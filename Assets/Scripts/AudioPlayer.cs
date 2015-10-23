using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour
{

    public AudioClip[] sounds;

    public int SQUARE = 0;
    public int CLICK = 1;
    public int WIN = 2;
    public int DRAW = 3;
    public int LOSE = 4;

    public AudioSource audiosource;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //play soundeffect clip
    public void playSound(AudioClip clip)
    {
        audiosource.PlayOneShot(clip, 1f);
    }
}
