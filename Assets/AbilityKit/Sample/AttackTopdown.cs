using System;
using NaughtyAttributes;
using UnityEngine;

namespace AbilityKit.Sample
{
    public class AttackTopdown: MonoBehaviour
    {
        [Header("Attack Settings")]
        public float attackRange = 2f; 
        public float attackSpeed = 1f; 
        public bool isMelee = true; 
        [ShowIf(nameof(isMelee))] public bool autoAim = true; 
        
        [Header("Movement Settings")]
        public bool attackOnMouseDirection = true;
        
        [Header("Auto Aim Settings (Melee Only)")]
        [ShowIf(nameof(autoAim))] public float snapRange = 3f; // How far the snap-to-target feature works
        [ShowIf(nameof(autoAim))] public float snapSpeed = 10f; // Speed to "snap" toward the target

        [Header("References")]
        public LayerMask attackableLayers;

        private bool _attacking;
        private float _nextAttackTime = 0f;
        private IMovementTopdown _wasdTopdown;
        private Transform _target;

        public event Action StartAttack;

        private void Awake()
        {
            _wasdTopdown = GetComponent<IMovementTopdown>();
        }

        private void Update()
        {
            if (Time.time < _nextAttackTime) 
                return;

            if (_wasdTopdown is WasdTopdown)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PerformDirectAttack();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1)) 
                {
                    AttemptClickToAttack();
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    PerformAttackMove();
                }
            }
            
            if (_attacking)
            {
                if (_wasdTopdown is WasdTopdown wasdTopdown)
                {
                    while (transform.rotation != wasdTopdown.GetLookRotation().Item1)
                    {
                        wasdTopdown.RotateToMouse();
                    }
                }
                
            }
        }
        
        /// <summary>
        /// Click an enemy to move and attack them (LoL-style).
        /// </summary>
        private void AttemptClickToAttack()
        {
            Debug.Log("Click to attack!");
            Transform clickedTarget = GetTargetUnderMouse();
            
            if (clickedTarget)
            {
                Debug.Log("Found target!");
                _target = clickedTarget;
                MoveToAttackTarget();
            }
        }
        
        /// <summary>
        /// Press A to attack the nearest target to the cursor.
        /// </summary>
        private void PerformAttackMove()
        {
            Debug.Log("Attack move!");
            _target = FindNearestTargetToMouse();
            
            if (_target)
            {
                MoveToAttackTarget();
            }
        }
        
        /// <summary>
        /// Direct attacks: Left-click to attack (mouse or movement direction).
        /// </summary>
        private void PerformDirectAttack()
        {
            Debug.Log("Calling direct attack.");
            
            if (isMelee)
            {
                Transform snapTarget = FindNearestTargetToMouse(snapRange);
                
                if (snapTarget && Vector3.Distance(transform.position, snapTarget.position) > attackRange)
                {
                    Debug.Log("Snapping to target.");
                    // Snap to target if close but out of range

                    if (_wasdTopdown is WasdTopdown wasdTopdown)
                    {
                        wasdTopdown.MoveTo(snapTarget, snapSpeed);
                    }
                }
            }

            if (_wasdTopdown is WasdTopdown wasdTopdownRotation)
            {
                wasdTopdownRotation.RotateToMouse();
            }

            Attack();
        }

        /// <summary>
        /// Moves toward the target and attacks when in range.
        /// </summary>
        private void MoveToAttackTarget()
        {
            if (!_target) 
                return;

            if (_wasdTopdown is ClickToMoveTopdown clickToMoveTopdown)
            {
                Debug.Log("Is moving to attack target!");
                // Move toward the target
                clickToMoveTopdown.SetDestination(_target.position);
            }
        }

        private void CheckIfInRangeToAttack()
        {
            if (!_target) return;

            if (Vector3.Distance(transform.position, _target.position) <= attackRange)
            {
                Debug.Log("Is in range to attack.");
                Attack();
            }
        }

        /// <summary>
        /// Executes an attack.
        /// </summary>
        void Attack()
        {
            _nextAttackTime = Time.time + (1f / attackSpeed);
        }

        /// <summary>
        /// Finds the nearest enemy to the cursor within the given range.
        /// </summary>
        Transform FindNearestTargetToMouse(float range = Mathf.Infinity)
        {
            Transform bestTarget = null;
            float closestDistance = range;

            foreach (Collider enemy in Physics.OverlapSphere(transform.position, snapRange, attackableLayers))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = enemy.transform;
                }
            }

            Debug.Log($"Best target: {bestTarget?.gameObject.name}");
            return bestTarget;
        }

        /// <summary>
        /// Checks if there's an enemy under the mouse.
        /// </summary>
        Transform GetTargetUnderMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, attackableLayers))
            {
                Debug.Log($"Target detected: {hit.transform.gameObject.name}");
                return hit.transform;
            }
            
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;

            if (autoAim)
            {
                Gizmos.DrawWireSphere(transform.position, snapRange);
            }
        }
    }
}