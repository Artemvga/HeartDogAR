using UnityEngine;

/// <summary>Breathes using the imported model's bone animation, without floating overlays.</summary>
public class LungModelBreathing : MonoBehaviour
{
    public bool paused;
    public float cycleSeconds = 6f;
    public float CyclePosition => Mathf.Repeat(clock / Mathf.Max(.1f, cycleSeconds), 1f);
    float clock;
    Animator animator;
    AnimationClip clip;
    bool initialized;
    bool originalAnimatorEnabled;

    public void Initialize()
    {
        if (initialized) return;
        animator = GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var candidate in animator.runtimeAnimatorController.animationClips)
            if (candidate.name == "LungsAnimation") { clip = candidate; break; }
        if (clip == null) return;
        originalAnimatorEnabled = animator.enabled;
        initialized = true;
        animator.enabled = false;
    }

    void OnEnable()
    {
        Initialize(); clock = 0; paused = false;
        if (initialized) { animator.enabled = false; Sample(0); }
    }
    void OnDisable() { if (initialized) animator.enabled = originalAnimatorEnabled; }
    public void TogglePause() => paused = !paused;
    void LateUpdate()
    {
        if (!paused) clock += Time.deltaTime;
        Sample(CyclePosition);
    }
    public void Sample(float normalizedTime)
    {
        Initialize();
        if (clip == null) return;
        // The imported take contains a complete breath with matching end poses.
        // Sample the whole cycle even though its import settings disable looping.
        clip.SampleAnimation(gameObject, Mathf.Repeat(normalizedTime, 1f) * clip.length);
    }
}
