using System.Linq;
using Swift_Blade.Combat.Health;
using Swift_Blade.Pool;
using UnityEngine;

namespace Swift_Blade.Skill
{
    [CreateAssetMenu(fileName = "ParryExplosionSkill", menuName = "SO/Skill/Parry/Explosion")]
    public class ParryExplosionSkill : SkillData
    {
        public int skillDamage;
        public Vector2 explosionAdjustment;
        public float skillRadius;
        public LayerMask whatIsTarget;
        
        [Range(0,100)]public float random;
        
        public override void Initialize()
        {
            MonoGenericPool<SmallExplosion>.Initialize(skillParticle);
        }
        
        public override void UseSkill(Player player, Transform[] targets = null)
        {
            Vector3 explosionPosition = player.GetPlayerTransform.position +
                                        (player.GetPlayerTransform.forward * explosionAdjustment.x);
            explosionPosition.y = explosionPosition.y + player.GetPlayerTransform.position.y;
            
            if (targets == null || targets.Length == 0)
            {
                targets = Physics.OverlapSphere(explosionPosition, skillRadius, whatIsTarget).Select(x => x.transform).ToArray();
            }
            
            foreach (var item in targets)
            {
                float randomValue = Random.Range(0, 100);
                if (randomValue < random && item.TryGetComponent(out BaseEnemyHealth health))
                {
                    ActionData actionData = new ActionData();
                    actionData.damageAmount = skillDamage;
                    health.TakeDamage(actionData);

                    SmallExplosion smallExplosion = MonoGenericPool<SmallExplosion>.Pop();
                    smallExplosion.transform.position =
                        explosionPosition;
                }                
            }
            
        }
        
    }
}
