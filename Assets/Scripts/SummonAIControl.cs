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
        [SerializeField] private LayerMask enemies;        
        [SerializeField] private float playerFollowowDistance = 2f;
        [SerializeField] private float enemyFollowDistance = 1f;
        [SerializeField] private float enemyDetectionRange = 10f;        
        [SerializeField] private float playerLeashRange = 15f;
        [SerializeField] private float recallDelay = 3f;
        [SerializeField] private bool debug = true;
        [SerializeField] private GameObject target;
        [SerializeField] private float enemySearchDelay = 1f;
        
        private AiAnimator_scr m_AiAnimatorScr;
        private Coroutine currentCoroutine;
        private NpcAudio_scr audioPlayer;
        private bool recalled = false;
        private bool dmgRoutineRunning = false;
        private PlayerController_scr summoner;
        private Attributes_scr minionAttributes;
        private Vector3 m_AdvanceDestination;
        private bool targetDead;
        private float nextEnemySearchTime;
        private Attributes_scr targetAttr;

        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                if(debug)
                    Debug.Log(gameObject.name + " coming from: " + currentState);
                
                currentState = value;
                agent.isStopped = false;

                if(currentCoroutine != null)
                    StopCoroutine(currentCoroutine);
                
                switch (currentState)
                {
                    case MINION_STATE.FOLLOW:
                        currentCoroutine = StartCoroutine(minionFollow());
                        break;
                    case MINION_STATE.ADVANCE:
                        currentCoroutine = StartCoroutine(minionAdvance());
                        break;
                    case MINION_STATE.CHASE:
                        currentCoroutine = StartCoroutine(minionChase());
                        break;
                    case MINION_STATE.ATTACK:
                        currentCoroutine = StartCoroutine(minionAttack());
                        break;
                    case MINION_STATE.NONE:
                        currentCoroutine = StartCoroutine(minionDoNothing());
                        break;
                }
                
                if(debug)
                    Debug.Log(gameObject.name + " going into: " + currentState);
            }
        }

        private IEnumerator minionDoNothing()
        {
            yield break;
        }

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            m_AiAnimatorScr = GetComponent<AiAnimator_scr>();

	        agent.updateRotation = true;
	        agent.updatePosition = true;

            player = GameObject.FindWithTag("Player");
            summoner = player.GetComponent<PlayerController_scr>();
            minionAttributes = gameObject.GetComponent<Attributes_scr>();

            gameObject.name = "Minion " + (player.GetComponent<PlayerController_scr>().minions.Count  + player.GetComponent<PlayerController_scr>().minionsAway.Count);

            // StartCoroutine(recall());
            CurrentState = MINION_STATE.FOLLOW;

