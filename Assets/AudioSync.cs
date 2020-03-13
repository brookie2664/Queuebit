using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioSync: NetworkBehaviour
{
    private AudioSource source;

    public AudioClip[] clips;

    void Start() {
        source = this.GetComponent<AudioSource>();
    }

    public void PlaySound(int id) {
        if (id >=0 && id < clips.Length) {
            CmdSendServerSoundID(id);
        }
    }

    [Command]
    void CmdSendServerSoundID(int id) {
        //Commands server to do something
        RpcSendSoundIDToClients(id);
    }

    [ClientRpc]
    void RpcSendSoundIDToClients(int id) {
        source.PlayOneShot(clips[id]);
        //AudioClip[] must be the same on all clients.
    }

    [ClientRpc]
    void RpcSendBGMStartToClients() {
        //
    }

}

