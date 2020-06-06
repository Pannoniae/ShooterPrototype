using TMPro;
using UnityEngine;

namespace UI {
    public class AmmoDisplay : MonoBehaviour {
        public TMP_Text ammoText;
        public TMP_Text magText;

        public void updateAmmo(int ammo, int reserve) {
            ammoText.text = ammo.ToString();
            magText.text = reserve.ToString();
        }
    }
}
