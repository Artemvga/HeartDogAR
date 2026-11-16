using UnityEditor;
using UnityEngine;
using System.Linq;
[InitializeOnLoad]
public static class FlowCatalogSetup
{
    static FlowCatalogSetup(){EditorApplication.delayCall+=Ensure;}
    [MenuItem("Tools/HeartDog/Refresh flow mesh catalog")]
    public static void Ensure()
    {
        const string path="Assets/Resources/UI/FlowMeshCatalog.asset";
        if(AssetDatabase.LoadAssetAtPath<FlowMeshCatalog>(path)!=null)return;
        var meshes=AssetDatabase.LoadAllAssetsAtPath("Assets/Models/STRELA.fbx").OfType<Mesh>().ToArray();
        if(meshes.Length==0)return;
        var catalog=ScriptableObject.CreateInstance<FlowMeshCatalog>();catalog.arrowMeshes=meshes;
        AssetDatabase.CreateAsset(catalog,path);AssetDatabase.SaveAssets();
    }
}
