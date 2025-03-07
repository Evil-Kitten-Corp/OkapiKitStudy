using NaughtyAttributes;
using UnityEngine;

namespace AbilityKit.Steps
{
    public class AbilityStepPlaySound: AbilityStepBase
    {
        public override string DisplayName => "Play Sound";
        
        [SerializeField]
        private AudioClip clip;
        [SerializeField, MinMaxSlider(0.0f, 1.0f)]
        private Vector2 volume = Vector2.one;
        [SerializeField, MinMaxSlider(0.0f, 2.0f)]
        private Vector2 pitch = Vector2.one;

        [HideInInspector] public string desc;
        
        public override string GetDisplayNameForEditor(int index)
        {
            return $"{index}. Play {(clip != null ? clip.name : "none")} sound";
        }

        public override string GetRawDescription(string ident, GameObject gameObject)
        {
            if (clip == null)
            {
                desc += "plays an undefined sound";
            }
            else
            {
                desc += $"plays sound {clip.name}";
            }

            if (volume.x == volume.y)
            {
                desc += $" at volume {volume.x}";
            }
            else
            {
                desc += $" with a volume in the range [{volume.x},{volume.y}]";
            }

            if (pitch.x == pitch.y)
            {
                desc += $" and at pitch {pitch.x}";
            }
            else
            {
                desc += $" and a pitch in the range [{pitch.x},{pitch.y}]";
            }

            return desc;
        }
    }
}