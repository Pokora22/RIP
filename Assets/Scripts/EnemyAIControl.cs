using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    public class EnemyAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public AiAnimator_scr m_AiAnimatorScr; // the character we are controlling
        public GameObject target;                                    // target to aim for

        private GameObject terrain;
        private GameObject player;
        private Coroutine currentCoroutine;
        private Attributes_scr targetAttr, selfAttr;
        [SerializeField] private float patrolSpeed;
        [SerializeField] private float chaseSpeed;
        [SerializeField] private bool doNotMove;
        [SerializeField] private bool debug;
        [SerializeField] private float AiCheckDelay = 1f;
        [SerializeField] private float FovDistance = 30f;
        [SerializeField] private float FovAngle = 45f;
        [SerializeField] private float hearingDistance = 5f;
        [SerializeField] private LayerMask enemiesMask;
        [SerializeField] private LayerMask obstacleMask;
        private float NextAiCheckTimestamp;
        
        public enum ENEMY_STATE {PATROL, CHASE, ATTACK, NONE};        
        
        //------------------------------------------
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                currentstate = value;
                if(debug)
                    Debug.Log("New state: " + currentstate);
                
                if(currentCoroutine != null)
                    StopCoroutine(currentCoroutine);

                switch(currentstate)
                {                    
                    case ENEMY_STATE.PATROL:
                        currentCoroutine = StartCoroutine(AIPatrol());
                        break;

                    case ENEMY_STATE.CHASE:                        
                        currentCoroutine = StartCoroutine(AIChase());
                        break;

                    case ENEMY_STATE.ATTACK:                        
                        currentCoroutine = StartCoroutine(AIAttack());
                        break;
                    
                    case ENEMY_STATE.NONE:
                        currentCoroutine = StartCoroutine(AIDoNothing());
                        break;
                }
            }
        }

        private IEnumerator AIDoNothing()
        {
            agent.isStopped = true;
            yield break;
        }

        //------------------------------------------
        [SerializeField]
        private ENEMY_STATE currentstate = ENEMY_STATE.PATROL;

        

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            m_AiAnimatorScr = GetComponent<AiAnimator_scr>();
            target = gameObject;
            selfAttr = GetComponent<Attributes_scr>();

	        agent.updateRotation = true;
	        agent.updatePosition = true;

            terrain = GameObject.FindWithTag("Terrain");

            NextAiCheckTimestamp = Time.time + AiCheckDelay;

            player = GameObject.FindGameObjectWithTag("Player");

            CurrentState = ENEMY_STATE.PATROL;
        }
        
        public IEnumerator AIPatrol()
	{        
		agent.speed = patrolSpeed * selfAttr.moveSpeedMultiplier;

        if (doNotMove)
            agent.SetDestination(transform.position);
        else
            agent.SetDestination(randomWaypoint());
        
        while (currentstate == ENEMY_STATE.PATROL)
        {
            (bool foundTarget, GameObject target) = findTarget();
            if (foundTarget)
            {
                this.target = target;
                targetAttr = target.GetComponent<Attributes_scr>();
                CurrentState = ENEMY_STATE.CHASE;                         
                yield break;                
            }
            
            this.target = gameObject;
            updatePosition();
            
			yield return null;
		}
	}
        //------------------------------------------
        public IEnumerator AIChase()
        {
            agent.speed = chaseSpeed * selfAttr.moveSpeedMultiplier;
            
            while(currentstate == ENEMY_STATE.CHASE)
            {
                if (!target) //if target stops existing break back to patrol
                {
                    CurrentState = ENEMY_STATE.PATROL;
                    yield break;
                }
                
                if (canSeeTarget(target) || canHearTarget(target)) //Update position when seeing player
                    agent.destination = target.transform.position;
                //Otherwise leave it as is (last seen position applies)
                
                updatePosition();
    
                yield return null;
            }
        }
    
        //------------------------------------------
        public IEnumerator AIAttack()
        {
//            string stackTrace = StackTraceUtility.ExtractStackTrace();
//            Debug.Log(stackTrace);
            
            while(currentstate == ENEMY_STATE.ATTACK)
            {
                if (!target || targetAttr.health <= 0) //If target stopped existing or dropped below 0 health break back to patrol
                {
                     CurrentState = ENEMY_STATE.PATROL;
                     yield break;
                }
                transform.LookAt(target.transform);

                m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed);
                while (m_AiAnimatorScr.CompareCurrentState("Attack"))
                    yield return null;

                if (!target || targetAttr.health <= 0) //Check if target still exists after the animation is done
                {
                    CurrentState = ENEMY_STATE.PATROL;
                    yield break;
                }

                agent.destination = target.transform.position;
                updatePosition();
    
                yield return null;
            }
        }
	
        public void SetTarget(GameObject target)
        {
            float distToCurrentTarget = this.target == gameObject ? 999 : Vector3.Distance(transform.position, this.target.transform.position); //Set distance to new target to 999 if targetting self (bandaid)
            float distToNewTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distToNewTarget < distToCurrentTarget)
            {
                this.target = target;
                targetAttr = target.GetComponent<Attributes_scr>();
            }
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

        public void setNewDestination(Vector3 destination)
        {
            if (CurrentState == ENEMY_STATE.PATROL)
            {
                Debug.Log(gameObject.name + " heard the call and moving toward " + destination);
                agent.destination = destination;
            }
        } 
       
        private void updatePosition()
        {
            transform.LookAt(agent.nextPosition);
            if (doNotMove)
            {
                agent.SetDestination(transform.position);
                return;
            }

            float remainingDistance = Vector3.Distance(transform.position, agent.destination);
            if(remainingDistance <= agent.stoppingDistance)
            {
                if (CurrentState == ENEMY_STATE.PATROL)
                    agent.SetDestination(randomWaypoint());
                
                else if (CurrentState == ENEMY_STATE.CHASE)
                {
                    if (canSeeTarget(target) || canHearTarget(target))
                        CurrentState = ENEMY_STATE.ATTACK; //TODO This should stop this coroutine while starting new one, but causes overflow. How ?
                    else
                        CurrentState = ENEMY_STATE.PATROL;
                }
                
                m_AiAnimatorScr.Move(Vector3.zero);
                return;
            }
            
            if(CurrentState == ENEMY_STATE.ATTACK)
                CurrentState = ENEMY_STATE.CHASE;
            else
                m_AiAnimatorScr.Move(agent.desiredVelocity);
        }
        
        private Tuple<bool, GameObject> findTarget()
        {
            if(Time.time < NextAiCheckTimestamp)
                return new Tuple<bool, GameObject>(false, gameObject); //Return self if it's not time to check yet
            NextAiCheckTimestamp = Time.time + AiCheckDelay;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, FovDistance, enemiesMask);
            List<Collider> enemyList = nearbyEnemies.OrderBy(
                x => (this.transform.position - x.transform.position).sqrMagnitude
            ).ToList();
            
            while (enemyList.Count > 0)
            {
//                if(enemyList.Exists(c => c.gameObject.CompareTag("Player"))){ //Check player next if in range
//                    newTarget = player;
//                    enemyList.Remove(player.GetComponent<Collider>());
//                }

                GameObject newTarget = enemyList[0].gameObject;
                enemyList.RemoveAt(0);

                if (canHearTarget(newTarget) || canSeeTarget(newTarget))
                    return new Tuple<bool, GameObject>(true, newTarget); //Return self if it's not time to check yet
            }
            
            return new Tuple<bool, GameObject>(false, gameObject); //Return self if no target found
        }
       
        private bool canSeeTarget(GameObject target)
        {
            Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
            Vector3 direction = (new Vector3(target.transform.position.x, 1f, target.transform.position.z) -
                                origin).normalized;
            float angle = Vector3.Angle(transform.forward, direction);

            if (angle < FovAngle)
            {
                float distane = Vector3.Distance(transform.position, target.transform.position);
                if(!Physics.Raycast(origin, direction, distane, obstacleMask))
                    return true;
            }

            return false;
        }
        
        private bool canHearTarget(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance < hearingDistance && rb.velocity.magnitude != 0;
        }
    }
}
