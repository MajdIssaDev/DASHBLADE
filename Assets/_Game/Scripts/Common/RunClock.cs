namespace Dashblade.Common
{
    /// <summary>
    /// Game time of a run, counted from the first frame the run is shown (F06, "Loading time is not game time"). The
    /// first <see cref="Step"/> starts the clock at zero; every later step is how far the game clock has moved since
    /// the previous step. Time that passed before the first frame (loading the scene) is therefore never counted, and
    /// a frozen game (Time.timeScale = 0) gives steps of 0.
    /// </summary>
    /// <remarks>
    /// The steps come from the game clock (Time.timeAsDouble), not from summing Time.deltaTime, because right after a
    /// scene load the two disagree: the first frame's deltaTime is the loading time, and on the next frame Unity
    /// holds the game clock still but reports that same deltaTime again; with Play Mode starting in the scene, the
    /// first frame reports a deltaTime of 0.02 while the clock reads 0. In every other frame a step equals that
    /// frame's deltaTime.
    /// </remarks>
    public sealed class RunClock
    {
        private bool started;
        private double startTime;
        private double lastTime;

        /// <summary>True once the first frame has been stepped.</summary>
        public bool HasStarted => started;

        /// <summary>Game time since the first frame, in seconds; 0 until the first <see cref="Step"/>.</summary>
        public double Elapsed => started ? lastTime - startTime : 0d;

        /// <summary>
        /// Call once per frame with the game clock (Time.timeAsDouble). Returns the game time, in seconds, since the
        /// previous call: 0 on the first call, which starts the clock, and never less than 0.
        /// </summary>
        public float Step(double gameTime)
        {
            if (!started)
            {
                started = true;
                startTime = gameTime;
                lastTime = gameTime;
                return 0f;
            }

            double step = gameTime - lastTime;
            if (!(step > 0d))
            {
                return 0f;
            }

            lastTime = gameTime;
            return (float)step;
        }
    }
}
