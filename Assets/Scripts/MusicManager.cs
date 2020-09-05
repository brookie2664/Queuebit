﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : NetworkBehaviour
{
    public static MusicManager musicManager {get; private set;}

    public GameObject gameState;

    public MusicData[] songs;
    [Min(0)]
    public float preBeatAssist = .5f;
    [Min(0)]
    public float postBeatAssist = .5f;

    private AudioSource source;
    public float beatLength {get; private set;}
    private bool playbackStarted;
    private float playbackStartTime;
    private float beatStartTime;
    public bool beatsStarted {get; private set;}
    private int lastBeatMode = 0;
    private int beatsToEnd = 0;
    public bool songEnded {get; private set;} = false; 

    public int GetRandomSong() {
        return new System.Random().Next(songs.Length);
    }

    [ClientRpc]
    public void RpcStartPlaying(int i) {
        MusicData song = songs[i];
        source.clip = song.clip;
        beatLength = 60f / song.bpm; 
        source.Play();
        playbackStarted = true;
        playbackStartTime = Time.time;
        beatStartTime = song.startDelay + playbackStartTime;
        Countdown.countdown.StartCountdown("Move in..", song.startDelay);
        beatsToEnd = song.totalPlayBeats;
    }

    void Awake() {
        musicManager = this;
    }

    void Start()
    {
        //Load AudioSource attached to main camera (local) gameobject:
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playbackStarted) {
            return;
        }
        source.volume = AudioSettings.MasterVolume * AudioSettings.MusicVolume;
        if (!beatsStarted) {
            beatsStarted = Time.time >= beatStartTime;
        }
        if (songEnded) {
            return;
        }

        float currTime = Time.time;
        float timeSinceBeatStart = currTime - beatStartTime;
        float percentTimeInCurrBeat = Util.nfmod(timeSinceBeatStart, beatLength) / beatLength;
        int currBeatMode;
        if (percentTimeInCurrBeat < (1 - preBeatAssist) / 2) {
            currBeatMode = 0;
        } else if (percentTimeInCurrBeat >= (1 - preBeatAssist) / 2 && percentTimeInCurrBeat < .5f) {
            currBeatMode = 1;
        } else if (percentTimeInCurrBeat < .5f + preBeatAssist / 2) {
            currBeatMode = 2;
        } else {
            currBeatMode = 0;
        }
        bool beatModeUpdated = false;
        if (currBeatMode != lastBeatMode) {
            lastBeatMode = currBeatMode;
            beatModeUpdated = true;
        }
        if (beatModeUpdated) {
            switch(currBeatMode) {
                case 0:
                    if (isServer && beatsStarted) {
                        gameState.GetComponent<GameState>().EndBeatUpdate();
                    }
                    break;
                case 2:
                    PlayerCameraController.cameraController.BeatFlash();
                    break;
            }
        }
        if (timeSinceBeatStart / beatLength >= beatsToEnd) {
            songEnded = true;
            if (isServer) {
                gameState.GetComponent<GameState>().EndGame();
            }
        }
    }
}
