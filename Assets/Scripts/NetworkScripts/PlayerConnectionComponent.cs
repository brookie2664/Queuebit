using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnectionComponent : NetworkBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //PlayerConnectionObject is an always-on, invisible object that is spawned when a player connects to the game.
        //
        if (isLocalPlayer == false) {
            //object belongs to another player
            return;
        }

        Debug.Log("Spawning Player Unit");

        //Instantiate() only creates an object on the LOCAL COMPUTER
        //Even if it has a networkidentity, it will still not exist on the network (and therefore not on any other client)
        //Unless you use NetworkServer.Spawn().
        //NetworkServer.Spawn(PlayerUnitPrefab);
        CmdSpawnPlayerQbit();
        //CmdSpawnPlayerCamera();
        //Visual effects can be instantiated on local computer - that's fine.
        
        //Attach main camera to player

    }

    public GameObject PlayerUnitPrefab;
    //public GameObject PlayerCameraPrefab;
    public string PlayerName = "Anon";
    // Update is called once per frame
    void Update()
    {
        //Update runs on everyone's computer, wether or not they own this particular player object
        if (isLocalPlayer == false) {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            CmdSpawnPlayerQbit();
        }


    }

    //Commands:
    //Commands are special function that only get executed on the server

    [Command]
    void CmdSpawnPlayerQbit() {
        //Guaranteed to be on the server right now
        //Can only call command spawn for things you have authority over.
        GameObject localPlayerUnit = Instantiate(PlayerUnitPrefab);
        //GameObject localPlayerCamera = Instantiate(PlayerCameraPrefab);
        //localPlayerCamera.GetComponent<PlayerCameraController>().setCameraTarget(localPlayerUnit.transform);
        //go.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        //Now, the game object exists on the server
        //Propagate it to all clients (and also wire up the NetworkIdentity)
        NetworkServer.SpawnWithClientAuthority(localPlayerUnit, connectionToClient);

        //Connect main camera to local player: Done in Player Controller Script

        
    }

}
