using UnityEngine;

namespace AbilityKit.Steps
{
    public class AbilityStepWait: AbilityStepBase
    {
        public override string DisplayName => "Wait for Interval";

        [SerializeField]
        private float interval;
        public float Interval
        {
            get => interval;
            set => interval = value;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            return $"{index}. Wait {interval} seconds";
        }
    }
}