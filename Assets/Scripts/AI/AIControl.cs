using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace AI
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    public class AIControl : MonoBehaviour
    {
        [SerializeField] public NavMeshAgent agent;       // the navmesh agent required for the path finding
        public GameObject target;                                    // target to aim for
        
        protected GameObject player;
        protected Attributes_scr targetAttr;
        protected Attributes_scr selfAttr;
        protected Vector3 targetDestination;
        [SerializeField] protected AiAnimator_scr m_AiAnimatorScr; // the character we are controlling
        [SerializeField] protected float chaseSpeed;
        [SerializeField] protected bool doNotMove;
        [SerializeField] protected bool debug;
        [SerializeField] protected float FovDistance, FovAngle, hearingDistance;
        [SerializeField] protected LayerMask enemiesMask, obstacleMask;

        protected void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponent<NavMeshAgent>();
            agent.SetDestination(transform.position);
            m_AiAnimatorScr = GetComponent<AiAnimator_scr>();
            target = gameObject;
            selfAttr = GetComponent<Attributes_scr>();

	        agent.updateRotation = true;
	        agent.updatePosition = true;
            
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        //------------------------------------------
        protected void AIChase()
        {
            agent.speed = chaseSpeed;
            //TODO: Return when lost target

            if (canSeeTarget(target) || canHearTarget(target)) //Update position only when seeing player
            {
                targetDestination = target.transform.position;

                if (TargetInAttackRange()) ;
                //TODO: Completed  chase with success
            }
            else if (inStoppingDistance()) ;
                //TODO: Completed chase with failure
        }

        protected bool TargetInAttackRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) <= agent.stoppingDistance;
        }

        //------------------------------------------
        protected IEnumerator AIAttack()
        {
            //TODO: Return when lost target
            //If target stopped existing or dropped below 0 health break back to patrol
            if (target && targetAttr.health > 0)
            {
                transform.LookAt(target.transform);

                m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed);
                yield return new WaitUntil(() => !agent.isStopped);

                //Check if target still exists after the animation is done
                if (!target || targetAttr.health <= 0) ;
                //TODO: Attack completed with success ?
                else
                {
                    targetDestination = target.transform.position;
                    if (!TargetInAttackRange()) ;
                    //TODO: Attack completed
                }
            }
            else ;
            //TODO: Return to basic state;
        }
	
        public void SetTarget(GameObject target)
        {
            float distToCurrentTarget = this.target && this.target != gameObject ? Vector3.Distance(transform.position, this.target.transform.position) : 999; //Set distance to new target to 999 if targetting self (bandaid)
            float distToNewTarget = Vector3.Distance(transform.position, target.transform.position);
            
            if (this.target == player || distToNewTarget < distToCurrentTarget)
            {
                this.target = target;
                targetAttr = target.GetComponent<Attributes_scr>();
            }
        }

        protected void updatePosition(Vector3 destination)
        {
            if (!doNotMove && agent && !agent.isStopped)
            {
                agent.SetDestination(destination);
                transform.LookAt(agent.nextPosition);

                if (!inStoppingDistance())
                {
                    m_AiAnimatorScr.Move(agent.desiredVelocity);
                }
                else
                    m_AiAnimatorScr.Move(Vector3.zero);
            }
        }

        protected bool inStoppingDistance()
        {
           float remainingDistance = Vector3.Distance(transform.position, targetDestination);

           return remainingDistance <= agent.stoppingDistance;
        }

        protected bool canSeeTarget(GameObject target)
        {
            Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
            Vector3 direction = (new Vector3(target.transform.position.x, 1f, target.transform.position.z) -
                                origin).normalized;
            float angle = Vector3.Angle(transform.forward, direction);

            if (angle < FovAngle)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if(!Physics.Raycast(origin, direction, distance, obstacleMask))
                    return true;
            }

            return false;
        }

        protected bool canHearTarget(GameObject target)
        {
            //TODO: Ask object for it's velocity instead (from attributes)
//            Rigidbody rb = target.GetComponent<Rigidbody>();
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance < hearingDistance;
            
                //&& rb.velocity.magnitude != 0; //Any movement (might change formula someday)
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
                Gizmos.DrawWireSphere(agent.destination, 1f);
        }

        //Used from animation event
        private void LockInPlace(int locked)
        {
            agent.isStopped = locked != 0;
        }
    }
}
