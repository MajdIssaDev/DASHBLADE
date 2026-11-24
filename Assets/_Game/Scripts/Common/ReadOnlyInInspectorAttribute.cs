using UnityEngine;

namespace Dashblade.Common
{
    /// <summary>
    /// Shows a serialized field in the Inspector greyed out, so it can be watched but not edited.
    /// The drawer lives in the Dashblade.EditorTools assembly.
    /// </summary>
    public sealed class ReadOnlyInInspectorAttribute : PropertyAttribute
    {
    }
}
