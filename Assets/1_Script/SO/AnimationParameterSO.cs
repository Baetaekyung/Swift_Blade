using UnityEngine;

namespace Swift_Blade
{
    [CreateAssetMenu(fileName = "AnimationParameterSO", menuName = "Scriptable Objects/AnimationParameterSO", order = int.MinValue)]
    public class AnimationParameterSO : ScriptableObject
    {
        [SerializeField] private string paramName;
        [SerializeField] private float noramlizedTime;
        public int GetAnimationHash { get; private set; }
        public float GetNormalizedTime => noramlizedTime;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(paramName) == false)
            {
                GetAnimationHash = Animator.StringToHash(paramName);
            }
        }
    }
}