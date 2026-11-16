using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
public static class LungModelPreview {
 [MenuItem("Tools/HeartDog/Capture 3D lungs")]
 public static void Capture(){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/ModelHeart.unity",OpenSceneMode.Additive);
 GameObject clone=null,camGO=null;
 try{
 var source=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="Lungs");
 clone=Object.Instantiate(source.gameObject);clone.transform.position=Vector3.zero;clone.SetActive(true);
 foreach(var t in clone.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
 var breath=clone.AddComponent<LungModelBreathing>();breath.Initialize();
 camGO=new GameObject("Lung preview camera");var cam=camGO.AddComponent<Camera>();cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=AppPresentation.Background;cam.orthographic=true;
 Directory.CreateDirectory("Library/HeartDogChecks");
 foreach(var r in clone.GetComponentsInChildren<Renderer>())r.enabled=true;
 for(int frame=0;frame<2;frame++){
 breath.Sample(frame==0?.2f:.7f);
 Bounds bounds=new Bounds(clone.transform.position,Vector3.zero);bool first=true;
 foreach(var r in clone.GetComponentsInChildren<SkinnedMeshRenderer>()){var mesh=new Mesh();r.enabled=true;r.BakeMesh(mesh);var b=mesh.bounds;var wb=r.bounds;if(first){bounds=wb;first=false;}else bounds.Encapsulate(wb);Object.DestroyImmediate(mesh);}
 foreach(var t in clone.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;

 cam.transform.position=bounds.center+clone.transform.forward*bounds.size.magnitude*3;cam.transform.LookAt(bounds.center,clone.transform.up);cam.orthographicSize=bounds.size.magnitude*.65f;cam.nearClipPlane=.00001f;cam.farClipPlane=100;
 var rt=new RenderTexture(1000,1000,24);cam.targetTexture=rt;cam.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1000,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1000,1000),0,0);tex.Apply();File.WriteAllBytes("Library/HeartDogChecks/Lungs3D-"+frame+".png",tex.EncodeToPNG());RenderTexture.active=prev;cam.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
 }
 }finally{if(clone!=null)Object.DestroyImmediate(clone);if(camGO!=null)Object.DestroyImmediate(camGO);EditorSceneManager.CloseScene(scene,true);}
 }
}
