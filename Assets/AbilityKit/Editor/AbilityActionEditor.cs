using System;
using AbilityKit.Actions;
using AbilityKit.Steps;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace AbilityKit.Editor
{
    [CustomEditor(typeof(AbilityAction), true)]
    public class AbilityActionEditor : UnityEditor.Editor
    {
        private static readonly GUIContent CollapseAllAbilityStepsContent = new("▸◂", "Collapse all ability steps");
        private static readonly GUIContent ExpandAllAbilityStepsContent   = new("◂▸", "Expand all ability steps");

        private ReorderableList reorderableList;
        
        private static AbilityStepAdvancedDropdown cachedAnimationStepsDropdown;
        private static AbilityStepAdvancedDropdown AnimationStepAdvancedDropdown
        {
            get
            {
                if (cachedAnimationStepsDropdown == null)
                    cachedAnimationStepsDropdown = new AbilityStepAdvancedDropdown(new AdvancedDropdownState());
                return cachedAnimationStepsDropdown;
            }
        }

        private bool showDescriptionPanel;
        private bool showCallbacksPanel;
        private bool showStepsPanel = true;
        private bool wasShowingStepsPanel;

        private (float start, float end)[] previewingTimings;

        private void OnEnable()
        {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("abilitySteps"), 
                true, false, true, true);
            reorderableList.drawElementCallback += OnDrawAnimationStep;
            reorderableList.drawElementBackgroundCallback += OnDrawAnimationStepBackground;
            reorderableList.elementHeightCallback += GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback += OnClickToAddNew;
            reorderableList.onRemoveCallback += OnClickToRemove;
            reorderableList.onReorderCallback += OnListOrderChanged;
            reorderableList.drawHeaderCallback += OnDrawerHeader;
            
            Repaint();
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        private void OnDisable()
        {
            reorderableList.drawElementCallback -= OnDrawAnimationStep;
            reorderableList.drawElementBackgroundCallback -= OnDrawAnimationStepBackground;
            reorderableList.elementHeightCallback -= GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback -= OnClickToAddNew;
            reorderableList.onRemoveCallback -= OnClickToRemove;
            reorderableList.onReorderCallback -= OnListOrderChanged;
            reorderableList.drawHeaderCallback -= OnDrawerHeader;
        }
        
        private void OnDrawerHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Ability Steps");
        }
        
        private void AddNewAnimationStepOfType(Type targetAnimationType)
        {
            SerializedProperty abilityStepsProperty = reorderableList.serializedProperty;
            int targetIndex = abilityStepsProperty.arraySize;
            abilityStepsProperty.InsertArrayElementAtIndex(targetIndex);
            SerializedProperty arrayElementAtIndex = abilityStepsProperty.GetArrayElementAtIndex(targetIndex);
            object managedReferenceValue = Activator.CreateInstance(targetAnimationType);
            arrayElementAtIndex.managedReferenceValue = managedReferenceValue;
        
            //TODO copy from last step would be better here.
            SerializedProperty targetSerializedProperty = arrayElementAtIndex.FindPropertyRelative("target");
            if (targetSerializedProperty != null)
                targetSerializedProperty.objectReferenceValue = (serializedObject.targetObject as GameObject)?.gameObject;
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnClickToRemove(ReorderableList list)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
            SerializedPropertyExtensions.ClearPropertyCache(element.propertyPath);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private void OnListOrderChanged(ReorderableList list)
        {
            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private void OnClickToAddNew(Rect buttonRect, ReorderableList list)
        {
            AnimationStepAdvancedDropdown.Show(buttonRect, OnNewAnimationStepTypeSelected);
        }

        private void OnNewAnimationStepTypeSelected(AbilityStepAdvancedDropdownItem abilityStepAdvancedDropdownItem)
        {
            AddNewAnimationStepOfType(abilityStepAdvancedDropdownItem.AnimationStepType);
        }

        public override void OnInspectorGUI()
        {
            DrawFoldoutArea("Description", ref showDescriptionPanel, DrawDescription);
            DrawFoldoutArea("Callback", ref showCallbacksPanel, DrawCallbacks);
            DrawFoldoutArea("Steps", ref showStepsPanel, DrawAnimationSteps, DrawAnimationStepsHeader, 50);
        }
        
        private void DrawAnimationStepsHeader(Rect rect, bool foldout)
        {
            if (!foldout)
                return;
            
            var collapseAllRect = new Rect(rect)
            {
                xMin = rect.xMax - 50,
                xMax = rect.xMax - 25,
            };

            var expandAllRect = new Rect(rect)
            {
                xMin = rect.xMax - 25,
                xMax = rect.xMax - 0,
            };

            if (GUI.Button(collapseAllRect, CollapseAllAbilityStepsContent, EditorStyles.miniButtonLeft))
            {
                SetStepsExpanded(false);
            }

            if (GUI.Button(expandAllRect, ExpandAllAbilityStepsContent, EditorStyles.miniButtonRight))
            {
                SetStepsExpanded(true);
            }
        }

        private void DrawAnimationSteps()
        {
            bool wasGUIEnabled = GUI.enabled;

            reorderableList.DoLayoutList();
                        
            GUI.enabled = wasGUIEnabled;
        }
        
        private void DrawDescription()
        {
            bool wasGUIEnabled = GUI.enabled;

            SerializedProperty tooltipSerializedProperty = serializedObject.FindProperty("tooltip");
            SerializedProperty descriptionSerializedProperty = serializedObject.FindProperty("description");
                        
            using (EditorGUI.ChangeCheckScope changeDesc = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(tooltipSerializedProperty);
                EditorGUI.EndDisabledGroup();  
                
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUILayout.PropertyField(descriptionSerializedProperty);
                
                if (changeDesc.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            
            GUI.enabled = wasGUIEnabled;
        }

        protected virtual void DrawCallbacks()
        {
            bool wasGUIEnabled = GUI.enabled;
            SerializedProperty onStartEventSerializedProperty = serializedObject.FindProperty("onStartEvent");
            SerializedProperty onFinishedEventSerializedProperty = serializedObject.FindProperty("onFinishedEvent");
            SerializedProperty onProgressEventSerializedProperty = serializedObject.FindProperty("onProgressEvent");

            
            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(onStartEventSerializedProperty);
                EditorGUILayout.PropertyField(onFinishedEventSerializedProperty);
                EditorGUILayout.PropertyField(onProgressEventSerializedProperty);
                
                if (changedCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            
            GUI.enabled = wasGUIEnabled;
        }
        
        private void DrawFoldoutArea(string title, ref bool foldout, Action additionalInspectorGUI,
            Action<Rect, bool> additionalHeaderGUI = null, float additionalHeaderWidth = 0)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            if (Event.current.type == EventType.Repaint)
            {
                GUI.skin.box.Draw(rect, false, false, false, false);
            }

            using (new EditorGUILayout.VerticalScope(AnimationSequencerStyles.InspectorSideMargins))
            {
                Rect rectWithMargins = new Rect(rect)
                {
                    xMin = rect.xMin + AnimationSequencerStyles.InspectorSideMargins.padding.left,
                    xMax = rect.xMax - AnimationSequencerStyles.InspectorSideMargins.padding.right,
                };

                var foldoutRect = new Rect(rectWithMargins)
                {
                    xMax = rectWithMargins.xMax - additionalHeaderWidth,
                };

                var additionalHeaderRect = new Rect(rectWithMargins)
                {
                    xMin = foldoutRect.xMax,
                };

                foldout = EditorGUI.Foldout(foldoutRect, foldout, title, true);

                additionalHeaderGUI?.Invoke(additionalHeaderRect, foldout);

                if (foldout)
                {
                    additionalInspectorGUI.Invoke();
                    GUILayout.Space(10);
                }
            }
        }

        private void OnDrawAnimationStepBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var titlebarRect = new Rect(rect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };

                if (isActive)
                    ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, isFocused, false);
                else
                    AnimationSequencerStyles.InspectorTitlebar.Draw(titlebarRect, false, false, false, false);
            }
        }

        private void OnDrawAnimationStep(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty flowTypeSerializedProperty = element.FindPropertyRelative("flowType");

            if (!element.TryGetTargetObjectOfProperty(out AbilityStepBase abilityStepBase))
                return;

            FlowType flowType = (FlowType)flowTypeSerializedProperty.enumValueIndex;

            int baseIdentLevel = EditorGUI.indentLevel;
            
            GUIContent guiContent = new GUIContent(element.displayName);
            if (abilityStepBase != null)
                guiContent = new GUIContent(abilityStepBase.GetDisplayNameForEditor(index + 1));

            if (flowType == FlowType.Join)
                EditorGUI.indentLevel = baseIdentLevel + 1;
            
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 10;
            rect.width -= 20;

            EditorGUI.PropertyField(
                rect,
                element,
                guiContent,
                false
            );

            EditorGUI.indentLevel = baseIdentLevel;
            // DrawContextInputOnItem(element, index, rect);
        }

        private float GetAnimationStepHeight(int index)
        {
            if (index > reorderableList.serializedProperty.arraySize - 1)
                return EditorGUIUtility.singleLineHeight;
            
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return element.GetPropertyDrawerHeight();
        }

        private void SetStepsExpanded(bool expanded)
        {
            SerializedProperty abilityStepsProperty = reorderableList.serializedProperty;
            for (int i = 0; i < abilityStepsProperty.arraySize; i++)
            {
                abilityStepsProperty.GetArrayElementAtIndex(i).isExpanded = expanded;
            }
        }

        private static Rect DrawAutoSizedBadgeRight(Rect rect, string text, Color color)
        {
            var style = AnimationSequencerStyles.Badge;
            var size = style.CalcSize(EditorGUIUtility.TrTempContent(text));
            var buttonRect = new Rect(rect)
            {
                xMin = rect.xMax - size.x,
            };

            if (Event.current.type == EventType.Repaint)
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
                style.Draw(buttonRect, text, false, false, true, false);
                GUI.backgroundColor = oldColor;
            }

            return new Rect(rect)
            {
                xMax = rect.xMax - size.x - style.margin.left,
            };
        }
    }
}