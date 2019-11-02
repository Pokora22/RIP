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
        [SerializeField] private LayerMask enemies;        
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
        
        while (gameObject && currentstate == ENEMY_STATE.PATROL)
        {
            target = findTarget(FovAngle, FovDistance);
            if(target != gameObject){
                CurrentState = ENEMY_STATE.CHASE;                         
                yield break;                
            }

            if (agent.remainingDistance > agent.stoppingDistance)
                m_AiAnimatorScr.Move(agent.desiredVelocity);
            else if (doNotMove)
                agent.SetDestination(transform.position);
            else
                agent.SetDestination(randomWaypoint());
                
            while (agent.pathPending)
                yield return null;
            
			yield return null;
		}
	}
        //------------------------------------------
        public IEnumerator AIChase()
        {
            ///Checking stack trace
//            string stackTrace = StackTraceUtility.ExtractStackTrace();
//            Debug.Log(stackTrace);

            agent.speed = chaseSpeed * selfAttr.moveSpeedMultiplier;
            
            //Loop while chasing
            while(currentstate == ENEMY_STATE.CHASE)
            {
                if (!target)
                {
                    CurrentState = ENEMY_STATE.PATROL;
                    yield break;
                }
                    
                if (doNotMove)
                    agent.SetDestination(transform.position);
                
                else if (findTarget(FovAngle, FovDistance) != gameObject) //Update position when seeing player
                    agent.destination = target.transform.position;
                
                //Otherwise leave it as is (last seen position applies)
                
                transform.LookAt(agent.destination);
                
                float remainingDistance = Vector3.Distance(transform.position, target.transform.position);
                if(remainingDistance <= agent.stoppingDistance)
                {
                    if(findTarget(180, 5, true) != gameObject)
                        CurrentState = ENEMY_STATE.ATTACK; //TODO This should stop this coroutine while starting new one, but causes overflow. How ?
                    else
                        CurrentState = ENEMY_STATE.PATROL;
    
                    yield break;
                }
                
                m_AiAnimatorScr.Move(agent.desiredVelocity);            
    
                //Wait until next frame
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
                if (targetAttr.health <= 0) //TODO: Breaks here 
                {
                     CurrentState = ENEMY_STATE.PATROL;
                     yield break;
                }
                transform.LookAt(target.transform);

                m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed);
                while (m_AiAnimatorScr.CompareCurrentState("Attack"))
                    yield return null;
                
                agent.destination = target.transform.position;
                
                if(agent.remainingDistance > agent.stoppingDistance)
                {
                    CurrentState = ENEMY_STATE.CHASE;
                    yield break;
                }
    
                //Wait until next frame
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

        //TODO: Change distance check to sphere overlap -> iterate through [] and get list of entities in FOV angle -> target player if found in list or closest enemy
        private GameObject findTarget(float fovAngle, float fovDistance, bool forceCheck = false)
        {
            if(Time.time < NextAiCheckTimestamp && !forceCheck)
                return gameObject; //Return self if it's not time to check yet
            NextAiCheckTimestamp = Time.time + AiCheckDelay;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, fovDistance, enemies);
            Collider currentTargetCollider = target.GetComponent<Collider>();
            GameObject newTarget;
            
            List<Collider> enemyList = nearbyEnemies.OrderBy(
                x => (this.transform.position - x.transform.position).sqrMagnitude
            ).ToList();
            
            while (enemyList.Count > 0)
            {
                if (enemyList.Contains(currentTargetCollider))
                {
                    newTarget = this.target; //Check current target first if not self
                    enemyList.Remove(currentTargetCollider);
                }
//                else if(enemyList.Exists(c => c.gameObject.CompareTag("Player"))){ //Check player next if in range
//                    newTarget = player;
//                    enemyList.Remove(player.GetComponent<Collider>());
//                }                    
                else{ //Otherwise take first from nearby targets
                    newTarget = enemyList[0].gameObject;
                       
                    if (Selection.Contains (gameObject) && debug)
                    {
                        Debug.Log(gameObject.name + " : " + newTarget.name);
                        Debug.Log("Can see: " + canSeeTarget(newTarget, fovAngle) + " Can hear: " +
                                  canHearTarget(newTarget));
                    }
                    enemyList.RemoveAt(0);
                }

                if (canHearTarget(newTarget) || canSeeTarget(newTarget, fovAngle))
                {
                    targetAttr = newTarget.GetComponent<Attributes_scr>();
                    return newTarget;
                }
            }
            
            return gameObject; //set self if no targets found
        }

        private bool canSeeTarget(GameObject target, float fovAngle)
        {
            RaycastHit hit;
            
            float angle = Vector3.Angle(transform.forward, target.transform.position - transform.position);

            if (Selection.Contains(gameObject) && debug)
            {
                Debug.Log("Target angle: " + angle);
            }
            if (angle > fovAngle)
                return false;

            Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
            Vector3 destination = new Vector3(target.transform.position.x, 1f, target.transform.position.z) -
                                  origin;
                
            
            if (Selection.Contains (gameObject) && debug)
                Debug.DrawRay(origin, destination, Color.red, 2f);
            Physics.Raycast(origin, destination, out hit);
            if (hit.transform.CompareTag("Minion") || hit.transform.CompareTag("Player"))
                return true;

            return false;
        }

        private bool canHearTarget(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            
            if (Selection.Contains (gameObject) && debug)
                Debug.Log("Targets vel: " + rb.velocity.magnitude);
            if (rb.velocity.magnitude == 0)
                return false;
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance < hearingDistance;
        }
    }
}
