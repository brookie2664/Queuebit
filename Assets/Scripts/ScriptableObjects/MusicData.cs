using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song", menuName = "ScriptableObjects/Music Data", order = 1)]
public class MusicData : ScriptableObject
{
    public AudioClip clip;

    [Min(0)]
    public int bpm;

    [Min(0)]
    public float startDelay = 0f;

    [Min(0)]
    public int totalPlayBeats;
}
