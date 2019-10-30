using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (EnemyAnimator_scr))]
    public class EnemyAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public EnemyAnimator_scr AnimatorScr { get; private set; } // the character we are controlling
        public GameObject target;                                    // target to aim for

        private GameObject terrain;
        private GameObject player;
        private Attributes_scr targetAttr;
        [SerializeField] private float patrolSpeed;
        [SerializeField] private float chaseSpeed;
        [SerializeField] private bool doNotMove;
        [SerializeField] private bool debug;
        [SerializeField] private float AiCheckDelay = 1f;
        [SerializeField] private float FovDistance = 30f;
        [SerializeField] private float FovAngle = 45f;
        [SerializeField] private LayerMask enemies;        
        private float NextAiCheckTimestamp;

        private Vector3 randomDest;
        
        public enum ENEMY_STATE {PATROL, CHASE, ATTACK};        
        
        //------------------------------------------
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                currentstate = value;
                if(debug)
                    Debug.Log("New state: " + currentstate);

                switch(currentstate)
                {                    
                    case ENEMY_STATE.PATROL:
                        StartCoroutine(AIPatrol());
                        break;

                    case ENEMY_STATE.CHASE:                        
                        StartCoroutine(AIChase());
                        break;

                    case ENEMY_STATE.ATTACK:                        
                       StartCoroutine(AIAttack());
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
            AnimatorScr = GetComponent<EnemyAnimator_scr>();
            target = gameObject;

	        agent.updateRotation = true;
	        agent.updatePosition = true;

            terrain = GameObject.FindWithTag("Terrain");

            NextAiCheckTimestamp = Time.time + AiCheckDelay;

            player = GameObject.FindGameObjectWithTag("Player");

//            CurrentState = ENEMY_STATE.PATROL;

            randomDest = randomWaypoint();
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
            if(findTarget(FovAngle, FovDistance) != gameObject){
                CurrentState = ENEMY_STATE.CHASE;                         
                yield break;                
            }
            
            agent.isStopped = false;

            
//            Debug.Log(gameObject.name + ": " + agent.desiredVelocity);

            if (agent.remainingDistance > agent.stoppingDistance)
                AnimatorScr.Move(agent.desiredVelocity);
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
		agent.speed = chaseSpeed;        
		
		//Loop while chasing
		while(currentstate == ENEMY_STATE.CHASE)
		{   
            if(findTarget(FovAngle, FovDistance) != gameObject) //Update position when seeing player (multiplier for chase)
                    agent.destination = player.transform.position;                                    
                //Otherwise leave it as is (last seen position applies)
            
            agent.isStopped = false;			
			while(agent.pathPending)
				yield return null;
			
			//Have we reached destination?
			if(agent.remainingDistance <= agent.stoppingDistance)
			{
				//Stop agent
               agent.isStopped = true;

				if(findTarget(180, 5, true) != gameObject)
					CurrentState = ENEMY_STATE.ATTACK;
                else
                    CurrentState = ENEMY_STATE.PATROL;

				yield break;
			}
            
            if (doNotMove)
                agent.SetDestination(transform.position);
            
            AnimatorScr.Move(agent.desiredVelocity);            

			//Wait until next frame
			yield return null;
		}
	}

	//------------------------------------------
	public IEnumerator AIAttack()
	{
		//Loop while chasing and attacking
		while(currentstate == ENEMY_STATE.ATTACK)
		{
           //Chase to player position
            agent.isStopped = true;			
            transform.LookAt(player.transform);			

			//Has player run away?
			if(agent.remainingDistance > agent.stoppingDistance)
			{
				// animator.SetBool("Attacking", false);
				// while (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attacking"))
				// 	yield return null;
				//Change back to chase
				CurrentState = ENEMY_STATE.CHASE;
				yield break;
			}

			//Wait until next frame
			yield return null;
		}
	}
    
    private void Update()
    {
        
//        agent.SetDestination(player.transform.position);
        agent.SetDestination(randomDest);
//        Debug.Log(gameObject.name + "@" + transform.position + " going toward " + agent.destination + " with desired velocity of " + agent.desiredVelocity);
//
//            if (agent.remainingDistance > agent.stoppingDistance)
//                character.Move(agent.desiredVelocity, false, false);
//            else
//                CurrentState = ENEMY_STATE.PATROL;
    }


        public void SetTarget(GameObject target)
        {
            float distToCurrentTarget = Vector3.Distance(transform.position, this.target.transform.position);
            float distToNewTarget = Vector3.Distance(transform.position, target.transform.position);
            if(distToNewTarget <= distToCurrentTarget)
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


        //TODO: Change distance check to sphere overlap -> iterate through [] and get list of entities in FOV angle -> target player if found in list or closest enemy
        private GameObject findTarget(float fovAngle, float fovDistance, bool forceCheck = false)
        {
            if(target != gameObject)
                return target.gameObject;

            if(Time.time < NextAiCheckTimestamp && !forceCheck)
                return gameObject; //Return self for checking
            NextAiCheckTimestamp = Time.time + AiCheckDelay;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, fovDistance, enemies);
            GameObject newTarget;
            RaycastHit hit;
            
            List<Collider> enemyList = new List<Collider>(nearbyEnemies); //TODO: Sort by distance
            while (enemyList.Count > 0)
            {
                if(enemyList.Exists(c => c.gameObject.CompareTag("Player"))){
                    newTarget = player;
                    enemyList.Remove(player.GetComponent<Collider>());
                }                    
                else{
                    newTarget = enemyList[0].gameObject;
                    enemyList.RemoveAt(0);
                }
                
                float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);            
                if(angle > fovAngle)
                    continue;

                Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
                Vector3 destination = new Vector3(newTarget.transform.position.x, 1.5f, newTarget.transform.position.z) -
                                      origin;
                
                Physics.Raycast(origin, destination, out hit);
                
                if (hit.transform.CompareTag("Minion") || hit.transform.CompareTag("Player"))
                {
                    targetAttr = hit.transform.GetComponent<Attributes_scr>();
                    if(debug)
                        Debug.Log("Target: " + newTarget.gameObject.name);
                    return newTarget; 
                }
            }

            return gameObject; //Don't change if no targets found
        }

        private bool CanSeePlayer(float fovAngle, float fovDistance, bool forceCheck = false){
            if(Time.time < NextAiCheckTimestamp && !forceCheck)
                return false;
            NextAiCheckTimestamp = Time.time + AiCheckDelay;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if(distance > fovDistance)
                return false;

            float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);            
            if(angle > fovAngle)
                return false;

            RaycastHit hit;
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Vector3 destination = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z) - origin;
            Debug.DrawRay(origin, destination, Color.red, 1f);
            if(Physics.Raycast(origin, destination, out hit))         
                if(!hit.transform.CompareTag("Player"))
                    return false;                                    

            return true;
        }
    }
}
