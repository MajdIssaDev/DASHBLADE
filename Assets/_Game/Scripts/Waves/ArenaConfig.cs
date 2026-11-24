using System;
using UnityEngine;

namespace Dashblade.Waves
{
    /// <summary>
    /// The arena's hand-made pillar layouts (F05). The asset lives at Assets/_Game/Config/ArenaConfig.asset. Each run
    /// uses one layout, picked by <see cref="LayoutPicker"/>; every layout must follow the layout safety rules
    /// (<see cref="LayoutSafety"/>), which an Edit Mode test checks against this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ArenaConfig", menuName = "Dashblade/Arena Config")]
    public sealed class ArenaConfig : ScriptableObject
    {
        [Tooltip("Testing aid. None: each run picks A, B or C at random. A, B or C: every run uses that layout.")]
        [SerializeField] private ForceLayout forceLayout = ForceLayout.None;

        [Tooltip("Layout A: the centres of its 2 × 2 pillars.")]
        [SerializeField] private Vector2[] layoutA =
        {
            new Vector2(-8f, 4f), new Vector2(-8f, -4f), new Vector2(-1f, 0f),
            new Vector2(4f, 4f), new Vector2(4f, -4f), new Vector2(9f, 0f),
        };

        [Tooltip("Layout B: the centres of its 2 × 2 pillars.")]
        [SerializeField] private Vector2[] layoutB =
        {
            new Vector2(-10f, -5f), new Vector2(-5f, 2f), new Vector2(0f, 5f),
            new Vector2(2f, -2f), new Vector2(5f, 2f), new Vector2(10f, -2f),
        };

        [Tooltip("Layout C: the centres of its 2 × 2 pillars.")]
        [SerializeField] private Vector2[] layoutC =
        {
            new Vector2(-6f, 0f), new Vector2(-3f, 5f), new Vector2(-3f, -5f),
            new Vector2(3f, 5f), new Vector2(3f, -5f), new Vector2(6f, 0f),
        };

        public ForceLayout ForceLayout => forceLayout;

        /// <summary>The pillar centres of <paramref name="layout"/>, a fresh copy.</summary>
        public Vector2[] PillarCentres(ArenaLayout layout)
        {
            Vector2[] source;
            switch (layout)
            {
                case ArenaLayout.A:
                    source = layoutA;
                    break;
                case ArenaLayout.B:
                    source = layoutB;
                    break;
                case ArenaLayout.C:
                    source = layoutC;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout), layout, "Unknown layout.");
            }

            return source != null ? (Vector2[])source.Clone() : Array.Empty<Vector2>();
        }

        /// <summary>The pillar centres of <paramref name="layout"/> as plain values for <see cref="LayoutSafety"/>.</summary>
        public ArenaPoint[] PillarPoints(ArenaLayout layout)
        {
            Vector2[] centres = PillarCentres(layout);
            var points = new ArenaPoint[centres.Length];
            for (int i = 0; i < centres.Length; i++)
            {
                points[i] = new ArenaPoint(centres[i].x, centres[i].y);
            }

            return points;
        }
    }
}
