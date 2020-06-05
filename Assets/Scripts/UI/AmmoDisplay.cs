using TMPro;
using UnityEngine;

namespace UI {
    public class AmmoDisplay : MonoBehaviour {
        
        [SerializeField]
        private TMP_Text ammoText;
        [SerializeField]
        private TMP_Text magText;
        public void updateAmmo(int ammo, int reserve) {
            ammoText.text = ammo.ToString();
            magText.text = reserve.ToString();
            
        }
    }
}