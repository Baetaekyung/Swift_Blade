﻿using System;
using System.Collections;
using System.Linq;
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
    
    public event Action<float> OnHitEvent;
    public event Action OnDeadEvent;

    [SerializeField] private Material _flashMat;
    [SerializeField] private SkinnedMeshRenderer[] _meshRenderers;
    private Material[] _originMats;
    
    private void Start()
    {
        guardCount = maxGuardCount;
        currentHealth = maxHealth;
        
        _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _originMats = new Material[_meshRenderers.Length];
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _originMats[i] = _meshRenderers[i].material;
        }


        OnHitEvent += FlashMat;
    }

    private void OnDestroy()
    {
        OnHitEvent -= FlashMat;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ActionData actionData = new ActionData();
            actionData.damageAmount = 10;
            
            TakeDamage(actionData);
        }
    }
    
    public void TakeDamage(ActionData actionData)
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
            HandleNonGuard(actionData.damageAmount);
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

    private void HandleNonGuard(float damage)
    {
        if (Random.value >= 0)
        {
            TriggerState(BossState.Hurt, damage);
        }
        else
        {
            isGuarding = true;
            TriggerState(BossState.Guard, damage / 2);
        }
        
        OnHitEvent.Invoke(GetHealthPercent());
    }

    private void TriggerState(BossState state, float damage)
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

    private float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
    
    public void TakeHeal()
    {
        
    }

    public void Dead()
    {
        BehaviorGraphAgent.SetVariableValue<BossState>("BossState", BossState.Dead);
        change.SendEventMessage(BossState.Dead);
    }

    private void FlashMat(float _trash)
    {
        StartCoroutine(FlashRoutine());
    }
    
    private IEnumerator FlashRoutine()
    {
        foreach (var t in _meshRenderers)
        {
            t.material = _flashMat;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material = _originMats[i];
        }
        
        
    }
}