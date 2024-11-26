using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPattern : MonoBehaviour
{
    const float wayPointRadiusGiz = 0.4f;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetPosWayPoint(i), wayPointRadiusGiz);
            Gizmos.DrawLine(GetPosWayPoint(i), GetPosWayPoint(j));
        }
    }
    public int GetNextIndex(int i)
    {
        if (i + 1 == transform.childCount)
        {
            return 0;
        }
        return i + 1;
    }
    public Vector3 GetPosWayPoint(int i)
    {
        return transform.GetChild(i).position;
    }
}