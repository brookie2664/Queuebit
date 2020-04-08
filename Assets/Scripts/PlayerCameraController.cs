using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public static PlayerCameraController cameraController {get; private set;}

    void Awake() {
        if (cameraController == null) {
            cameraController = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    public Vector3 target;
    [Range(0, 1)]
    public float beatFlashStrength = .3f;
    public float smoothTime = 0.2f;
    public Vector3 offset = new Vector3(0, 0, -1);
    private Vector3 velocity = Vector3.zero;
    
    private bool hasBeatFlash = false;
    private float timeSinceBeatFlash;

    void FixedUpdate() {
        if (target != null) {
            Vector3 desiredPosition = target + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            gameObject.transform.position = smoothedPosition;
        }

        if (hasBeatFlash) {
            timeSinceBeatFlash += Time.fixedDeltaTime;
            GetComponent<Camera>().backgroundColor = new Color(beatFlashStrength/(1+timeSinceBeatFlash), beatFlashStrength/(1+timeSinceBeatFlash), beatFlashStrength/(1+timeSinceBeatFlash));
        }
    }

    public void BeatFlash() {
        hasBeatFlash = true;
        timeSinceBeatFlash = 0;
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
