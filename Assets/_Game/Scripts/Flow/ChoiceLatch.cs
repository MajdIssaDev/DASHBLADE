using System;
using System.Collections.Generic;

namespace Dashblade.Flow
{
    /// <summary>
    /// Accepts one menu choice and then nothing more: a double click, or a click on a second button before the
    /// first one's scene has loaded, is ignored. A choice that is not offered here is refused and does not use the
    /// latch up.
    /// </summary>
    public sealed class ChoiceLatch
    {
        private readonly HashSet<MenuChoice> offered;

        /// <param name="offered">The choices this screen offers (for example Play and Quit on the main menu). At least one.</param>
        public ChoiceLatch(params MenuChoice[] offered)
        {
            if (offered == null || offered.Length == 0)
            {
                throw new ArgumentException("A menu needs at least one choice.", nameof(offered));
            }

            this.offered = new HashSet<MenuChoice>(offered);
        }

        /// <summary>True once a choice has been accepted.</summary>
        public bool HasChosen { get; private set; }

        /// <summary>The accepted choice; meaningful only when <see cref="HasChosen"/> is true.</summary>
        public MenuChoice Choice { get; private set; }

        public bool Offers(MenuChoice choice) => offered.Contains(choice);

        /// <summary>
        /// Returns true if <paramref name="choice"/> is accepted: it is offered and nothing was chosen before.
        /// Every later call returns false.
        /// </summary>
        public bool TryChoose(MenuChoice choice)
        {
            if (HasChosen || !offered.Contains(choice))
            {
                return false;
            }

            HasChosen = true;
            Choice = choice;
            return true;
        }
    }
}
