using NaughtyAttributes;
using UnityEngine;

namespace AbilityKit.Sample
{
    [RequireComponent(typeof(Rigidbody))]
    public class WasdTopdown: MonoBehaviour, IMovementTopdown
    {
        [Header("Movement Settings")]
        public float moveSpeed = 1f;
        
        [Header("Rotation Settings")]
        public bool rotateToMouse = false;
        [ShowIf(nameof(rotateToMouse))] public float rotationSpeed = 10f;
        [HideIf(nameof(rotateToMouse))] public float turnSpeed = 20f;

        private Rigidbody _rigidbody;
        private Vector3 _movement;
        private Quaternion _rotation = Quaternion.identity;

        private Transform _overrideMovement = null;
        private float _overrideMs = 0;
        private bool _attacking;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            var myAttack = GetComponent<AttackTopdown>();

            if (myAttack != null)
            {
                myAttack.StartAttack += () => _attacking = true;
            }
        }
        
        private void FixedUpdate()
        {
            if (_overrideMovement != null)
            {
                if (_attacking)
                {
                    var lookRotation = GetLookRotation();
                    
                    if (transform.rotation == lookRotation.Item1)
                    {
                        _attacking = false;
                        return;
                    }
                    
                    RotateToMouse();
                    return;
                }
                
                float distance = Vector3.Distance(transform.position, _overrideMovement.position);
        
                if (distance < 1f)
                {
                    _overrideMovement = null;
                    return;
                }
                
                Vector3 movePosition = transform.position;

                movePosition.x = Mathf.MoveTowards(transform.position.x, 
                    _overrideMovement.transform.position.x, _overrideMs * Time.deltaTime);
                movePosition.z = Mathf.MoveTowards(transform.position.z, 
                    _overrideMovement.transform.position.z, _overrideMs * Time.deltaTime);

                _rigidbody.MovePosition(movePosition);
                
                return;
            }
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            _movement.Set(horizontal, 0f, vertical);
            _movement.Normalize();

            bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
            bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
            bool isWalking = hasHorizontalInput || hasVerticalInput;
            
            if (_attacking)
            {
                var lookRotation = GetLookRotation();
                    
                if (transform.rotation == lookRotation.Item1)
                {
                    _attacking = false;
                    return;
                }
                    
                RotateToMouse();
                return;
            }

            HandleRotation();
            
            _rigidbody.MovePosition(_rigidbody.position + _movement * (moveSpeed * Time.deltaTime));
        }

        private void HandleRotation()
        { 
            if (rotateToMouse)
            {
                RotateToMouse();
            }
            else
            {
                Vector3 desiredForward = Vector3.RotateTowards(transform.forward, _movement, 
                    turnSpeed * Time.deltaTime, 0f);
                _rotation = Quaternion.LookRotation(desiredForward);
                
                _rigidbody.MoveRotation(_rotation);
            }
        }

        public (Quaternion, Vector3) GetLookRotation()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldMousePos = ray.GetPoint(enter);
                Vector3 lookDirection = worldMousePos - transform.position;
                lookDirection.y = 0;

                return (Quaternion.LookRotation(lookDirection), lookDirection);
            }

            return (default, Vector3.zero);
        }
        
        public void RotateToMouse()
        {
            (Quaternion rotation, Vector3 lookDir) lookRotation = GetLookRotation();
            var lookDirection = lookRotation.lookDir;
            
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = lookRotation.rotation;
                _rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 
                    rotationSpeed * Time.deltaTime));
            }
        }

        public void MoveTo(Transform t, float snapSpeed)
        {
            _overrideMovement = t;
            _overrideMs = snapSpeed;
        }

        private void OnDrawGizmos()
        {
            if (_overrideMovement == null)
            {
                Gizmos.color = Color.cyan;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            
            Gizmos.DrawWireCube(transform.position, new Vector3(3f, 3f, 3f));
        }
    }
}