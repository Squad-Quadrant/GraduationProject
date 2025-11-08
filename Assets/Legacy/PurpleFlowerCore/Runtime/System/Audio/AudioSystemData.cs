using System.Collections.Generic;
using PurpleFlowerCore.Utility;
using UnityEngine;
namespace PurpleFlowerCore.Scripts.System.Audio
{
    [Configurable("Audio")]
    public class AudioSystemData : ScriptableObject
    {
        public float BGMVolume = 1;
        public float EffectVolume = 1;
        public List<AudioClip> AudioClips = new List<AudioClip>();
    }
}