using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    private LineRenderer line;
    private Vector3[] positions;
    private Vector3[] targets;
    private Vector3[] velocities;

    private int weapon;
    public float smoothTime = 0.2f;

    private void SimplifyVertices() {
        if (line.positionCount == 0) return;
        List<Vector3> positions = new List<Vector3>();
        Vector3 lastGoodPos = line.GetPosition(line.positionCount - 1);
        for (int i = 0; i < line.positionCount; i++) {
            Vector3 curPos = line.GetPosition(i);
            if ((curPos - lastGoodPos).magnitude >= .1) {
                lastGoodPos = curPos;
                positions.Add(curPos);
            }
        }
        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());
    }

    private Vector3[] GetPoints(int atkLevel, Vector2 direction) {
        direction.y = -direction.y;
        Vector3[] output;
        Vector3[] directions3 = {Vector3.up, Vector3.right, Vector3.down, Vector3.left};
        Vector2[] directions2 = {Vector2.up, Vector2.right, Vector2.down, Vector2.left};
        int pointsAdded = 0;
        switch (weapon) {
            case 0: // Weapon is splash
                output = new Vector3[4];
                foreach (Vector3 dir in directions3) {
                    output[pointsAdded] = atkLevel > 0 ? dir * .7f + dir * GridRenderer.CELL_SEPARATION * atkLevel + Vector3.forward : Vector3.zero;
                    pointsAdded++;
                }
                break;
            case 1: // Weapon is sniper
                output = new Vector3[8];
                if (atkLevel == 0) {
                    pointsAdded = 8;
                } else {
                    for (; pointsAdded < 8; pointsAdded++) {
                        Vector2 point = 1.9f * directions2[((pointsAdded + 1) % 8) / 2];
                        for (int i = pointsAdded; i < pointsAdded + 3; i++) {
                            if (directions2[(i % 8) / 2] == direction) {
                                point += GridRenderer.CELL_SEPARATION * (atkLevel - 1) * direction;
                                break;
                            }
                        }
                        output[pointsAdded] = new Vector3(point.x, point.y, 1); 
                    }
                }
                break;
            default:
                output = new Vector3[0];
                break;
        }
        return output;
    }

    public void AdjustRender(int atkLevel, Vector2 direction) {
        Vector3[] points = GetPoints(atkLevel, direction);
        targets = points;
    }

    public void SetWeapon(int weapon, int atkLevel, Vector2 direction) {
        this.weapon = weapon;
        Vector3[] points = GetPoints(atkLevel, direction);
        line.positionCount = points.Length;
        targets = points;
        positions = (Vector3[]) points.Clone();
        line.SetPositions(points);
        velocities = new Vector3[points.Length];
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
        line.positionCount = 4;
        line.SetPositions(new Vector3[4]);
        targets = new Vector3[4];
        velocities = new Vector3[4];
        positions = new Vector3[4];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        Vector3[] points = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++) {
            Vector3 smoothedPosition = Vector3.SmoothDamp(positions[i], targets[i], ref velocities[i], smoothTime);
            points[i] = smoothedPosition;
        }
        positions = points;
        line.positionCount = positions.Length;
        line.SetPositions(points);
        SimplifyVertices();
    }
}
