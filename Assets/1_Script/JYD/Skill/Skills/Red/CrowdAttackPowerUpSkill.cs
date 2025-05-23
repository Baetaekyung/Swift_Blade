using System.Collections.Generic;
using Swift_Blade.Pool;
using System.Linq;
using Swift_Blade.Combat.Health;
using UnityEngine;

namespace Swift_Blade.Skill
{
    [CreateAssetMenu(fileName = "CrowdAttackPowerUp", menuName = "SO/Skill/Red/CrowdAttackPowerUp")]
    public class CrowdAttackPowerUpSkill : SkillData
    {
        [Range(0.1f, 10)] [SerializeField] private float radius;
        [SerializeField] private LayerMask whatIsTarget;
        [Range(1, 10)] [SerializeField] private int targetCount;
        [Range(1f, 10f)] [SerializeField] private float increaseValue;
        
        private bool isUpgrade;
        
        public override void Initialize()
        {
            MonoGenericPool<RedWaveParticle>.Initialize(skillParticle);
        }
        
        public override void SkillUpdate(Player player,  IEnumerable<Transform> targets = null)
        {
            targets = Physics.OverlapSphere(player.GetPlayerTransform.position, radius, whatIsTarget)
                .Select(x => x.transform).ToList();
            
            if (isUpgrade == false && targets.Count() >= targetCount)
            {
                isUpgrade = true;
                GenerateSkillText(true);
                statCompo.AddModifier(statType , skillName , increaseValue * GetColorRatio());
                
                RedWaveParticle redWaveParticle = MonoGenericPool<RedWaveParticle>.Pop();
                redWaveParticle.transform.position = player.GetPlayerTransform.position + new Vector3(0,1,0);
            }
            else if(isUpgrade && targets.Count() < targetCount)
            {
                ResetSkill();
            }
            
        }
        
        public override void UseSkill(Player player,  IEnumerable<Transform> targets = null)
        {
            
        }

        public override void ResetSkill()
        {
            GenerateSkillText(false);
            isUpgrade = false;
            statCompo.RemoveModifier(statType , skillName);
        }
    }
}
