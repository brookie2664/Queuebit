using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 target;
    public float smoothTime = 0.2f;
    public Vector3 offset = new Vector3(0, 0, -1);
    private Vector3 velocity = Vector3.zero;
    
    void FixedUpdate() {
        if (target != null) {
            Vector3 desiredPosition = target + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            gameObject.transform.position = smoothedPosition;
        }
    }

    public void setCameraTarget(Vector3 inputTarget) {
        target = inputTarget;
    }

    // public void turnCameraOn() {
    //     if (gameObject.Camera.enabled == false) {
    //         gameObject.Camera.enabled = true;
    //     }
    // }
}
