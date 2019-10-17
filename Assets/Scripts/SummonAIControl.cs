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
    [RequireComponent(typeof (SummonAnimator_scr))]
    public class SummonAIControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public enum MINION_STATE{FOLLOW, ADVANCE, CHASE, ATTACK}

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
        
        private SummonAnimator_scr AnimatorScr;
        private Coroutine currentCoroutine;
        private NpcAudio_scr audioPlayer;
        private bool recalled = false;
        private bool dmgRoutineRunning = false;
        private SummonControl_scr summoner;
        private Attributes_scr minionAttributes;
        private Vector3 m_AdvanceDestination;
        private bool targetDead;
        private float nextEnemySearchTime;
        private Attributes_scr enemyAttr;
        

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
                }
                
                if(debug)
                    Debug.Log(gameObject.name + " going into: " + currentState);
            }
        }

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            AnimatorScr = GetComponent<SummonAnimator_scr>();

	        agent.updateRotation = true;
	        agent.updatePosition = true;

            player = GameObject.FindWithTag("Player");
            summoner = player.GetComponent<SummonControl_scr>();
            minionAttributes = gameObject.GetComponent<Attributes_scr>();

            gameObject.name = "Minion " + (player.GetComponent<SummonControl_scr>().minions.Count  + player.GetComponent<SummonControl_scr>().minionsAway.Count);

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
            AnimatorScr.SetAttackAnim(false, minionAttributes.attackSpeed);
            
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

        private IEnumerator minionAdvance()
        {
            summoner.minionLeave(gameObject);
            agent.stoppingDistance = .2f;

            while (currentState == MINION_STATE.ADVANCE)
            {
                while (agent.pathPending)
                    yield return null;
                
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
                    if (target.CompareTag("Destructible") || target.CompareTag("Barricade"))
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
            AnimatorScr.SetAttackAnim(false, minionAttributes.attackSpeed);
            
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
                if (!target)
                {
                    CurrentState = MINION_STATE.FOLLOW;
                    yield break;
                }

                float attackLength = AnimatorScr.SetAttackAnim(true, minionAttributes.attackSpeed);
                yield return new WaitForSeconds(attackLength); //TODO: Sync with animation instead and then have a cooldown + animation speed based on attack speed formula
                
                agent.SetDestination(target.transform.position);

                if (agent.remainingDistance > agent.stoppingDistance)
                {                    
                    CurrentState = MINION_STATE.CHASE;
                    yield break;
                }

                while (AnimatorScr.CompareCurrentState("Attacking"))
                    yield return null;
                
                yield return null;
            }
        }


        private void UpdatePosition(Vector3 destination)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 1f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);

            if (agent.remainingDistance > agent.stoppingDistance)
                AnimatorScr.Move(agent.desiredVelocity);
            else
                AnimatorScr.Move(Vector3.zero);
        }

        private GameObject findTargetEnemy() //TODO: Include destructibles later
        {
            if (Time.time < nextEnemySearchTime)
                return player;
            nextEnemySearchTime += enemySearchDelay;
            
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyDetectionRange, enemies);
            GameObject newTarget;
            RaycastHit hit;
            
            List<Collider> enemyList = new List<Collider>(nearbyEnemies);
            while (enemyList.Count > 0 && !recalled) //TODO: Make a list instead and choose closest target and switch if not currently attacking instead (sort by distance?)
            {
//                int index = Random.Range(0, enemyList.Count - 1); 
                newTarget = enemyList[0].gameObject;
                enemyList.RemoveAt(0);
                
                Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
                Vector3 destination = new Vector3(newTarget.transform.position.x, 1.5f, newTarget.transform.position.z) -
                                      origin;
                
                Physics.Raycast(origin, destination, out hit);
                
                if (hit.transform.CompareTag("Enemy"))
                {
                    enemyAttr = hit.transform.GetComponent<Attributes_scr>(); //Expensive but called rarely (comparatively)
                    return newTarget; 
                }
            }

            return target; //Don't change if no enemies found
        }

        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
            recalled = false;

            if (obstacleHit) {//TODO: Can't. This changes target to cube (world) and minions are confused (try to pathfind to center)
                if (rayHit.transform.gameObject.CompareTag("Destructible"))
                {
                    target = rayHit.transform.gameObject; //Change target to the destructible
                    CurrentState =
                        MINION_STATE.CHASE; //Might be a problem again with distance and size of destructibles
                }
                else
                {
                    m_AdvanceDestination = rayHit.point;
                    CurrentState = MINION_STATE.ADVANCE;
                }
            }
            else
            {
                m_AdvanceDestination = destination;
                CurrentState = MINION_STATE.ADVANCE;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(agent.destination, 1f);
        
            Vector3 origin = new Vector3(transform.position.x, 1.5f, transform.position.z);
            Vector3 destination = new Vector3(target.transform.position.x, 1.5f, target.transform.position.z) - origin;
        
            Gizmos.DrawRay(origin, destination);
        }
        
        public IEnumerator recall()
        {
            if (recalled)
                yield break;
            
            recalled = true;
            CurrentState = MINION_STATE.FOLLOW;
            yield return new WaitForSeconds(recallDelay);
            recalled = false;
        }

        public void MinionAttack() //called by animation
        {
            Debug.Log("This was called from animation");
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.ATTACK);
            
            if(Vector3.Distance(transform.position, target.transform.position) <= agent.stoppingDistance)
                enemyAttr.damage(minionAttributes.attackDamage, minionAttributes);
        }

        public void setTargetDead(bool dead)
        {
            targetDead = dead;
        }
    }
}
