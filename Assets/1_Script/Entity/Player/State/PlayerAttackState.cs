using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Swift_Blade.FSM.States
{
    public class PlayerAttackState : BasePlayerState
    {
        //protected override bool BaseAllowParryInput => false;

        private readonly PlayerRenderer playerRenderer;
        private ComboData currentComboData;

        private bool allowListening;
        private bool isCurrentAnimationEndable;
        private bool inputBuffer;
        private bool forceExit;

        private float delayContinuousCombo;
        private bool IsContinuousComboAllowed => delayContinuousCombo > Time.time;
        public EComboState PreviousComboState { get; set; }
        public EComboState NonImmediateComboState { get; set; }
        private readonly List<EComboState> comboStateHistory = new(5);
        public void ClearComboHistory() => comboStateHistory.Clear();
        public PlayerAttackState(FiniteStateMachine<PlayerStateEnum> stateMachine, Animator animator, Player entity, AnimationTriggers animTrigger, AnimationParameterSO animParamSO = null)
            : base(stateMachine, animator, entity, animTrigger, animParamSO)
        {
            playerRenderer = player.GetPlayerRenderer;
            //Player.Debug_Updt += () =>
            //{
            //    //UI_DebugPlayer.DebugText(2, IsContinuousComboAllowed, "dpover", DBG_UI_KEYS.Keys_PlayerAction);
            //    //UI_DebugPlayer.DebugText(3, delayContinuousCombo, "deadPeriod", DBG_UI_KEYS.Keys_PlayerAction);
            //    //UI_DebugPlayer.DebugText(4, Time.time, "time", DBG_UI_KEYS.Keys_PlayerAction);
            //    //UI_DebugPlayer.DebugText(5, comboStateHistory.Count, "cshCount", DBG_UI_KEYS.Keys_PlayerAction);
            //};
        }

        public override void Enter()
        {
            base.Enter();
            forceExit = false;
            if (!IsContinuousComboAllowed)
                comboStateHistory.Clear();
            
            if (NonImmediateComboState != EComboState.None)
            {
                comboStateHistory.Add(NonImmediateComboState);
                NonImmediateComboState = EComboState.None;
            }
            
            comboStateHistory.Add(PreviousComboState);
            bool matchFound = GetMatchingComboSO(out currentComboData);
            if (matchFound)
            {
                bool mouseMove = true;
                Vector3 direction = mouseMove ?
                    player.GetPlayerInput.GetMousePositionWorld - playerMovement.transform.position :
                    player.GetPlayerInput.GetInputDirectionRawRotated;
                playerRenderer.LookAtDirection(direction);
                playerMovement.UseMouseLock = true;

                ComboAttack();
            }
            else
            {
                forceExit = true;
                OnComboFail();
            }
        }
        public override void Update()
        {
            base.Update();
            if (inputBuffer && isCurrentAnimationEndable && IsContinuousComboAllowed)
            {
                comboStateHistory.Add(PreviousComboState);
                if (GetMatchingComboSO(out currentComboData))
                    ComboAttack();
                else
                    OnComboFail();
            }
        }
        private void OnComboFail()
        {
            comboStateHistory.Clear();
            delayContinuousCombo = 0;
            comboStateHistory.Add(PreviousComboState);
            if (GetMatchingComboSO(out currentComboData) && false)
                ComboAttack();
            else
            {
                if (forceExit == true)
                {
                    GetOwnerFsm.ChangeState(PlayerStateEnum.Move);
                }
                forceExit = true;
                Debug.Log("no match, no combo, no attack");
            }
        }
        private bool GetMatchingComboSO(out ComboData comboData)
        {
            comboData = null;
            foreach (AttackComboSO comboSO in player.GetComboList)
            {
                if (comboSO.IsMatch(comboStateHistory, out comboData))
                    return true;
            }
            return false;
        }
        private void ComboAttack()
        {
            inputBuffer = false;
            allowListening = false;
            isCurrentAnimationEndable = false;

            delayContinuousCombo = currentComboData.GetAnimParam.GetPeriod + Time.time;
            AnimationParameterSO param = currentComboData.GetAnimParam;
            PlayAnimationRebind(param);
        }
        protected override void OnAttackInput(EComboState currentState, EComboState nonImeediateState = EComboState.None)
        {
            if (allowListening && inputBuffer != true)
            {
                PreviousComboState = currentState;
                inputBuffer = true;
            }
        }
        protected override void OnAnimationEndTriggerListen() => allowListening = true;
        protected override void OnAnimationEndTriggerStoplisten() => allowListening = false;
        protected override void OnAnimationEndableTrigger()
        {
            if(forceExit)
            {
                GetOwnerFsm.ChangeState(PlayerStateEnum.Move);
            }
            isCurrentAnimationEndable = true;
        }
        protected override void OnForceEventTrigger(float force)
        {
            Vector3 result = currentComboData.GetComboForce * force;
            playerMovement.AddForceFacingDirection(result);
        }
        protected override void OnForceEventTrigger2(float force)
        {
            Vector3 result = currentComboData.GetComboForce * force;
            playerMovement.AddForceCurve(result);
        }
        public override void Exit()
        {
            playerMovement.SpeedMultiplierDefault = 1;
            anim_inputLocalLerp = Vector3.zero;
            //playerMovement.UseMouseLock = false;
            base.Exit();
        }
    }
}
