using System;
using AbilityKit.Steps;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AbilityKit.Editor
{
    public sealed class AbilityStepAdvancedDropdown : AdvancedDropdown
    {
        private Action<AbilityStepAdvancedDropdownItem> callBack;

        public AbilityStepAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Ability Step");

            TypeCache.TypeCollection availableTypesOfAnimationStep = TypeCache.GetTypesDerivedFrom(typeof(AbilityStepBase));
            foreach (Type animatedItemType in availableTypesOfAnimationStep)
            {
                if (animatedItemType.IsAbstract)
                    continue;
                
                AbilityStepBase animationStepBase = Activator.CreateInstance(animatedItemType) as AbilityStepBase;

                string displayName = animationStepBase.GetType().Name;
                if (!string.IsNullOrEmpty(animationStepBase.DisplayName))
                    displayName = animationStepBase.DisplayName;
                
                root.AddChild(new AbilityStepAdvancedDropdownItem(animationStepBase, displayName));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            callBack?.Invoke(item as AbilityStepAdvancedDropdownItem);
        }

        public void Show(Rect rect, Action<AbilityStepAdvancedDropdownItem> onItemSelectedCallback)
        {
            callBack = onItemSelectedCallback;
            base.Show(rect);
        }
    }
}