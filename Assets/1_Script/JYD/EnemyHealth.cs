﻿using System;
using Unity.Behavior;
using UnityEngine;
using Action = System.Action;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour , IDamageble
{
    public float maxHealth;
    public float currentHealth;
    
    public BehaviorGraphAgent BehaviorGraphAgent;
    
    [Header("Animation info")]
    public BossAnimationController BossAnimationController;
    public Animator Animator;
    [SerializeField] private ChangeState change;
    
    [Header("Guard info")]
    public bool isGuarding;
    public int maxGuardCount;
    private int guardCount;
    
    public event Action<ActionData> OnHitEvent; 
    public event Action OnDeadEvent; 
    
    private void Start()
    {
        guardCount = maxGuardCount;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage();
        }
    }
    
    public void TakeDamage()
    {
        if (currentHealth <= 0)
        {
            TriggerState(BossState.Dead , 0);
            OnDeadEvent?.Invoke();
            return;
        }
        
        if (isGuarding)
        {
            HandleGuard();
        }
        else
        {
            HandleNonGuard();
        }
    }

    private void HandleGuard()
    {
        guardCount--;
        if (guardCount > 0)
        {
            Animator.SetTrigger("GuardHit");
            Animator.SetInteger("GuardHitType", Random.Range(0, 2));
        }
        else
        {
            Animator.SetTrigger("GuardBreak");
        }
    }

    private void HandleNonGuard()
    {
        if (Random.value >= 0)
        {
            TriggerState(BossState.Hurt, 10);
        }
        else
        {
            isGuarding = true;
            TriggerState(BossState.Guard, 5);
        }

        OnHitEvent.Invoke(GetHealthPercent());
    }

    private void TriggerState(BossState state, int damage)
    {
        BehaviorGraphAgent.SetVariableValue("BossState", state);
        change.SendEventMessage(state);
        currentHealth -= damage;
    }

    private void TriggerGroggyState()
    {
        TriggerState(BossState.Groggy, 0);
    }

    public void OffGuarding()
    {
        guardCount = maxGuardCount;
        isGuarding = false;
    }

    private ActionData GetHealthPercent()
    {
        ActionData actionData = new ActionData();
        actionData.healthPercent = currentHealth / maxHealth;
        actionData.knockbackDir = -transform.forward;
        actionData.knockbackPower = 20f;
        
        return actionData;
    }
    
    public void TakeHeal()
    {
        
    }

    public void Dead()
    {
        BehaviorGraphAgent.SetVariableValue<BossState>("BossState", BossState.Dead);
        change.SendEventMessage(BossState.Dead);
    }
}