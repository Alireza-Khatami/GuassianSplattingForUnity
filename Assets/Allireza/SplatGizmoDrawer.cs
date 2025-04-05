using UnityEngine;

public class SplatGizmoDrawer : MonoBehaviour
{
    public SplatData SplatData;
    public int MaxPoints = 1000;
    public float Radius = 0.05f;
    public float scale = 1;
    private void OnDrawGizmos()
    {
        if (!SplatData) return;

        int count = SplatData.Count;
        int step = Mathf.Max(1, count / MaxPoints);
        //Debug.Log(count);
        Debug.Log(SplatData.debugLog);

        for (int i = 0; i < count; i += step)
        {
            var position = transform.TransformPoint(SplatData.Positions[i])* scale;
            var color = SplatData.Colors[i];
            Gizmos.color = color;
            Gizmos.DrawSphere(position, Radius);
        }
    }
}