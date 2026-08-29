using UnityEngine;

#if UNITY_EDITOR
namespace Mirage.Visibility.SpatialHash.EditorScripts
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    [CustomEditor(typeof(SpatialHashSystem))]
    public class SpatialHashSystemEditor : Editor
    {
        const PrimitiveBoundsHandle.Axes BoundsAxis = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;

        BoxBoundsHandle boundsHandle = new BoxBoundsHandle() { axes = BoundsAxis };

        // cache array for drawing handle
        readonly Vector3[] verts = new Vector3[4];

        readonly List<Vector3> playerPositions = new List<Vector3>();

        public void OnSceneGUI()
        {
            var system = target as SpatialHashSystem;
            var bounds = new Rect(system.Bounds.center.ToXZ(), system.Bounds.size.ToXZ());
            Vector2Int points = system.GridCount;
            GetPlayerPos(system);

            DrawGrid(bounds, points, playerPositions);
            DrawBoundsHandle(system);
        }

        private void GetPlayerPos(SpatialHashSystem system)
        {
            playerPositions.Clear();

            NetworkServer server = system.Server;
            if (server != null && server.Active)
            {
                foreach (INetworkPlayer player in server.Players)
                {
                    if (player.HasCharacter)
                    {
                        NetworkIdentity character = player.Identity;
                        playerPositions.Add(character.transform.position);
                    }
                }

                // return here, dont need to check client if we are server
                return;
            }

            NetworkClient client = system.GetComponent<NetworkClient>();
            if (client != null && client.Active)
            {
                if (client.Player != null && client.Player.HasCharacter)
                {
                    NetworkIdentity character = client.Player.Identity;
                    playerPositions.Add(character.transform.position);
                    return;

                }
            }
        }

        private void DrawGrid(Rect bounds, Vector2Int points, List<Vector3> players)
        {
            Vector2 offset = bounds.center - bounds.size;
            Vector2 size = bounds.size;

            var gridSize = new Vector2(size.x / points.x, size.y / points.y);
            Vector2 halfGrid = gridSize / 2;

            bool colorFlip = false;
            for (int i = 0; i < points.x; i++)
            {
                for (int j = 0; j < points.y; j++)
                {
                    float x = i * gridSize.x;
                    float y = j * gridSize.y;

                    // center of 1 gridPoint
                    Vector2 pos2d = new Vector2(x, y) + halfGrid + offset;
                    Vector3 pos = pos2d.FromXZ();

                    // +- halfGrid to get corners
                    verts[0] = pos + new Vector3(-halfGrid.x, 0, -halfGrid.y);
                    verts[1] = pos + new Vector3(-halfGrid.x, 0, +halfGrid.y);
                    verts[2] = pos + new Vector3(+halfGrid.x, 0, +halfGrid.y);
                    verts[3] = pos + new Vector3(+halfGrid.x, 0, -halfGrid.y);

                    Color color = Color.gray * (colorFlip ? 1 : 0.7f);

                    foreach (Vector3 player in players)
                    {
                        var gridRect = new Rect(x + offset.x, y + offset.y, gridSize.x, gridSize.y);
                        if (gridRect.Contains(player.ToXZ()))
                        {
                            // not too red
                            color = Color.red * 0.5f;
                            break;
                        }
                    }

                    // transparent
                    color.a = 0.6f;
                    Handles.DrawSolidRectangleWithOutline(verts, color, Color.black);
                    colorFlip = !colorFlip;
                }
                // if even we need to re-flip so that color is samme as last one, but will be different than above it
                if (points.y % 2 == 0)
                    colorFlip = !colorFlip;
            }
        }

        private void DrawBoundsHandle(SpatialHashSystem system)
        {
            boundsHandle.center = system.Bounds.center;
            boundsHandle.size = system.Bounds.size;
            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(system, "Change Bounds");
                system.Bounds.center = boundsHandle.center;
                system.Bounds.size = boundsHandle.size;
            }
        }
    }
}
#endif
