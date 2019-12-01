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


namespace AI
{    
    public class GuardAIControl : EnemyAIControl
    {
        [SerializeField] private float lookAroundDelay = 3f;
        [SerializeField] private Vector3 guardPosition;
        [SerializeField] private float guardLeashRange = 20f;
        private Quaternion desiredRotQ;
        
        public enum ENEMY_STATE {CHASE, ATTACK, GUARD}
        [SerializeField] private ENEMY_STATE currentstate = ENEMY_STATE.GUARD;
        
        public ENEMY_STATE CurrentState
        {
            get{return currentstate;}

            set
            {
                ENEMY_STATE oldState = currentstate;
                currentstate = value;
                AiAnimatorScr.SetAttackAnim(SelfAttr.attackSpeed, false);
                StopCoroutine(LookAround(lookAroundDelay));
                if(debug)
                    Debug.Log(gameObject.name + " going from " + oldState +  " to new state: " + currentstate);
                
                switch(currentstate)
                {   
                    case ENEMY_STATE.CHASE:                        
                        Agent.speed = chaseSpeed * SelfAttr.moveSpeedMultiplier;
                        break;

                    case ENEMY_STATE.ATTACK:
                        
                        break;
                    
                    case ENEMY_STATE.GUARD:
                        StartCoroutine(LookAround(lookAroundDelay));
                        TargetDestination = guardPosition;
                        target = gameObject;
                        break;
                }
            }
        }

        private void Start()
        {            
            base.Start();
            guardPosition = transform.position;
            CurrentState = ENEMY_STATE.GUARD;
        }

        private void Update()
        {
            UpdatePosition(TargetDestination);            

            switch (CurrentState)
            {
                case ENEMY_STATE.GUARD:
                    AiGuard();
                    break;
                
                case ENEMY_STATE.CHASE:
                    if (AiChase()) {
                        if(TargetInAttackRange())
                            CurrentState = ENEMY_STATE.ATTACK;
                    }
                    else
                    {
                        CurrentState = ENEMY_STATE.GUARD;
                    }

                    break;
                
                case ENEMY_STATE.ATTACK:
                    if (!TargetInAttackRange())
                        CurrentState = ENEMY_STATE.CHASE;
                    else if (!Agent.isStopped)
                        StartCoroutine(AiAttack());
                    break;                
            }                
        }

        private void AiGuard()
        {
            if(target != gameObject)
                CurrentState = ENEMY_STATE.CHASE;
            
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotQ, Time.deltaTime );
        }

        protected override bool AiChase()
        {
            if (Vector3.Distance(TargetDestination, guardPosition) > guardLeashRange)
            {                
                return false;
            }
            else
                return base.AiChase();
        }


        private IEnumerator LookAround(float time)
        {
            while (true)
            {
                if (InStoppingDistance())
                {
                    float direction = Random.Range(0, 360);
                    
                    desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, direction, transform.eulerAngles.z);
                }

                yield return new WaitForSeconds(time);
            }//            
        }

    }
}
