using Swift_Blade.Feeling;

namespace Swift_Blade.Combat.Feedback
{
    public class HitStopFeedback : Feedback
    {
        public HitStopSO hitStopData;
        
        
        public override void PlayFeedback()
        {
            
            HitStopManager.Instance.DoHitStop(hitStopData);
        }

        public override void ResetFeedback()
        {
            
        }
                
        
    }
}
