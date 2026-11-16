using UnityEngine;

/// <summary>Replaces an imported arrow mesh while preserving its existing animation and colour.</summary>
public class FlowParticleReplacement : MonoBehaviour
{
    Renderer original;
    Renderer particle;
    Transform dot;
    Material material;
    MaterialPropertyBlock properties;
    Vector3 meshSize;
    public void Initialize(Mesh mesh)
    {
        original=GetComponent<Renderer>();
        if(original==null || dot!=null)return;
        original.forceRenderingOff=true;
        meshSize=mesh.bounds.size;
        var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Поток вместо стрелки";
        go.transform.SetParent(transform,false);go.transform.localPosition=mesh.bounds.center;
        var collider=go.GetComponent<Collider>();if(collider!=null){if(Application.isPlaying)Destroy(collider);else DestroyImmediate(collider);}
        dot=go.transform;particle=go.GetComponent<Renderer>();
        material=new Material(Resources.Load<Shader>("UI/LungAirflow"));particle.sharedMaterial=material;
        particle.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;properties=new MaterialPropertyBlock();
        Synchronize();
    }
    void LateUpdate()=>Synchronize();
    void Synchronize()
    {
        if(original==null || dot==null)return;
        original.forceRenderingOff=true;
        particle.enabled=original.enabled;
        var scale=transform.lossyScale;
        float diameter=Mathf.Min(Mathf.Abs(meshSize.x*scale.x),Mathf.Abs(meshSize.y*scale.y),Mathf.Abs(meshSize.z*scale.z))*.8f;
        dot.localScale=new Vector3(diameter/Mathf.Max(Mathf.Abs(scale.x),1e-8f),diameter/Mathf.Max(Mathf.Abs(scale.y),1e-8f),diameter/Mathf.Max(Mathf.Abs(scale.z),1e-8f));
        Color tint=AppPresentation.Accent;
        if(original.sharedMaterial!=null && original.sharedMaterial.HasProperty("_Color"))tint=original.sharedMaterial.color;
        tint.a=.9f;properties.SetColor("_Color",tint);particle.SetPropertyBlock(properties);
    }
    void OnDestroy(){if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}}
}
