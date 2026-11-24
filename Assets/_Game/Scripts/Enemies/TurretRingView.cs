using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Placeholder Turret warning (until F08): an orange ring outline (not a filled disc) around the Turret, drawn as
    /// a closed line. Its outer edge is the given width. Colour, line thickness, material and draw order are set on
    /// the LineRenderer; draw order keeps it above the floor and under the enemies and the player.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class TurretRingView : MonoBehaviour
    {
        private const int Segments = 64;

        private readonly Vector3[] points = new Vector3[Segments];
        private LineRenderer line;
        private float shownWidth = -1f;

        /// <summary>The outer width currently drawn, in units; 0 while hidden.</summary>
        public float ShownWidth => line != null && line.enabled ? shownWidth : 0f;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = Segments;
            Hide();
        }

        /// <summary>Shows the ring with its outer edge <paramref name="outerWidth"/> units across.</summary>
        public void Show(float outerWidth)
        {
            if (line == null)
            {
                return;
            }

            if (!Mathf.Approximately(outerWidth, shownWidth))
            {
                shownWidth = outerWidth;

                // The line is drawn centred on its path, so the path runs half a line thickness inside the outer edge.
                float radius = Mathf.Max(0f, (outerWidth - line.widthMultiplier) * 0.5f);
                for (int i = 0; i < Segments; i++)
                {
                    float angle = i * (2f * Mathf.PI / Segments);
                    points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                }

                line.SetPositions(points);
            }

            line.enabled = true;
        }

        public void Hide()
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }
    }
}
