using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Placeholder Charger warning (until F08): a faint strip as wide as the Charger from its centre to the wall it
    /// will hit, and a solid bar that grows along the strip from the Charger. Both are 1 × 1 square sprites scaled
    /// and turned each frame. Draw order (set on the renderers) keeps them above the floor and under the enemies and
    /// the player. Shows only what it is told; the Charger decides.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChargerLaneView : MonoBehaviour
    {
        [Tooltip("The faint strip along the whole lane.")]
        [SerializeField] private SpriteRenderer strip;

        [Tooltip("The solid bar that grows along the strip during the warning.")]
        [SerializeField] private SpriteRenderer bar;

        private void Awake()
        {
            if (strip == null || bar == null)
            {
                Debug.LogError($"{nameof(ChargerLaneView)} on '{name}' needs its strip and bar sprites assigned.", this);
                enabled = false;
                return;
            }

            Hide();
        }

        /// <summary>Shows the lane.</summary>
        /// <param name="origin">The Charger's centre, where the strip and the bar start.</param>
        /// <param name="direction">Unit vector along the lane.</param>
        /// <param name="length">Distance from the Charger's centre to the wall.</param>
        /// <param name="width">Strip width (the Charger's width).</param>
        /// <param name="fill">How far the bar has grown, 0..1 (1 = at the wall).</param>
        public void Show(Vector2 origin, Vector2 direction, float length, float width, float fill)
        {
            if (!enabled)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Place(strip, origin, direction, rotation, Mathf.Max(0f, length), width);
            Place(bar, origin, direction, rotation, Mathf.Max(0f, length) * Mathf.Clamp01(fill), width);
        }

        public void Hide()
        {
            if (strip != null)
            {
                strip.enabled = false;
            }

            if (bar != null)
            {
                bar.enabled = false;
            }
        }

        private static void Place(SpriteRenderer sprite, Vector2 origin, Vector2 direction, Quaternion rotation, float length, float width)
        {
            Transform t = sprite.transform;
            Vector2 middle = origin + direction * (length * 0.5f);
            t.SetPositionAndRotation(new Vector3(middle.x, middle.y, t.position.z), rotation);

            // The sprite is 1 × 1 unit; its parents are never scaled or rotated, so local scale is world size.
            t.localScale = new Vector3(length, width, 1f);
            sprite.enabled = length > 0f;
        }
    }
}
