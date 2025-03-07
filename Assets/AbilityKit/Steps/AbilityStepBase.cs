using System;
using AbilityKit.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace AbilityKit.Steps
{
    [Serializable]
    public abstract class AbilityStepBase
    {
        [SerializeField] internal float delay;
        public float Delay => delay;

        [SerializeField]
        private FlowType flowType;
        public FlowType FlowType => flowType;
        
        public virtual string DisplayName { get; }
        
        public virtual string GetDisplayNameForEditor(int index)
        {
            return $"{index}. {this}";
        }
        
        public virtual string GetRawDescription(string ident, GameObject gameObject)
        {
            return "";
        }
    }
}