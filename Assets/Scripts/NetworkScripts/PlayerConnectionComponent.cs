using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnectionComponent : NetworkBehaviour
{

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

        //PlayerConnectionObject is an always-on, invisible object that is spawned when a player connects to the game.
        //
        if (isLocalPlayer == false) {
            //object belongs to another player
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
        //Update runs on everyone's computer, wether or not they own this particular player object
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
