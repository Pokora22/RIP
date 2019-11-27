using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    [RequireComponent(typeof (Attributes_scr))]
    [RequireComponent(typeof (NpcAudio_scr))]
    public class PatrolAIControl : EnemyAIControl
    {
//        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
//        public AiAnimator_scr m_AiAnimatorScr; // the character we are controlling
//        public GameObject target;                                    // target to aim for
//
        private GameObject terrain;
        private Bounds levelBounds;
//        private GameObject player;
//        private Rigidbody m_Rigidbody;
//        private Coroutine currentCoroutine;
//        private Attributes_scr targetAttr, selfAttr;
        [SerializeField] private float patrolSpeed;
//        [SerializeField] private float chaseSpeed;
//        [SerializeField] private bool doNotMove, guard;
//        [SerializeField] private bool debug;
//        [SerializeField] private float lookAroundDelay = 3f;
//        [SerializeField] private float FovDistance = 30f;
//        [SerializeField] private float FovAngle = 45f;
//        [SerializeField] private float hearingDistance = 5f;
//        [SerializeField] private LayerMask enemiesMask;
//        [SerializeField] private LayerMask obstacleMask;
//        [SerializeField] private float targetScanDelay = .25f;
//        [SerializeField] private Vector3 guardPosition;
//        [SerializeField] private float guardLeashRange = 20f;
//        private Vector3 targetDestination;
//        private Quaternion desiredRotQ;
//
//        [SerializeField] private bool resetPath = false;
//        
        public enum ENEMY_STATE {PATROL, CHASE, ATTACK}
        [SerializeField] private ENEMY_STATE currentstate = ENEMY_STATE.PATROL;
        
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                ENEMY_STATE oldState = currentstate;
                currentstate = value;
                m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed, false);
                if(debug)
                    Debug.Log(gameObject.name + " going from " + oldState +  " to new state: " + currentstate);
                
                switch(currentstate)
                {                    
                    case ENEMY_STATE.PATROL:
                        agent.speed = patrolSpeed * selfAttr.moveSpeedMultiplier;
                        target = gameObject;
                        break;

                    case ENEMY_STATE.CHASE:                        
                        agent.speed = chaseSpeed * selfAttr.moveSpeedMultiplier;
                        break;

                    case ENEMY_STATE.ATTACK:
                        
                        break;
                }
            }
        }

        private void Start()
        {
            base.Start();
            terrain = GameObject.FindWithTag("Terrain");
            levelBounds = terrain.gameObject.GetComponent<Collider>().bounds;
            
            CurrentState = ENEMY_STATE.PATROL;
        }
//
//        private void Update()
//        {
//            updatePosition(targetDestination);
//            
//            if (resetPath)
//            {
//                targetDestination = randomWaypoint();
//                resetPath = false;
//            }
//
//            switch (CurrentState)
//            {
//                case ENEMY_STATE.PATROL:
//                    AIPatrol();
//                    break;
//                case ENEMY_STATE.CHASE:
//                    AIChase();
//                    break;
//                case ENEMY_STATE.ATTACK:
////                    if (!m_AiAnimatorScr.CompareCurrentState("Attacking"))
//                    if (!agent.isStopped)
//                        StartCoroutine(AIAttack());
//                    break;
//                case ENEMY_STATE.GUARD:
//                    AIGuard();
//                    break;
//            }
//            
//            if(CurrentState == ENEMY_STATE.GUARD)
//                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotQ, Time.deltaTime );
//        }
//
//        private void AIGuard()
//        {
//            if(target != gameObject)
//                CurrentState = ENEMY_STATE.CHASE;
//        }
//
//        public void AIPatrol()
//        {   
//            if(target != gameObject)
//                CurrentState = ENEMY_STATE.CHASE;
//            else if (inStoppingDistance() && !doNotMove)
//            {                
//                agent.SetDestination(randomWaypoint());
//            }
//
//            targetDestination = agent.destination;
//        }
//        
//        //------------------------------------------
//        public void AIChase()
//        {
//            if (!target || targetAttr.health <= 0) //if target stops existing break back to patrol
//                CurrentState = guard? ENEMY_STATE.GUARD : ENEMY_STATE.PATROL;
//
//            if (canSeeTarget(target) || canHearTarget(target)) //Update position only when seeing player
//            {
//                targetDestination = guard && Vector3.Distance(transform.position, guardPosition) > guardLeashRange
//                    ? transform.position
//                    : target.transform.position;
//                
//                transform.LookAt(target.transform);
//                transform.LookAt(target.transform);
//                
//                if (TargetInAttackRange())
//                    CurrentState = ENEMY_STATE.ATTACK;
//            }
//            else if(inStoppingDistance())
//                CurrentState = guard? ENEMY_STATE.GUARD : ENEMY_STATE.PATROL;
//        }
//
//        private bool TargetInAttackRange()
//        {
//            return Vector3.Distance(transform.position, target.transform.position) <= agent.stoppingDistance;
//        }
//
//        //------------------------------------------
//        public IEnumerator AIAttack()
//        {
//            //If target stopped existing or dropped below 0 health break back to patrol
//            if (target && targetAttr.health > 0)
//            {
//                transform.LookAt(target.transform);
//                
////                m_Rigidbody.MoveRotation(Quaternion.LookRotation(target.transform.position)); //TODO: Should use this, but don't understand
//
//                float animTime = m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed);
//
////                yield return new WaitForSeconds(animTime);
//                yield return new WaitUntil(() => !agent.isStopped);
//
//                //Check if target still exists after the animation is done
//                if (!target || targetAttr.health <= 0) 
//                    CurrentState = ENEMY_STATE.PATROL;
//                else
//                {
//                    targetDestination = target.transform.position;
//                    if (!TargetInAttackRange())
//                        CurrentState = ENEMY_STATE.CHASE;
//                }
//            }
//            else
//                CurrentState = ENEMY_STATE.PATROL;
//        }
//	
//        public void SetTarget(GameObject target)
//        {
//            float distToCurrentTarget = this.target && this.target != gameObject ? Vector3.Distance(transform.position, this.target.transform.position) : 999; //Set distance to new target to 999 if targetting self (bandaid)
//            float distToNewTarget = Vector3.Distance(transform.position, target.transform.position);
//            
//            if (this.target == player || distToNewTarget < distToCurrentTarget)
//            {
//                this.target = target;
//                targetAttr = target.GetComponent<Attributes_scr>();
//            }
//        }
//
//        private Vector3 randomWaypoint()
//        {
//            
//            float x = Random.Range(levelBounds.min.x, levelBounds.max.x);
//            float z = Random.Range(levelBounds.min.z, levelBounds.max.z);
//            
//            NavMeshHit hit;
//            NavMesh.SamplePosition(new Vector3(x, 0, z), out hit, 2.0f, NavMesh.AllAreas);
//            
//            NavMeshPath path = new NavMeshPath();
//            //Dirty            
//            try{
//                return agent.CalculatePath(hit.position, path) ? hit.position : randomWaypoint();
//            }
//            catch{
//                return randomWaypoint();
//            }
//        }
//
        public void setNewDestination(Vector3 destination)
        {
            if (CurrentState == ENEMY_STATE.PATROL)
            {
                Debug.Log(gameObject.name + " heard the call and moving toward " + destination);
                targetDestination = destination;
            }
        }
