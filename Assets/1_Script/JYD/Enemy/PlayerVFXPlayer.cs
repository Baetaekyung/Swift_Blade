using Swift_Blade.Pool;
using UnityEngine;

namespace Swift_Blade
{
    public class PlayerVFXPlayer : MonoBehaviour,IEntityComponent
    {
        [SerializeField] private PoolPrefabMonoBehaviourSO dustEffect;
        [SerializeField] private PoolPrefabMonoBehaviourSO hitSlashEffect;
        [SerializeField] private PoolPrefabMonoBehaviourSO parryEffect;
        [SerializeField] private Transform parryParticleTrm;
        private PlayerStatCompo _statCompo;
        
        public void EntityComponentAwake(Entity entity)
        {
            MonoGenericPool<Dust>.Initialize(dustEffect);
            MonoGenericPool<HitSlash>.Initialize(hitSlashEffect);
            MonoGenericPool<ParryParticle>.Initialize(parryEffect);
        }
                
        public void PlayDamageEffect(ActionData actionData)
        {
            Dust dust = MonoGenericPool<Dust>.Pop();
            dust.transform.position = actionData.hitPoint;
            dust.transform.rotation = Quaternion.LookRotation(-actionData.hitNormal);
            
            HitSlash hitSlash = MonoGenericPool<HitSlash>.Pop();
            hitSlash.transform.position = actionData.hitPoint;
            hitSlash.transform.rotation = Quaternion.LookRotation(-actionData.hitNormal);
        }
        
        public void PlayParryEffect()
        {
            ParryParticle parryParticle = MonoGenericPool<ParryParticle>.Pop();
            parryParticle.transform.position = parryParticleTrm.position;
            
            HitSlash hitSlash = MonoGenericPool<HitSlash>.Pop();
            hitSlash.transform.position = parryParticleTrm.position;
        }
                
    }
}
