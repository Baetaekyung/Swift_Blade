using Swift_Blade.UI;
using System.Collections;
using Swift_Blade.Combat.Health;
using UnityEngine;

namespace Swift_Blade
{
    public class BrokingToRock : MinigameItems
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerHealth>(out PlayerHealth ph))
            {
                PlayerMinigameStatus.Instance.GetCanBrokingItem();
                Destroy(gameObject);
            }  
        }
    }
}
