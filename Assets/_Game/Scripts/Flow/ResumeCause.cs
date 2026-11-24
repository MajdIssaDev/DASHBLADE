namespace Dashblade.Flow
{
    /// <summary>Why a pause (F11) ended and the game runs again.</summary>
    public enum ResumeCause
    {
        /// <summary>Esc was pressed while paused (on either view). No dash lockout follows.</summary>
        Escape,

        /// <summary>The Resume button was clicked. The dash then drops left-clicks for the Start Lockout.</summary>
        ResumeButton,

        /// <summary>The run got a result while paused; the ending takes over.</summary>
        RunOver,
    }
}
