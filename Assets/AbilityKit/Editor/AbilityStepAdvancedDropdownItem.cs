using System;
using AbilityKit.Steps;
using UnityEditor.IMGUI.Controls;

namespace AbilityKit.Editor
{
    public sealed class AbilityStepAdvancedDropdownItem : AdvancedDropdownItem
    {
        private readonly Type animationStepType;
        public Type AnimationStepType => animationStepType;

        public AbilityStepAdvancedDropdownItem(AbilityStepBase animationStepBase, string displayName) : base(displayName)
        {
            animationStepType = animationStepBase.GetType();
        }
    }
}
