using UnityEngine;

namespace MemoryTranser.Scripts.Game.Sound {
    public class SeManager : MonoBehaviour {
        [SerializeField] private AudioClip[] seClips;
    }

    public enum SEs {
        ThrowBox
    }
}