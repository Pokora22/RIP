using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    public class SummonAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK, NONE}

        [SerializeField] private MINION_STATE currentState;
        [SerializeField] private GameObject player;
        [SerializeField] private LayerMask enemiesMask, obstaclesMask, destructiblesMask;        
        [SerializeField] private float playerFollowowDistance = 2f;
        [SerializeField] private float enemyStoppingDistance = 1f;
        [SerializeField] private float destructibleStoppingDistance = .5f;
        [SerializeField] private float enemyDetectionRange = 10f;        
        [SerializeField] private float playerLeashRange = 15f;
        [SerializeField] private bool debug = true;
        [SerializeField] private GameObject target;
        private Attributes_scr targetAttr;
        private EnemyAIControl targetAI;
        [SerializeField] private float enemySearchDelay = 1f;
        
        private AiAnimator_scr m_AiAnimatorScr;
        private NpcAudio_scr audioPlayer;
        private bool recalled = false;
        private bool dmgRoutineRunning = false;
        private bool targettingDestructible = false;
        private PlayerController_scr summoner;
        private Attributes_scr minionAttributes;
        private Vector3 m_AdvanceDestination, targetDestination;

        private Coroutine targetScan;
        
        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                if(debug)
                    Debug.Log(gameObject.name + " coming from: " + currentState);
                
                currentState = value;
                agent.isStopped = false;
                StopCoroutine(setDestructibleDestination(GetComponent<Collider>()));
                
                switch (currentState)
                {
                    case MINION_STATE.FOLLOW:
                        target = player;
                        summoner.minionReturn(this);
                        agent.stoppingDistance = playerFollowowDistance;
                        targettingDestructible = false;
                        break;
                    
                    case MINION_STATE.ADVANCE:
                        targetDestination = m_AdvanceDestination;
                        summoner.minionLeave(this);
                        recalled = false;
                        break;
                    
                    case MINION_STATE.CHASE:
                        targettingDestructible = !target.CompareTag("Enemy");
                        if (targettingDestructible)
                            StartCoroutine(setDestructibleDestination(target.GetComponent<Collider>()));
                        agent.stoppingDistance = targettingDestructible ? 
                            destructibleStoppingDistance : enemyStoppingDistance; //Conditional distance depending on if target is enemy or destructible?
                        summoner.minionLeave(this);
                        break;
                    
                    case MINION_STATE.ATTACK:
                        
                        break;
                }
                
                if(debug)
                    Debug.Log(gameObject.name + " going into: " + currentState);
            }
        }

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            m_AiAnimatorScr = GetComponent<AiAnimator_scr>();

	        agent.updateRotation = true;
	        agent.updatePosition = true;

            player = GameObject.FindWithTag("Player");
            target = player;
            summoner = player.GetComponent<PlayerController_scr>();
            minionAttributes = gameObject.GetComponent<Attributes_scr>();

            gameObject.name = "Minion " + (player.GetComponent<PlayerController_scr>().minions.Count  + player.GetComponent<PlayerController_scr>().minionsAway.Count);
            
            CurrentState = MINION_STATE.FOLLOW;

            audioPlayer = GetComponent<NpcAudio_scr>();
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.RAISE);
            
            targetScan = StartCoroutine(findTarget(.25f));
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case MINION_STATE.FOLLOW:
                    minionFollow();
                    break;
                case MINION_STATE.ADVANCE:
                    minionAdvance();
                    break;
                case MINION_STATE.CHASE:
                    minionChase();
                    break;
                case MINION_STATE.ATTACK:
                    //Check if already mid attack animation
                    if (!m_AiAnimatorScr.CompareCurrentState("Attack"))
                        StartCoroutine(minionAttack());
                    break;
                default:
                       break;
            }
            
            UpdatePosition(targetDestination);
        }

        private void minionFollow()
        {
            //Check if there's a new target
            if (!recalled && target != player)
                CurrentState = MINION_STATE.CHASE;
            //Update destination to player position
            else
                targetDestination = player.transform.position;
        }
        
        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
            recalled = false;
            
            if (obstacleHit)
                m_AdvanceDestination = new Vector3(rayHit.point.x, transform.position.y, rayHit.point.z);
            else
                m_AdvanceDestination = destination;
            
            CurrentState = MINION_STATE.ADVANCE;
        }

        private void minionAdvance()
        {
            if (target != player)
                CurrentState = MINION_STATE.CHASE;
            else if (inStoppingDistance())
                CurrentState = MINION_STATE.FOLLOW;
        }

        private bool inStoppingDistance()
        {
            float remainingDistance = Vector3.Distance(transform.position, targetDestination);
            Debug.Log(remainingDistance);
            Debug.Log(targetDestination);
            Debug.Log(transform.position);
            return remainingDistance <= agent.stoppingDistance;
        }

        private void minionChase()
        {
            //Return to player if enemies run out too far from player or die
            if (!target || targetAttr.health <= 0 || Vector3.Distance(transform.position, player.transform.position) > playerLeashRange)
                CurrentState = MINION_STATE.FOLLOW;
            else
            {
                //Need to only update destination for non destructible targets
                if (!targettingDestructible)
                    targetDestination = target.transform.position;

                if (inStoppingDistance())
                    CurrentState = MINION_STATE.ATTACK;
            }
        }

        private IEnumerator setDestructibleDestination(Collider target)
        {
            while (true)
            {
//                Vector3 closestPoint = target.ClosestPointOnBounds(transform.position);
                targetDestination = target.ClosestPointOnBounds(transform.position);
                
                yield return new WaitForSeconds(1f); //Update location once a second
            }
        }

        private IEnumerator minionAttack()
        {
            agent.isStopped = true;
            
            //Check if target exists first
            if (target && targetAttr.health > 0)
            {
                transform.LookAt(target.transform);
                if(target.CompareTag("Enemy") && targetAI.target != gameObject)
                    targetAI.SetTarget(this.gameObject);

                //Start attack animation
                float animLength = m_AiAnimatorScr.SetAttackAnim(minionAttributes.attackSpeed);

                //Wait for animation to finish
                yield return new WaitForSeconds(animLength);

                //Check if target still exists
                if (!target || targetAttr.health <= 0)
                    CurrentState = MINION_STATE.FOLLOW;
                else
                {
                    //Update destination and check if target is too far for an attack
                    targetDestination = target.transform.position;
                    if (!inStoppingDistance())
                        CurrentState = MINION_STATE.CHASE;
                }
            }
            else
                CurrentState = MINION_STATE.FOLLOW;

            agent.isStopped = false;
        }


        private void UpdatePosition(Vector3 destination)
        {
            agent.SetDestination(destination);
            
            if(!inStoppingDistance())
            {
                m_AiAnimatorScr.Move(agent.desiredVelocity);
            }
            else
            {
                m_AiAnimatorScr.Move(Vector3.zero);
                recalled = false;
            }
        }

        //TODO: Might need to split this into two routines - low and high priority and cycle them when changing states
        private IEnumerator findTarget(float targetScanDelay)
        {
            //Use scanDelay time on low priority search
            float lowPriorityDelay = targetScanDelay;
            //Use average frame time when advancing instead (high priority search)
            float highPriorityDelay = Time.smoothDeltaTime;
            
            while (true)
            {
                if (!recalled)
                {
                    //Wait if minion is already targeting something
                    while (target != player)
                    {
                        yield return new WaitForSeconds(lowPriorityDelay);
                    }

                    Collider[] nearbyEnemies =
                        Physics.OverlapSphere(transform.position, enemyDetectionRange, enemiesMask);
                    List<Collider> enemyList = nearbyEnemies.OrderBy(
                        x => (this.transform.position - x.transform.position).sqrMagnitude
                    ).ToList();

                    if (enemyList.Count == 0 && CurrentState == MINION_STATE.ADVANCE)
                    {
                        nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, destructiblesMask);
                        enemyList = nearbyEnemies.OrderBy(
                            x => (this.transform.position - x.transform.position).sqrMagnitude
                        ).ToList();
                    }

                    while (enemyList.Count > 0)
                    {
                        GameObject newTarget = enemyList[0].gameObject;
                        enemyList.RemoveAt(0);

                        Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
                        Vector3 destination =
                            (new Vector3(newTarget.transform.position.x, 1f, newTarget.transform.position.z));
                        float distance = Vector3.Distance(origin, destination);

                        Debug.DrawRay(origin, destination - origin, Color.red, 1f);

                        if (!Physics.Raycast(origin, destination - origin, distance, obstaclesMask))
                        {
                            Debug.DrawRay(origin, destination - origin, Color.blue, 2f);
                            this.target = newTarget;
                            targetAttr = newTarget.GetComponent<Attributes_scr>();
                            if (newTarget.CompareTag("Enemy"))
                                targetAI = newTarget.GetComponent<EnemyAIControl>();
                        }
                    }
                }
                //Search more often when minion is advancing forward
                yield return CurrentState == MINION_STATE.ADVANCE ? new WaitForSeconds(highPriorityDelay) : new WaitForSeconds(lowPriorityDelay);
            }
        }

        private void OnDrawGizmos()
        {
            if(!target.gameObject)
                return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(agent.destination, 1f);
        
            Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
            Vector3 destination = new Vector3(target.transform.position.x, 1.5f, target.transform.position.z) - origin;
        
            Gizmos.DrawRay(origin, destination);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        }

        public IEnumerator recall()
        {
            if (!recalled)
            {
                recalled = true;
                CurrentState = MINION_STATE.FOLLOW;
            }

//            yield return new WaitForSeconds(recallDelay);
//            recalled = false;
            yield break;
        }
    }
}