//            StartCoroutine(stateDebug());

            audioPlayer = GetComponent<NpcAudio_scr>();
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.RAISE);

            nextEnemySearchTime = Time.time + enemySearchDelay;
        }

        private IEnumerator minionFollow()
        {
            target = player;
            summoner.minionReturn(this.gameObject);
            agent.stoppingDistance = playerFollowowDistance;
//            m_AiAnimatorScr.SetAttackAnim(false, minionAttributes.attackSpeed);
            
            while (currentState == MINION_STATE.FOLLOW)
            {
                while (agent.pathPending)
                    yield return null;
                
                target = findTargetEnemy();
                
                if (target != player)
                {
                    CurrentState = MINION_STATE.CHASE;
                    yield break;
                }
                
                UpdatePosition(player.transform.position);
                yield return null;
            }
        }
        
        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
            recalled = false;
            
            if (obstacleHit) 
                m_AdvanceDestination = rayHit.point;
            else
                m_AdvanceDestination = destination;
            
            CurrentState = MINION_STATE.ADVANCE;
        }

        private IEnumerator minionAdvance()
        {
            summoner.minionLeave(gameObject);
            agent.stoppingDistance = .2f;
            recalled = false;

            while (currentState == MINION_STATE.ADVANCE)
            {
                while (agent.pathPending)
                    yield return null;
                
                //TODO: Add search for obstacles
                target = findTargetEnemy();
                
                if (target != player)
                {                    
                    CurrentState = MINION_STATE.CHASE;
                    yield break;
                }
                
                UpdatePosition(m_AdvanceDestination);
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    //If there's a target?
                    if (target.CompareTag("Destructible") || target.CompareTag("Barricade")) //This is redundant ?
                        CurrentState = MINION_STATE.ATTACK;                     
                    else
                        CurrentState = MINION_STATE.FOLLOW;
                }
                
                yield return null;
            }
        }

        private IEnumerator minionChase()
        {
            agent.stoppingDistance = enemyFollowDistance; //Conditional distance depending on if target is enemy or destructible?
            
            summoner.minionLeave(gameObject);
//            m_AiAnimatorScr.SetAttackAnim(false, minionAttributes.attackSpeed);
            
            while (currentState == MINION_STATE.CHASE)
            {
                while (agent.pathPending)
                    yield return null;
                
                //Return to player if enemies run out too far from player
                if (targetDead || Vector3.Distance(transform.position, player.transform.position) > playerLeashRange)
                {
                    CurrentState = MINION_STATE.FOLLOW;
                    yield break;                 
                }

                UpdatePosition(target.transform.position);
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {                    
                    CurrentState = MINION_STATE.ATTACK;
                }
                yield return null;
            }
        }

        private IEnumerator minionAttack()
        {
            agent.isStopped = true;

            /*
            if (target.CompareTag("Enemy"))
                agent.stoppingDistance = enemyFollowDistance;
            else if (target.CompareTag("Destructible") || target.CompareTag("Barricade"))
                agent.stoppingDistance = 1.6f; //Destructible attack distance
            */
            
            while (currentState == MINION_STATE.ATTACK)
            {
                if (!target || targetAttr.health <= 0)
                {
                    CurrentState = MINION_STATE.FOLLOW;
                    yield break;
                }
                
                transform.LookAt(target.transform);
                m_AiAnimatorScr.Move(Vector3.zero);
                
                m_AiAnimatorScr.SetAttackAnim(minionAttributes.attackSpeed);
                while (m_AiAnimatorScr.CompareCurrentState("Attack"))
                    yield return null;

                agent.SetDestination(target.transform.position);                

                if (agent.remainingDistance > agent.stoppingDistance)
                {                    
                    CurrentState = MINION_STATE.CHASE;
                    yield break;
                }
                
                yield return null;
            }
        }


        private void UpdatePosition(Vector3 destination)
        {
            agent.SetDestination(destination);

            float remainingDistance = Vector3.Distance(transform.position, target.transform.position);
            if (remainingDistance > agent.stoppingDistance)
                m_AiAnimatorScr.Move(agent.desiredVelocity);
            else
            {
                m_AiAnimatorScr.Move(Vector3.zero);
                recalled = false;
            }
        }

        private GameObject findTargetEnemy()
        {
            if (Time.time < nextEnemySearchTime || recalled)
                return target;
            nextEnemySearchTime += enemySearchDelay;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, enemies);
            GameObject newTarget;
            RaycastHit hit;
            
            List<Collider> enemyList = new List<Collider>(nearbyEnemies);
            while (enemyList.Count > 0) //TODO: Make a list instead and choose closest target and switch if not currently attacking instead (sort by distance?)
            {
//                int index = Random.Range(0, enemyList.Count - 1); 
                newTarget = enemyList[0].gameObject;
                enemyList.RemoveAt(0);
                
                Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
                Vector3 direction = new Vector3(newTarget.transform.position.x, 1f, newTarget.transform.position.z) -
                                      origin;
                
                Physics.Raycast(origin, direction, out hit);
                Debug.DrawRay(origin, direction, Color.red, 2f);
                
                if (hit.transform.CompareTag("Enemy")){
                    
                    targetAttr = hit.transform.GetComponent<Attributes_scr>(); //Expensive but called rarely (comparatively)
                    return newTarget; 
                }
            }

            return target; //Don't change if no enemies found
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


        public void setTargetDead(bool dead)
        {
            targetDead = dead;
        }
    }
}
