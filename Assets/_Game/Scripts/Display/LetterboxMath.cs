using UnityEngine;

namespace Dashblade.Display
{
    /// <summary>
    /// Works out the camera viewport rect that keeps a fixed aspect ratio inside any screen shape:
    /// bars top and bottom on narrower screens, bars left and right on wider ones.
    /// </summary>
    public static class LetterboxMath
    {
        public const float SixteenByNine = 16f / 9f;

        /// <summary>
        /// Returns a normalised viewport rect (0..1) centred on the screen with the target aspect ratio.
        /// Returns the full screen for invalid sizes or aspect values.
        /// </summary>
        public static Rect Viewport(float screenWidth, float screenHeight, float targetAspect)
        {
            if (screenWidth <= 0f || screenHeight <= 0f || targetAspect <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            float screenAspect = screenWidth / screenHeight;

            if (Mathf.Approximately(screenAspect, targetAspect))
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            if (screenAspect < targetAspect)
            {
                // Narrower than the target: full width, bars top and bottom.
                float height = screenAspect / targetAspect;
                return new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }

            // Wider than the target: full height, bars left and right.
            float width = targetAspect / screenAspect;
            return new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }
    }
}
