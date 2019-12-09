using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AI;
using UnityEngine.Events;

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
        [SerializeField] private GameObject destinationIndicatorPrefab;
        
        private Attributes_scr targetAttr;
        private EnemyAIControl targetAI;
        private AiAnimator_scr m_AiAnimatorScr;
        private NpcAudio_scr audioPlayer;
        private bool recalled = false;
        private bool dmgRoutineRunning = false;
        private bool targettingDestructible = false;
        private PlayerController_scr summoner;
        private Attributes_scr selfAttributes;
        private Vector3 m_AdvanceDestination, targetDestination;
        private GameObject destinationIndicator;
        private Renderer destIndicatorRenderer;
        
        public MINION_STATE CurrentState
        {
            get { return currentState; }

            set
            {
                if(debug)
                    Debug.Log(gameObject.name + " coming from: " + currentState);
                
                currentState = value;
                agent.isStopped = false;
                m_AiAnimatorScr.SetAttackAnim(selfAttributes.attackSpeed, false);
                targettingDestructible = false;
                
                switch (currentState)
                {
                    case MINION_STATE.FOLLOW:
                        target = player;
                        summoner.minionReturn(this);
                        agent.stoppingDistance = playerFollowowDistance;
                        break;
                    
                    case MINION_STATE.ADVANCE:
                        destIndicatorRenderer.material.SetColor("_Color", Color.yellow);
                        targetDestination = m_AdvanceDestination;
                        SeekDestructible();
                        summoner.minionLeave(this);
                        break;
                    
                    case MINION_STATE.CHASE:
                        destIndicatorRenderer.material.SetColor("_Color", Color.red);
                        targettingDestructible = target && !target.CompareTag("Enemy");
//                        if (targettingDestructible)
//                            setDestructibleDestination();
                        agent.stoppingDistance = targettingDestructible ? 
                            destructibleStoppingDistance : enemyStoppingDistance; //Conditional distance depending on if target is enemy or destructible?
                        summoner.minionLeave(this);
                        break;
                    
                    case MINION_STATE.ATTACK:
                        targettingDestructible = target && !target.CompareTag("Enemy");
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
            selfAttributes = gameObject.GetComponent<Attributes_scr>();
            agent.speed *= selfAttributes.moveSpeedMultiplier;

            gameObject.name = "Minion " + (player.GetComponent<PlayerController_scr>().minions.Count  + player.GetComponent<PlayerController_scr>().minionsAway.Count);
            
            CurrentState = MINION_STATE.FOLLOW;

            destinationIndicator = Instantiate(destinationIndicatorPrefab);
            destIndicatorRenderer = destinationIndicator.GetComponent<Renderer>();

            audioPlayer = GetComponent<NpcAudio_scr>();
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.RAISE);            
        }

        private void Update()
        {
            UpdatePosition(targetDestination);
            
            findTarget();
            
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
                    if (!agent.isStopped)
                        StartCoroutine(minionAttack());

                    break;
                default:
                       break;
            }
        }

        private void minionFollow()
        {
            targetDestination = player.transform.position;
        }
        
        public void SendToDestination(Vector3 destination, bool obstacleHit, RaycastHit rayHit)
        {
            if (obstacleHit)
                m_AdvanceDestination = new Vector3(rayHit.point.x, transform.position.y, rayHit.point.z);
            else
                m_AdvanceDestination = destination;
            
            CurrentState = MINION_STATE.ADVANCE;
        }

        private void minionAdvance()
        {
            if (inStoppingDistance())
                CurrentState = MINION_STATE.FOLLOW;
        }

        private bool inStoppingDistance()
        {
            float remainingDistance = Vector3.Distance(transform.position, agent.destination);
            
            //Bandaid for remaining distance being different on agent and manual calculation. NavMesh agent sucks ??
//            remainingDistance = remainingDistance < agent.remainingDistance
//                ? remainingDistance
//                : agent.remainingDistance;            

            return remainingDistance <= agent.stoppingDistance;
        }

        private void minionChase()
        {
            //Return to player if enemies run out too far from player or die
            Debug.Log(name + " target: " + target);
            if (!target || targetAttr.health <= 0 || Vector3.Distance(transform.position, player.transform.position) > playerLeashRange)
                CurrentState = MINION_STATE.FOLLOW;
            else
            {
                //Need to only update destination for non destructible targets
                if (!targettingDestructible)
                {                    
                    targetDestination = target.transform.position;
                }

                if (inStoppingDistance())
                    CurrentState = MINION_STATE.ATTACK;
            }
        }

        private void setDestructibleDestination()
        {
            Collider target = this.target.GetComponent<Collider>();
            
            targetDestination = target.ClosestPointOnBounds(transform.position);            
        }

        private IEnumerator minionAttack()
        {
            //Check if target exists first
            if (target && targetAttr.health > 0)
            {
                Vector3 lookTarget = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
                transform.LookAt(lookTarget, Vector3.up);
                if(target.CompareTag("Enemy") && targetAI.target != gameObject)
                    targetAI.SetTarget(this.gameObject);
                
                //Start attack animation
                float animLength = m_AiAnimatorScr.SetAttackAnim(selfAttributes.attackSpeed);

//                Debug.Log("Waiting for " + animLength);
                //Wait for animation to finish
//                yield return new WaitForSeconds(animLength);

                yield return new WaitUntil(() => !agent.isStopped);

                //Check if target still exists
                if (!target || targetAttr.health <= 0)
                    CurrentState = MINION_STATE.FOLLOW;
                else
                {
                    //Update destination and check if target is too far for an attack
                    if (!targettingDestructible)
                    {                        
                        targetDestination = target.transform.position;
                        if (!inStoppingDistance())
                            CurrentState = MINION_STATE.CHASE;
                    }
                }
            }
            else
                CurrentState = MINION_STATE.FOLLOW;
        }


        private void UpdatePosition(Vector3 destination)
        {
            agent.SetDestination(destination);

            float y = CurrentState == MINION_STATE.FOLLOW ? -1f : .1f;
            destinationIndicator.transform.position = new Vector3(destination.x, y, destination.z);

            if (!agent.pathPending)
            {
                //https://docs.unity3d.com/ScriptReference/Rigidbody.SweepTest.html
                //TODO: Check above for possible solution for AIs walking into each other

                if (!inStoppingDistance())
                {
                    m_AiAnimatorScr.Move(agent.desiredVelocity);
                }
                else
                {
                    m_AiAnimatorScr.Move(Vector3.zero);
//                    recalled = false;
                }
            }
        }

        private void SeekDestructible()
        {
            Vector3 scanDir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, enemyDetectionRange,
                scanDir, playerLeashRange, destructiblesMask, QueryTriggerInteraction.Ignore);
            
            for (int i = 0; i < hits.Length; i++)
            {                
                Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
                Vector3 destination = new Vector3(hits[i].point.x, 1f, hits[i].point.z);
                float distance = Vector3.Distance(origin, destination);
                GameObject newTarget = hits[i].transform.gameObject;                
                
                RaycastHit hit;
                if (!Physics.Raycast(origin, destination - origin, out hit, distance, obstaclesMask))
                {
                    Debug.DrawRay(origin, destination - origin, Color.blue, 2f);
                    this.target = newTarget;
                    targetAttr = newTarget.GetComponent<Attributes_scr>();
                    targetDestination = hits[i].point;
                    CurrentState = MINION_STATE.CHASE;
                }
            }            
        }

        //TODO: Might need to split this into two routines - low and high priority and cycle them when changing states
        private void findTarget()
        {        
            if (!recalled)
            {
                //Wait if minion is already targeting something
                if (!target || target.CompareTag("Enemy"))
                    return;

                Collider[] nearbyEnemies =
                    Physics.OverlapSphere(transform.position, enemyDetectionRange, enemiesMask);
                List<Collider> enemyList = nearbyEnemies.OrderBy(
                    x => (this.transform.position - x.transform.position).sqrMagnitude
                ).ToList();

                while (enemyList.Count > 0)
                {
                    GameObject newTarget = enemyList[0].gameObject;
                    enemyList.RemoveAt(0);

                    Vector3 origin = new Vector3(transform.position.x, 1f, transform.position.z);
                    Vector3 destination =
                        (new Vector3(newTarget.transform.position.x, 1f, newTarget.transform.position.z));
                    //Dirty edit to offset destructible walls inner lining
                    float distance = Vector3.Distance(origin, destination) - .2f;

                    Debug.DrawRay(origin, destination - origin, Color.red, 1f);

                    RaycastHit hit;
                    if (!Physics.Raycast(origin, destination - origin, out hit, distance, obstaclesMask))
                    {
                        Debug.DrawRay(origin, destination - origin, Color.blue, 2f);
                        this.target = newTarget;
                        targetAttr = newTarget.GetComponent<Attributes_scr>();                        
                        targetAI = newTarget.GetComponent<EnemyAIControl>();
                        CurrentState = MINION_STATE.CHASE;
                        return;                        
                    }
                }                
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

            yield return new WaitForSeconds(.5f);

            while (!inStoppingDistance() && CurrentState != MINION_STATE.ADVANCE)
                yield return null;
            
            recalled = false;
        }

        public void ModifyAttributes(float attack = 0, float moveSpeed = 0, float attackSpeed = 0, float health = 0)
        {
            selfAttributes.health += health;
            selfAttributes.maxHealth += health;
            selfAttributes.moveSpeedMultiplier += moveSpeed;
            selfAttributes.attackSpeed += moveSpeed;
            selfAttributes.attackSpeed += attackSpeed;
            selfAttributes.attackDamage += attack;
        }
        
        private void LockInPlace(int locked)
        {
            agent.isStopped = locked != 0;
        }

        private void OnDestroy()
        {
            Destroy(destinationIndicator);
        }
    }
}
