using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace AI
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    public class AiControl : MonoBehaviour
    {
        public GameObject target;
        
        protected NavMeshAgent Agent;      
        protected GameObject Player;
        protected Attributes_scr TargetAttr;
        protected Attributes_scr SelfAttr;
        [SerializeField] protected Vector3 TargetDestination;
        protected AiAnimator_scr AiAnimatorScr; // the character we are controlling
        [SerializeField] protected float chaseSpeed;
        [SerializeField] protected bool doNotMove;
        [SerializeField] protected bool debug;
        [SerializeField] protected float fovDistance, fovAngle, hearingDistance;
        [SerializeField] protected LayerMask enemiesMask, obstacleMask;

        protected void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            Agent = GetComponent<NavMeshAgent>();
            TargetDestination = transform.position;
            AiAnimatorScr = GetComponent<AiAnimator_scr>();
            target = gameObject;
            SelfAttr = GetComponent<Attributes_scr>();

	        Agent.updateRotation = true;
	        Agent.updatePosition = true;
            
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        
        //------------------------------------------
        protected bool AiChase()
        {
            Agent.speed = chaseSpeed;
                
            if (CanSeeTarget(target) || CanHearTarget(target)) //Update position only when seeing player
            {
                TargetDestination = target.transform.position;                
            }
            else if (InStoppingDistance()) 
                return false;
                
            return true;
        }

        //------------------------------------------
        protected IEnumerator AiAttack()
        {
            //TODO: Return when lost target
            //If target stopped existing or dropped below 0 health break back to patrol
            if (target && TargetAttr.health > 0)
            {
                transform.LookAt(target.transform);

                AiAnimatorScr.SetAttackAnim(SelfAttr.attackSpeed);
                yield return new WaitUntil(() => !Agent.isStopped);

                //Check if target still exists after the animation is done
                if (target && TargetAttr.health <= 0)                
                {
                    TargetDestination = target.transform.position;                    
                }
            }            
        }
	
        public void SetTarget(GameObject target)
        {
            float distToCurrentTarget = this.target && this.target != gameObject ? Vector3.Distance(transform.position, this.target.transform.position) : 999; //Set distance to new target to 999 if targetting self (bandaid)
            float distToNewTarget = Vector3.Distance(transform.position, target.transform.position);
            
            if (this.target == Player || distToNewTarget < distToCurrentTarget)
            {
                this.target = target;
                TargetAttr = target.GetComponent<Attributes_scr>();
            }
        }

        protected void UpdatePosition(Vector3 destination)
        {
            if (!doNotMove && Agent && !Agent.isStopped)
            {                
                Agent.SetDestination(destination);
                transform.LookAt(Agent.nextPosition);

                if (!InStoppingDistance())
                {
                    AiAnimatorScr.Move(Agent.desiredVelocity);
                }
                else
                    AiAnimatorScr.Move(Vector3.zero);
            }
        }

        protected bool InStoppingDistance()
        {
           float remainingDistance = Vector3.Distance(transform.position, TargetDestination);

           return remainingDistance <= Agent.stoppingDistance;
        }
        
        
        protected bool TargetInAttackRange()
        {
            return (target && Vector3.Distance(transform.position, target.transform.position) <= Agent.stoppingDistance);
        }

        protected bool CanSeeTarget(GameObject target)
        {
            if(target){
                Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
                Vector3 direction = (new Vector3(target.transform.position.x, 1f, target.transform.position.z) -
                                    origin).normalized;
                float angle = Vector3.Angle(transform.forward, direction);

                if (angle < fovAngle)
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    if(!Physics.Raycast(origin, direction, distance, obstacleMask))
                        return true;
                }
            }

            return false;
        }

        protected bool CanHearTarget(GameObject target)
        {
            if(target){
                //TODO: Ask object for it's velocity instead (from attributes)
//                Rigidbody rb = target.GetComponent<Rigidbody>();
                
                float distance = Vector3.Distance(transform.position, target.transform.position);
                return distance < hearingDistance;
                
                //&& rb.velocity.magnitude != 0; //Any movement (might change formula someday)
            }
            
            return false;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
                Gizmos.DrawWireSphere(Agent.destination, 1f);
        }

        //Used from animation event
        private void LockInPlace(int locked)
        {
            Agent.isStopped = locked != 0;
        }
    }
}
