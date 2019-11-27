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
        private GameObject terrain;
        private Bounds levelBounds;

        [SerializeField] private float patrolSpeed;

        public enum ENEMY_STATE {PATROL, CHASE, ATTACK}
        [SerializeField] private ENEMY_STATE currentstate = ENEMY_STATE.PATROL;
        
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                ENEMY_STATE oldState = currentstate;
                currentstate = value;
                AiAnimatorScr.SetAttackAnim(SelfAttr.attackSpeed, false);
                if(debug)
                    Debug.Log(gameObject.name + " going from " + oldState +  " to new state: " + currentstate);
                
                switch(currentstate)
                {                    
                    case ENEMY_STATE.PATROL:
                        Agent.speed = patrolSpeed * SelfAttr.moveSpeedMultiplier;
                        target = gameObject;
                        break;

                    case ENEMY_STATE.CHASE:                        
                        Agent.speed = chaseSpeed * SelfAttr.moveSpeedMultiplier;
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

        private void Update()
        {
            UpdatePosition(TargetDestination);

            switch (CurrentState)
            {
                case ENEMY_STATE.PATROL:
                    AIPatrol();
                    break;
                case ENEMY_STATE.CHASE:
                    //If still chasing
                    if (AiChase()) {
                        if(TargetInAttackRange())
                            CurrentState = ENEMY_STATE.ATTACK;
                    }
                    else
                        CurrentState = ENEMY_STATE.PATROL;
                    break;
                case ENEMY_STATE.ATTACK:
                    if (!TargetInAttackRange())
                        CurrentState = ENEMY_STATE.CHASE;
                    else if (!Agent.isStopped)
                        StartCoroutine(AiAttack());
                    break;
            }
        }

        public void AIPatrol()
        {   
            if(target != gameObject)
                CurrentState = ENEMY_STATE.CHASE;
            else if (InStoppingDistance() && !doNotMove)
            {                
                TargetDestination = randomWaypoint();                
            }
        }

        private Vector3 randomWaypoint()
        {
            
            float x = Random.Range(levelBounds.min.x, levelBounds.max.x);
            float z = Random.Range(levelBounds.min.z, levelBounds.max.z);
            
            NavMeshHit hit;
            //If navmesh fails to find a close position
            if (!NavMesh.SamplePosition(new Vector3(x, 0, z), out hit, 2f, NavMesh.AllAreas))
            {
                Debug.Log("Navmesh not sampled");
                return transform.position;
            }

            NavMeshPath path = new NavMeshPath();
            //Dirty            
            try{
                return Agent.CalculatePath(hit.position, path) ? hit.position : randomWaypoint();
            }
            catch{
                return randomWaypoint();
            }
        }

        public void setNewDestination(Vector3 destination)
        {
            if (CurrentState == ENEMY_STATE.PATROL)
            {
                Debug.Log(gameObject.name + " heard the call and moving toward " + destination);
                TargetDestination = destination;
            }
        }
    }
}
