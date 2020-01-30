using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductorComponent : MonoBehaviour
{
    // Start is called before the first frame update

    public float songBpm;
    public float secPerBeat;
    public float songPosition;
    public float songPositionInBeats;
    public float dspSongTime;
    public AudioSource musicSource;


    void Start()
    {
        //Load AudioSource attached to conductor gameobject:
        musicSource = GetComponent<AudioSource>();
        secPerBeat = 60f / songBpm;
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        //
        musicSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
    }
}
