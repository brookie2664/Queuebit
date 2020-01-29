using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Note: Update() here runs on all playerunits.
        if (hasAuthority == false) { //Do you have authority over this object?
            return;
        }
        
        //if IsActive:
        if (Input.GetKeyDown(KeyCode.Space)) {
            //Spacebar special
        } 
        else {
            if (Input.GetKeyDown(KeyCode.W)) {
                this.transform.Translate(0, 0, 1);
            }
            if (Input.GetKeyDown(KeyCode.A)) {
                this.transform.Translate(-1, 0, 0);
            }
            if (Input.GetKeyDown(KeyCode.S)) {
                this.transform.Translate(0, 0, -1);
            }
            if (Input.GetKeyDown(KeyCode.D)) {
                this.transform.Translate(1, 0, 0);
            }

        }

        //TODO: Decide if we want this self-destruct code
        if(Input.GetKeyDown(KeyCode.Backspace)) {
            Destroy(gameObject);
        }

    }
}
