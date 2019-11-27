using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using AI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (AiAnimator_scr))]
    public class EnemyAIControl : AIControl
    {
//        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
//        public AiAnimator_scr m_AiAnimatorScr; // the character we are controlling
//        public GameObject target;                                    // target to aim for
//
//        private GameObject terrain;
//        private Bounds levelBounds;
//        private GameObject player;
//        private Rigidbody m_Rigidbody;
//        private Coroutine currentCoroutine;
//        private Attributes_scr targetAttr, selfAttr;
//        [SerializeField] private float patrolSpeed;
//        [SerializeField] private float chaseSpeed;
//        [SerializeField] private bool doNotMove, guard;
//        [SerializeField] private bool debug;
//        [SerializeField] private float lookAroundDelay = 3f;
//        [SerializeField] private float FovDistance = 30f;
//        [SerializeField] private float FovAngle = 45f;
//        [SerializeField] private float hearingDistance = 5f;
//        [SerializeField] private LayerMask enemiesMask;
//        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float targetScanDelay = .25f;
//        [SerializeField] private Vector3 guardPosition;
//        [SerializeField] private float guardLeashRange = 20f;
//        private Vector3 targetDestination;
//        private Quaternion desiredRotQ;
//
//        [SerializeField] private bool resetPath = false;
//        
//        public enum ENEMY_STATE {PATROL, CHASE, ATTACK, GUARD}
//        [SerializeField] private ENEMY_STATE currentstate = ENEMY_STATE.PATROL;
//        
//        public ENEMY_STATE CurrentState
//        {
//            get{return currentstate;}
//
//            set
//            {
//                ENEMY_STATE oldState = currentstate;
//                currentstate = value;
//                m_AiAnimatorScr.SetAttackAnim(selfAttr.attackSpeed, false);
//                StopCoroutine(LookAround(lookAroundDelay));
//                if(debug)
//                    Debug.Log(gameObject.name + " going from " + oldState +  " to new state: " + currentstate);
//                
//                switch(currentstate)
//                {                    
//                    case ENEMY_STATE.PATROL:
//                        agent.speed = patrolSpeed * selfAttr.moveSpeedMultiplier;
//                        target = gameObject;
//                        break;
//
//                    case ENEMY_STATE.CHASE:                        
//                        agent.speed = chaseSpeed * selfAttr.moveSpeedMultiplier;
//                        break;
//
//                    case ENEMY_STATE.ATTACK:
//                        
//                        break;
//                    
//                    case ENEMY_STATE.GUARD:
//                        StartCoroutine(LookAround(lookAroundDelay));
//                        agent.speed = patrolSpeed * selfAttr.moveSpeedMultiplier;
//                        targetDestination = guardPosition;
//                        target = gameObject;
//                        break;
//                }
//            }
//        }

        protected void Start()
        {   
            base.Start();
            
            StartCoroutine(findTarget(targetScanDelay));
        }


        protected IEnumerator findTarget(float targetScanDelay)
        {
            while (true)
            {
                while (target != gameObject)
                {
                    yield return new WaitForSeconds(targetScanDelay);
                }
                
                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, FovDistance, enemiesMask);
                List<Collider> enemyList = nearbyEnemies.OrderBy(
                    x => (this.transform.position - x.transform.position).sqrMagnitude
                ).ToList();

                while (enemyList.Count > 0)
                {
                    GameObject newTarget = enemyList[0].gameObject;
                    enemyList.RemoveAt(0);

                    if (canHearTarget(newTarget) || canSeeTarget(newTarget))
                    {
                        target = newTarget;
                        targetAttr = newTarget.GetComponent<Attributes_scr>();
                        break;
                    }
                }
                
                yield return new WaitForSeconds(targetScanDelay);
            }
        }
    }
}
