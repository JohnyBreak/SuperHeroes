using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Optional fallback: enable/disable colliders registered by string id from AttackDefinition.
    /// Prefer <see cref="SweptHitProbe"/> for fists; use this when a designer attaches volume colliders to bones.
    /// </summary>
    public class HitboxRegistry : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public string Id;
            public Collider Collider;
        }

        [SerializeField] private Entry[] _entries;

        private void Awake()
        {
            Debug.Assert(_entries != null);
            SetAllEnabled(false);
        }

        public void SetEnabled(string hitboxId, bool enabled)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];
                if (entry == null || entry.Collider == null)
                {
                    continue;
                }

                if (entry.Id == hitboxId)
                {
                    entry.Collider.enabled = enabled;
                }
            }
        }

        public void SetAllEnabled(bool enabled)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];
                if (entry?.Collider != null)
                {
                    entry.Collider.enabled = enabled;
                }
            }
        }
    }
}
