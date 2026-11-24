using UnityEngine;

namespace Dashblade.Display
{
    /// <summary>
    /// Thin scene wrapper: keeps this camera's viewport at 16:9 whatever the window shape,
    /// and updates it when the window is resized. Runs before other scripts so the rect is
    /// current when they convert mouse positions.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-100)]
    public sealed class CameraLetterbox : MonoBehaviour
    {
        private Camera targetCamera;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                Apply();
            }
        }

        private void Apply()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            targetCamera.rect = LetterboxMath.Viewport(lastScreenWidth, lastScreenHeight, LetterboxMath.SixteenByNine);
        }
    }
}
