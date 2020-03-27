using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class PlayerConnectionComponent : NetworkBehaviour
{

    private AudioSource source;
    public AudioClip[] clips;
    public bool loopingAudio = false;
    public float bpm = 120;
    public double localBGMStartTime;
    public GameObject gameStateObject;
    private GameState gameState;

    private GameObject playerCamera;
    private GameObject renderer;
    private Vector2 playerPosition;
    private static KeyCode[] ALLOWED_INPUTS = {
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.Space
    };

    [ClientRpc]
    public void RpcUpdateCamera(Vector2 gridPos) {
        if (!isLocalPlayer) {
            return;
        }
        GameObject targetCell = renderer.GetComponent<GridRenderer>().GetCellRenderAt(gridPos);
        playerCamera.GetComponent<PlayerCameraController>().setCameraTarget(targetCell.transform.position);
    }

    //Clips reference:
    //0: Movement SFX
    //1: 
    [ClientRpc]
    public void RpcPlayOneShotOnClients(int id) {
        //if(isLocalPlayer) {
            source.PlayOneShot(clips[id]);
        //}
    }
    // [ClientRpc]
    // void RpcPlayClipAtPointOnClients(int id, Vector3 soundPosition) {
    //     GameState.source.PlayClipAtPoint(clips[id], soundPosition);
    //     //AudioClip[] must be the same on all clients.
    // }
    [ClientRpc]
    public void RpcSendBGMStartToClients(bool loopingAudio, int id) {
        //Starts audio
        if (loopingAudio == true) {
            //Do loop logic here?
        } 
        else {
            source.PlayOneShot(clips[id]);
        }
        localBGMStartTime = AudioSettings.dspTime;
    }

    [Command]
    public void CmdCreatePlayer(NetworkIdentity playerId) {
        gameState.CreatePlayer(playerId);
    }

    [Command]
    public void CmdSendInput(KeyCode input, NetworkIdentity playerId) {
        gameState.SendInput(input, playerId);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStateObject = GameObject.Find("GameState");
        gameState = gameStateObject.GetComponent<GameState>();
        source = this.GetComponent<AudioSource>();

        // PlayerConnectionObject is an always-on, invisible object that is spawned when a player connects to the game.
        //
        if (isLocalPlayer == false) {
            // object belongs to another player
            return;
        }

        playerCamera = gameState.localCamera;
        renderer = gameState.localRenderer;
        
        Debug.Log("Spawning Player Unit");

        CmdCreatePlayer(GetComponent<NetworkIdentity>());
        
    }

    public string PlayerName = "Anon";
    // Update is called once per frame
    void Update()
    {
        // Update runs on everyone's computer, wether or not they own this particular player object
        if (isLocalPlayer == false) {
            return;
        }
        foreach (KeyCode entry in ALLOWED_INPUTS) {
            if (Input.GetKeyDown(entry)) {
                CmdSendInput(entry, GetComponent<NetworkIdentity>());
                break;
            }
        }

    }

}