//       
//        private void updatePosition(Vector3 destination)
//        {
//            if (!doNotMove && agent && !agent.isStopped)
//            {
//                agent.SetDestination(destination);
//                transform.LookAt(agent.nextPosition);
//
//                if (!inStoppingDistance())
//                {
//                    m_AiAnimatorScr.Move(agent.desiredVelocity);
//                }
//                else
//                    m_AiAnimatorScr.Move(Vector3.zero);
//            }
//        }
//        
//        private bool inStoppingDistance()
//        {
//           float remainingDistance = Vector3.Distance(transform.position, targetDestination);
//
//           return remainingDistance <= agent.stoppingDistance;
//        }
//        
//        private IEnumerator findTarget(float targetScanDelay)
//        {
//            while (true)
//            {
//                while (target != gameObject)
//                {
//                    yield return new WaitForSeconds(targetScanDelay);
//                }
//                
//                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, FovDistance, enemiesMask);
//                List<Collider> enemyList = nearbyEnemies.OrderBy(
//                    x => (this.transform.position - x.transform.position).sqrMagnitude
//                ).ToList();
//
//                while (enemyList.Count > 0)
//                {
////                if(enemyList.Exists(c => c.gameObject.CompareTag("Player"))){ //Check player next if in range
////                    newTarget = player;
////                    enemyList.Remove(player.GetComponent<Collider>());
////                }
//
//                    GameObject newTarget = enemyList[0].gameObject;
//                    enemyList.RemoveAt(0);
//
//                    if (canHearTarget(newTarget) || canSeeTarget(newTarget))
//                    {
//                        target = newTarget;
//                        targetAttr = newTarget.GetComponent<Attributes_scr>();
//                        break;
//                    }
//                }
//
//                yield return new WaitForSeconds(targetScanDelay);
//            }
//        }
//       
//        private bool canSeeTarget(GameObject target)
//        {
//            Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
//            Vector3 direction = (new Vector3(target.transform.position.x, 1f, target.transform.position.z) -
//                                origin).normalized;
//            float angle = Vector3.Angle(transform.forward, direction);
//
//            if (angle < FovAngle)
//            {
//                float distance = Vector3.Distance(transform.position, target.transform.position);
//                if(!Physics.Raycast(origin, direction, distance, obstacleMask))
//                    return true;
//            }
//
//            return false;
//        }
//        
//        private bool canHearTarget(GameObject target)
//        {
//            //TODO: Ask object for it's velocity instead (from attributes)
////            Rigidbody rb = target.GetComponent<Rigidbody>();
//            
//            float distance = Vector3.Distance(transform.position, target.transform.position);
//            return distance < hearingDistance;
//            
//                //&& rb.velocity.magnitude != 0; //Any movement (might change formula someday)
//        }
//
//        private IEnumerator LookAround(float time)
//        {
//            while (true)
//            {
//                if (inStoppingDistance())
//                {
//                    float direction = Random.Range(0, 360);
//                    
//                    desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, direction, transform.eulerAngles.z);
//                }
//
//                yield return new WaitForSeconds(time);
//            }
//            
//        }
//        
//        private void OnDrawGizmos()
//        {
//            Gizmos.color = Color.white;
//            if(Application.isPlaying)
//                Gizmos.DrawWireSphere(agent.destination, 1f);
//        }
//
//        private void LockInPlace(int locked)
//        {
//            agent.isStopped = locked != 0;
//        }
    }
}
