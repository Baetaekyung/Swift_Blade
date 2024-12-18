using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Swift_Blade.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private Image _healthFillAmount;
        
        /// <param name="normalizedHealth"> input currentHealth / maxHealth</param>
        public void SetHealthFillAmount(int normalizedHealth)
        {
            _healthFillAmount.fillAmount = normalizedHealth;
        }
    }
}
