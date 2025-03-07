using System;
using AbilityKit.Steps;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Action = OkapiKit.Action;

namespace AbilityKit.Actions
{
    [AddComponentMenu("AbilityAction/Action/Ability")]
    public class AbilityAction : Action
    {
        [SerializeReference] 
        private AbilityStepBase[] abilitySteps = Array.Empty<AbilityStepBase>();
        public AbilityStepBase[] AbilitySteps => abilitySteps;
        
        [SerializeField] 
        private UnityEvent onStartEvent = new UnityEvent();

        public UnityEvent OnStartEvent
        {
            get => onStartEvent;
            protected set => onStartEvent = value;
        }

        [SerializeField] 
        private UnityEvent onFinishedEvent = new UnityEvent();

        public UnityEvent OnFinishedEvent
        {
            get => onFinishedEvent;
            protected set => onFinishedEvent = value;
        }

        [SerializeField] 
        private UnityEvent onProgressEvent = new UnityEvent();
        public UnityEvent OnProgressEvent => onProgressEvent;
        
        [SerializeField] [TextArea]
        private string description = String.Empty; 
        public string Description => description;
        
        [SerializeField] [TextArea]
        private string tooltip = String.Empty;
        public string Tooltip => tooltip;
        
        public override string GetRawDescription(string ident, GameObject refObject)
        {
            string baseDescription = "This ability will...";

            float lastTime = -float.MaxValue;
            
            if (abilitySteps != null)
            {
                for (int i = 0; i < abilitySteps.Length; i++)
                {
                    var action = abilitySteps[i];
                    string actionDesc = "[NULL]";
                    string timeString = $" At {action.delay} seconds, \n";

                    string spaces = "";

                    for (int k = 0; k < 10; k++)
                    {
                        spaces += " ";

                        if (lastTime == action.delay)
                        {
                            timeString = spaces;
                        }
                        else
                        {
                            timeString += spaces;
                        }

                        if (action != null)
                        {
                            actionDesc = action.GetRawDescription("  ", gameObject);
                            actionDesc = actionDesc.Replace("\n", "\n" + spaces);
                        }

                        baseDescription += $"{timeString}{actionDesc}\n";                    
                        lastTime = action.delay;
                    }
                }
                
                return baseDescription;
            }

            return "Allows user to design ability and modify its steps.";
        }

        public override void Execute()
        {
        }
    }
};