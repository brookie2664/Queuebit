using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    private LineRenderer line;
    private Vector3[] targets = new Vector3[4];
    private Vector3[] velocities = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};

    public float smoothTime = 0.2f;

    public void SetRender(int atkLevel) {
        Vector3[] directions = {Vector3.up, Vector3.left, Vector3.down, Vector3.right};
        int i = 0;
        foreach (Vector3 dir in directions) {
            targets[i] = dir * 1.9f + dir * 1.2f * atkLevel + Vector3.forward;
            i++;
        }
    }

    public void SetColor(int color) {
        Color newColor;
        switch (color) {
            case 1:
                newColor = Color.red;
                break;
            case 2:
                newColor = Color.green;
                break;
            case 3:
                newColor = Color.blue;
                break;
            default:
                newColor = Color.white;
                break;
        }
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(new GradientColorKey[]{new GradientColorKey(newColor, 0)}, new GradientAlphaKey[]{new GradientAlphaKey(.4f, 0)});
        line.colorGradient = colorGradient;
    }

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        for (int i = 0; i < targets.Length; i++) {
            Vector3 smoothedPosition = Vector3.SmoothDamp(line.GetPosition(i), targets[i], ref velocities[i], smoothTime);
            line.SetPosition(i, smoothedPosition);
        }
    }
}
