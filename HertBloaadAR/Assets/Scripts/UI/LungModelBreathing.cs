using System.Collections.Generic;
using UnityEngine;

/// <summary>Plays the imported lung rig and draws airflow along its actual bone branches.</summary>
public class LungModelBreathing : MonoBehaviour
{
    public bool paused;
    public float cycleSeconds = 6f;
    public float CyclePosition => Mathf.Repeat(clock / cycleSeconds, 1f);
    public int AirParticleCount => particles.Count;
    float clock;
    Animator animator;
    AnimationClip clip;
    bool originalAnimatorEnabled;
    Transform inlet;
    readonly List<Transform> branches = new List<Transform>();
    readonly List<Transform> particles = new List<Transform>();
    readonly List<Renderer> particleRenderers = new List<Renderer>();
    Material airMaterial;
    MaterialPropertyBlock properties;

    public void Initialize()
    {
        if (animator != null) return;
        animator = GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null) return;
        originalAnimatorEnabled = animator.enabled;
        if(gameObject.activeInHierarchy) { animator.Rebind(); animator.Update(0); }
        foreach (var candidate in animator.runtimeAnimatorController.animationClips)
            if (candidate.name == "LungsAnimation") { clip = candidate; break; }
        foreach (var bone in GetComponentsInChildren<Transform>(true))
        {
            if (bone.name == "Bone") inlet = bone;
            if (bone.name == "Bone.001_end" || bone.name == "Bone.002_end" || bone.name == "Bone.003_end" || bone.name == "Bone.004_end") branches.Add(bone);
        }
        airMaterial = new Material(Resources.Load<Shader>("UI/LungAirflow"));
        airMaterial.name = "Lung airflow";
        properties = new MaterialPropertyBlock();
        for(int i=0;i<branches.Count*7;i++)
        {
            var dot=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name="Частица воздуха"; dot.transform.SetParent(transform,false);
            var collider=dot.GetComponent<Collider>(); if(collider!=null) Destroy(collider);
            var renderer=dot.GetComponent<Renderer>(); renderer.sharedMaterial=airMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows=false;
            particles.Add(dot.transform); particleRenderers.Add(renderer);
        }
    }
    void OnEnable() { Initialize(); clock=0; paused=false; if(animator!=null) { animator.enabled=true; animator.Rebind(); animator.Update(0); animator.enabled=false; } }
    void OnDisable() { if(animator!=null) animator.enabled=originalAnimatorEnabled; }
    public void TogglePause() => paused=!paused;
    void LateUpdate()
    {
        if(!paused) clock+=Time.deltaTime;
        Sample(CyclePosition);
    }
    public void Sample(float normalizedTime)
    {
        Initialize();
        if(clip==null || inlet==null || branches.Count==0) return;
        // Only imported bone curves are sampled; the AR target transform stays untouched.
        clip.SampleAnimation(gameObject, Mathf.Repeat(normalizedTime,1f)*clip.length);
        bool inward=normalizedTime<.5f;
        Color tint=inward?new Color32(99,222,198,255):new Color32(255,179,115,255);
        for(int branch=0;branch<branches.Count;branch++)
        {
            Transform end=branches[branch];
            Vector3 start=transform.InverseTransformPoint(inlet.position);
            Vector3 fork=transform.InverseTransformPoint(end.parent.position);
            Vector3 finish=transform.InverseTransformPoint(end.position);
            float radius=Mathf.Max(Vector3.Distance(start,finish)*.025f,.00001f);
            for(int dot=0;dot<7;dot++)
            {
                int index=branch*7+dot;
                float t=Mathf.Repeat(normalizedTime*3f+dot/7f,1f); if(!inward)t=1-t;
                particles[index].localPosition=t<.65f?Vector3.Lerp(start,fork,t/.65f):Vector3.Lerp(fork,finish,(t-.65f)/.35f);
                particles[index].localScale=Vector3.one*radius;
                properties.SetColor("_Color",tint); particleRenderers[index].SetPropertyBlock(properties);
            }
        }
    }
    void OnDestroy(){ if(airMaterial!=null) { if(Application.isPlaying) Destroy(airMaterial); else DestroyImmediate(airMaterial); } }
}
