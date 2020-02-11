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
    public int beatsPerBar;
    public float beatNum;
    public float dspSongTime;
    public float firstBeatOffset;
    public AudioSource musicSource;

    private bool alreadyRefreshedActivity = false;

    void Start()
    {
        //Load AudioSource attached to main camera (local) gameobject:
        musicSource = Camera.main.GetComponent<AudioSource>();
        secPerBeat = 60f / songBpm;
        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;
    }

    // Update is called once per frame
    void Update()
    {
        //Determine how many seconds has passed since song has started
        songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);

        //
        songPositionInBeats = songPosition / secPerBeat;
        //reminder: this is a float value and it starts at 0
        
        //Refresh between beats:
        if ((Mathf.Repeat(songPositionInBeats, 1) >= 0.5f) && !alreadyRefreshedActivity) {
            //Refresh activity of all Qbits
            
            alreadyRefreshedActivity = true;
        } else if (Mathf.Repeat(songPositionInBeats, 1) < 0.5f) {
            alreadyRefreshedActivity = false;
        }

        //Beat number per bar: If we are to interact with things every x beats
        //beatNum = Mathf.Repeat(songPositionInBeats, (beatsPerBar-1));

    }
}
