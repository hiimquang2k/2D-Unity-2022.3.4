using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RainCollision : MonoBehaviour {
    [SerializeField] private ParticleSystem splashPS;
    [SerializeField] private LayerMask groundLayer;
    [Range(0,1)] public float splashChance = 0.3f;

    private ParticleSystem.Particle[] particles;
    private ParticleSystem rainPS;

    void Start() {
        rainPS = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[rainPS.main.maxParticles];
    }

    void LateUpdate() {
        int count = rainPS.GetParticles(particles);
        
        for (int i = 0; i < count; i++) {
            if (particles[i].position.y <= GetGroundHeight() + 0.1f) {
                if (Random.value < splashChance) {
                    splashPS.transform.position = particles[i].position;
                    splashPS.Emit(1);
                }
                particles[i].remainingLifetime = -1;
            }
        }
        rainPS.SetParticles(particles, count);
    }

    float GetGroundHeight() {
        // Adjust to your ground Y position
        return 0f; 
    }
}