using UnityEngine;
using Random = System.Random;

public class AudioManager : MonoBehaviour {
    
    [SerializeField]
    private float footstepCooldown = 0.5f;
    // when the last footstep sound was emitted, used for the cooldown
    private float lastFootstep;
    
    public AudioClip[] footsteps;

    private AudioSource audioSrc;
    
    // the actual instance lol
    private static AudioManager _instance;
    public static AudioManager instance {
        get {
            if (_instance != null) {
                return _instance;
            }

            _instance = GameManager.instance.player.GetComponent<AudioManager>(); // TODO store the audiosource normally, not with this janky hack
            return _instance;
        }
    }

    void Awake() {
        lastFootstep = Time.time;
        audioSrc = GetComponent<AudioSource>();
    }

    public void playFootstep() {
        if (Time.time - lastFootstep > footstepCooldown) {
            var random =
                    new Random().Next(footsteps.Length); // TODO check this shit works and doesn't throw index error
            audioSrc.PlayOneShot(footsteps[random]);
            lastFootstep = Time.time;
        }
    }
}