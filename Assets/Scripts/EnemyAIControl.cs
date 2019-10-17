using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class EnemyAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling
        public Transform target;                                    // target to aim for

        private GameObject terrain;
        
        [SerializeField] private float patrolSpeed;
        [SerializeField] private float chaseSpeed;
        [SerializeField] private bool doNotMove;
        
        public enum ENEMY_STATE {PATROL, CHASE, ATTACK};
        //------------------------------------------
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                currentstate = value;
                
                StopAllCoroutines();

                switch(currentstate)
                {
                    case ENEMY_STATE.PATROL:
                        StartCoroutine(AIPatrol());
                        break;

                    case ENEMY_STATE.CHASE:
//                        StartCoroutine(AIChase());
                        break;

                    case ENEMY_STATE.ATTACK:
//                        StartCoroutine(AIAttack());
                        break;
                }
            }
        }
        //------------------------------------------
        [SerializeField]
        private ENEMY_STATE currentstate = ENEMY_STATE.PATROL;

        


        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;

            terrain = GameObject.FindWithTag("Terrain");

            CurrentState = ENEMY_STATE.PATROL;
        }
        
        public IEnumerator AIPatrol()
	{
		agent.speed = patrolSpeed;

        if (doNotMove)
            agent.SetDestination(transform.position);
        else
            agent.SetDestination(randomWaypoint());
        
        while (gameObject && currentstate == ENEMY_STATE.PATROL)
        {
            //TODO: Add line of sight
//            m_ThisScrLineOfSight.Sensitivity = scr_LineOfSight.SightSensitivity.STRICT;
            
            agent.isStopped = false;

            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else if (doNotMove)
                agent.SetDestination(transform.position);
            else
                agent.SetDestination(randomWaypoint());
                
            while (agent.pathPending)
                yield return null;
            
            //TODO: Line of sight
//            if (m_ThisScrLineOfSight.CanSeeTarget)
//            {
//                agent.isStopped = true;
//                CurrentState = ENEMY_STATE.CHASE;
//                yield break;
//            }

			yield return null;
		}
	}
	//------------------------------------------
//	public IEnumerator AIChase()
//	{
//		agent.speed = chaseSpeed;
//		
//		//Loop while chasing
//		while(currentstate == ENEMY_STATE.CHASE)
//		{
//			//Set loose search
//			m_ThisScrLineOfSight.Sensitivity = scr_LineOfSight.SightSensitivity.LOOSE;
//
//            //Chase to last known position
//            agent.isStopped = false;
//			agent.SetDestination(m_ThisScrLineOfSight.LastKnowSighting);
//
//			//Wait until path is computed
//			while(agent.pathPending)
//				yield return null;
//			
//			//Have we reached destination?
//			if(agent.remainingDistance <= agent.stoppingDistance)
//			{
//				//Stop agent
//                agent.isStopped = true;
//
//				//Reached destination but cannot see player
//				if (!m_ThisScrLineOfSight.CanSeeTarget)
//				{
//					//Check nearest gate if lost player
//					PatrolDestination = newDestination(nearestDestination());
//					CurrentState = ENEMY_STATE.PATROL;
//				}
//					
//				else //Reached destination and can see player. Reached attacking distance
//					CurrentState = ENEMY_STATE.ATTACK;
//
//				yield break;
//			}
//
//			//Wait until next frame
//			yield return null;
//		}
//	}
//
//	//------------------------------------------
//	public IEnumerator AIAttack()
//	{
//		
//		//Loop while chasing and attacking
//		while(currentstate == ENEMY_STATE.ATTACK)
//		{
//            //Chase to player position
//            agent.isStopped = false;
//			agent.SetDestination(PlayerTransform.position);
//
//			//Wait until path is computed
//			while(agent.pathPending)
//				yield return null;
//
//			//Has player run away?
//			if(agent.remainingDistance > agent.stoppingDistance)
//			{
//				animator.SetBool("Attacking", false);
//				while (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attacking"))
//					yield return null;
//				//Change back to chase
//				CurrentState = ENEMY_STATE.CHASE;
//				yield break;
//			}
//
//			//Wait until next frame
//			yield return null;
//		}
//	}


        private void Update()
        {

//
//            if (agent.remainingDistance > agent.stoppingDistance)
//                character.Move(agent.desiredVelocity, false, false);
//            else
//                CurrentState = ENEMY_STATE.PATROL;
        }


        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private Vector3 randomWaypoint()
        {
            Bounds levelBounds = terrain.gameObject.GetComponent<Collider>().bounds;
            float x = Random.Range(levelBounds.min.x, levelBounds.max.x);
            float z = Random.Range(levelBounds.min.z, levelBounds.max.z);
            
            NavMeshHit hit;
            NavMesh.SamplePosition(new Vector3(x, 0, z), out hit, 2.0f, NavMesh.AllAreas);
            
            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(hit.position, path) ? hit.position : randomWaypoint();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if(Application.isPlaying)
                Gizmos.DrawWireSphere(agent.destination, 1f);
        }
    }
}
