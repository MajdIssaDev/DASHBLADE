using System.Collections.Generic;

namespace Dashblade.Combat
{
    /// <summary>
    /// The enemies a dash will destroy, each with the distance along the path at which the hit area first reaches it.
    /// The dash hands them out in path order as the body gets there.
    /// </summary>
    public sealed class DashHitQueue<T> where T : class
    {
        private readonly List<Entry> entries = new List<Entry>();
        private int next;

        /// <summary>Hits not yet handed out.</summary>
        public int PendingCount => entries.Count - next;

        public void Clear()
        {
            entries.Clear();
            next = 0;
        }

        /// <summary>
        /// Adds a hit. Adding the same item twice keeps one entry at the smaller distance, so a target found by
        /// two queries is destroyed once. Call before the first <see cref="CollectReached"/> of a dash.
        /// </summary>
        public void Add(T item, float reachDistance)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (ReferenceEquals(entries[i].Item, item))
                {
                    // Already handed out items are never handed out again.
                    if (i >= next && reachDistance < entries[i].Reach)
                    {
                        entries.RemoveAt(i);
                        break;
                    }

                    return;
                }
            }

            // Insert sorted by distance; equal distances keep the order they were added in.
            int index = entries.Count;
            while (index > next && entries[index - 1].Reach > reachDistance)
            {
                index--;
            }

            entries.Insert(index, new Entry(item, reachDistance));
        }

        /// <summary>
        /// Appends to <paramref name="output"/>, nearest first, every pending item whose reach distance is at most
        /// <paramref name="travelled"/>, and removes them from the queue. Returns how many were appended.
        /// </summary>
        public int CollectReached(float travelled, List<T> output)
        {
            int count = 0;
            while (next < entries.Count && entries[next].Reach <= travelled)
            {
                output.Add(entries[next].Item);
                next++;
                count++;
            }

            return count;
        }

        private readonly struct Entry
        {
            public Entry(T item, float reach)
            {
                Item = item;
                Reach = reach;
            }

            public T Item { get; }
            public float Reach { get; }
        }
    }
}
