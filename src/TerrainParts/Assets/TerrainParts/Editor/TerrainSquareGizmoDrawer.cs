using UnityEditor;
using UnityEngine;

namespace TerrainParts.Editor
{
    public static class TerrainSquareGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        public static void DrawGizmo(TerrainSquare terrainSquare, GizmoType _)
        {
            Gizmos.DrawIcon(terrainSquare.transform.position, "TerrainParts/TerrainSquareIcon.png", true);
        }
    }
}
