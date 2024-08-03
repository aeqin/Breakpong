using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmoMarker : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.red;
    [SerializeField] private float gizmoSize = 8f;

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}
