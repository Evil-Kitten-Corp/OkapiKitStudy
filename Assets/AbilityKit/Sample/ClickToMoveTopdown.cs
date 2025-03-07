using UnityEngine;
using UnityEngine.AI;

namespace AbilityKit.Sample
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ClickToMoveTopdown: MonoBehaviour, IMovementTopdown
    {
        public NavMeshAgent agent;
        public float rotateSpeedMovement = 0.05f;
        
        private float rotateVelocity;
        private float motionSmoothTime = 0.1f;
        
        private Vector3 _overrideMovement = Vector3.zero;

        private bool _attacking;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();

        }

        private void Update()
        {
            if (_overrideMovement != Vector3.zero)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    agent.SetDestination(_overrideMovement);
                    
                    Quaternion rot = Quaternion.LookRotation(_overrideMovement - transform.position);
                    float rotY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rot.eulerAngles.y, 
                        ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5f));
                    
                    transform.eulerAngles = new Vector3(0, rotY, 0);
                }
                
                return;
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 
                        out hit, Mathf.Infinity))
                {
                    agent.SetDestination(hit.point);
                    
                    Quaternion rot = Quaternion.LookRotation(hit.point - transform.position);
                    float rotY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rot.eulerAngles.y, 
                        ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5f));
                    
                    transform.eulerAngles = new Vector3(0, rotY, 0);
                }
            }
        }

        public (Quaternion, Vector3) GetLookRotation()
        {
            return (Quaternion.LookRotation(agent.velocity.normalized), agent.destination);
        }

        public void RotateToMouse()
        {
            return;
        }

        public void MoveTowards(Vector3 transformPosition, Vector3 snapTargetPosition, float snapSpeed)
        {
            return;
        }

        public void SetDestination(Vector3 pos)
        {
            _overrideMovement = pos;
        }
    }
}