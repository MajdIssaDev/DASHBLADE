namespace Dashblade.Flow
{
    /// <summary>Where the pause (F11) stands.</summary>
    public enum PauseState
    {
        /// <summary>Not paused: the run is being played (or has ended; see <see cref="PauseFlow.RunIsOver"/>).</summary>
        Playing,

        /// <summary>Paused, the panel showing "PAUSED" and its four buttons.</summary>
        Buttons,

        /// <summary>Paused, the panel showing "CONTROLS", the three control lines and Back.</summary>
        Controls,
    }
}
