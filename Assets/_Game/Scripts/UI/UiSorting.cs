namespace Dashblade.UI
{
    /// <summary>
    /// Drawing order of the screen-space canvases (F06). Every canvas is Screen Space - Camera on the letterboxed Main
    /// Camera, on the Default sorting layer, so canvases and sprites sort together by Order in Layer. Arena sprites use
    /// orders -2 to 3. Anything drawn between the sprites and <see cref="EndingOrder"/> (the in-run HUD,
    /// <see cref="HudOrder"/>) stays visible but darkened under the ending's black layer.
    /// </summary>
    public static class UiSorting
    {
        /// <summary>Order in Layer of the in-run HUD canvas (F07): above every arena sprite, below the ending canvas.</summary>
        public const int HudOrder = 50;

        /// <summary>Order in Layer of the ending canvas (black layer and panel) and of the main menu canvas.</summary>
        public const int EndingOrder = 100;
    }
}
