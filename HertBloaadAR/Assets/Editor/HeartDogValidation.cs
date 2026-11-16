using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HeartDogValidation
{
    const string Output = "Library/HeartDogChecks";
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);

    [MenuItem("Tools/HeartDog/Validate and capture UI")]
    public static void Validate()
    {
        Directory.CreateDirectory(Output);
        try{ValidateCore();File.WriteAllText(Output+"/LayoutValidation.txt","PASS: 3 scenes, 6 screen formats, notch simulation, bounds checks, no visible legacy arrows.");}
        catch(Exception error){File.WriteAllText(Output+"/LayoutValidation.txt",error.ToString());throw;}
    }
    static void ValidateCore()
    {
        Directory.CreateDirectory(Output);
        foreach (string sceneName in new[] { "Menu", "ModelHeart", "CycleHeart" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Additive);
            try
            {
                var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
                if (sceneName == "ModelHeart")
                {
                    var heartbeat = all.Select(t => t.GetComponent<HeartbeatController>()).First(c => c != null);
                    var serialized = new SerializedObject(heartbeat);
                    var animator = serialized.FindProperty("dogHeartAnimator").objectReferenceValue as Animator;
                    Require(animator != null && animator.runtimeAnimatorController != null, "Missing heart animator");
                    Call(heartbeat, "Start");
                    heartbeat.SetSpeedMultiplier(1.5f);
                    Require(Mathf.Approximately(animator.speed, 1.5f), "Heartbeat slider does not control animation");
                    var mode = all.Select(t => t.GetComponent<ModeController>()).First(c => c != null);
                    var target = new GameObject("Validation target");
                    try
                    {
                        target.transform.localScale = new Vector3(2, 3, 4);
                        mode.SetTargetObject(target.transform);
                        target.transform.localScale *= 1.5f;
                        mode.SetTargetObject(target.transform);
                        Require(mode.scaleController.GetInitialScale() == new Vector3(2, 3, 4), "Scale baseline drift");
                        Call(mode, "ApplyScaleChange", .1f);
                        Require(Mathf.Approximately(target.transform.localScale.y / target.transform.localScale.x, 1.5f), "Non-uniform scale distorted");
                        var rotation = mode.rotationController; rotation.smoothRotation = false;
                        target.transform.localRotation = Quaternion.identity;
                        rotation.StartRotation(Vector2.zero); rotation.ProcessRotation(new Vector2(10, 0));
                        float first = Quaternion.Angle(Quaternion.identity, target.transform.localRotation);
                        target.transform.localRotation = Quaternion.identity;
                        rotation.StartRotation(Vector2.zero); rotation.ProcessRotation(new Vector2(5, 0)); rotation.ProcessRotation(new Vector2(10, 0));
                        Require(Mathf.Abs(first - Quaternion.Angle(Quaternion.identity, target.transform.localRotation)) < .01f, "Rotation depends on event count");
                        rotation.smoothRotation = true;
                        rotation.Initialize();
                        target.transform.localRotation = Quaternion.Euler(0, 60, 0);
                        Call(rotation, "Update");
                        Require(Quaternion.Angle(target.transform.localRotation, Quaternion.Euler(0,60,0)) < .01f, "Idle rotation overwrites external pose");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(target); }
                }
                AppPresentation.Apply(scene);
                var catalog=Resources.Load<FlowMeshCatalog>("UI/FlowMeshCatalog");
                Require(catalog!=null && catalog.arrowMeshes.Length>0,"Missing arrow mesh catalog");
                int arrowCount=0;
                foreach(var root in scene.GetRootGameObjects()) foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                    if(catalog.arrowMeshes.Contains(filter.sharedMesh))
                    { arrowCount++; Require(filter.GetComponent<Renderer>().forceRenderingOff,"Legacy 3D arrow is still visible"); }
                if(sceneName=="CycleHeart")Require(arrowCount>=9,"Cycle arrow replacements missing");
                Capture(scene, sceneName, 1080, 1920);
                Capture(scene, sceneName+"-compact", 750, 1334);
                Capture(scene, sceneName+"-tablet", 2048, 2732);
                Capture(scene, sceneName+"-landscape", 1920, 1080);
                Capture(scene, sceneName+"-tall", 1080, 2400);
                foreach(var root in scene.GetRootGameObjects())foreach(var safe in root.GetComponentsInChildren<AppSafeArea>(true))
                {var rect=(RectTransform)safe.transform;rect.anchorMin=new Vector2(.055f,.035f);rect.anchorMax=new Vector2(.945f,.955f);}
                Capture(scene, sceneName+"-notch", 1080, 2340);
                foreach(var root in scene.GetRootGameObjects())foreach(var safe in root.GetComponentsInChildren<AppSafeArea>(true))
                {var rect=(RectTransform)safe.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;}
                if(sceneName == "ModelHeart")
                {
                    var heart=all.First(t=>t.name=="HeartUI");heart.gameObject.SetActive(true);
                    all.First(t=>t.name=="ModeControllerUI").gameObject.SetActive(false);
                    all.First(t=>t.name=="ControllerUI").gameObject.SetActive(false);
                    all.First(t=>t.name=="ButtonsUI").gameObject.SetActive(false);
                    Capture(scene,"HeartPanel",1080,1920);Capture(scene,"HeartPanel-landscape",1920,1080);
                    heart.gameObject.SetActive(false);
                    var lungs = all.First(t=>t.name=="LungUI"); lungs.gameObject.SetActive(true);
                    all.First(t=>t.name=="ModeControllerUI").gameObject.SetActive(false);
                    all.First(t=>t.name=="ControllerUI").gameObject.SetActive(false);
                    all.First(t=>t.name=="ButtonsUI").gameObject.SetActive(false);
                    Capture(scene,"Lungs",1080,1920);Capture(scene,"Lungs-landscape",1920,1080);
                    var diagram=all.First(t=>t.name=="Lungs").GetComponent<LungModelBreathing>();
                    diagram.TogglePause();
                    Require(diagram.paused,"Lung pause failed");
                }
                if (sceneName == "Menu")
                {
                    var menu = all.First(t => t.name == "MenuUI");
                    var choice = all.First(t => t.name == "ChoiseMode");
                    menu.gameObject.SetActive(false); choice.gameObject.SetActive(true);
                    Capture(scene, "Modes", 1080, 1920);
                    menu.gameObject.SetActive(true); choice.gameObject.SetActive(false);
                    Capture(scene, "Menu-landscape", 1920, 1080);
                    menu.gameObject.SetActive(false);
                    var information = all.First(t => t.name == "Information"); information.gameObject.SetActive(true);
                    Capture(scene, "Information", 1080, 1920);
                    foreach (string page in new[] { "Information1", "Information2", "Developers" })
                    {
                        var nav = all.First(t => t.name == "Buttons" && t.parent == information); nav.gameObject.SetActive(false);
                        var child = all.First(t => t.name == page); child.gameObject.SetActive(true);
                        Capture(scene, page, 1080, 1920); child.gameObject.SetActive(false);
                    }
                }
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        Debug.Log("HeartDog verification passed (previews: Library/HeartDogChecks): animator, scale baseline, aspect ratio, pointer deltas, idle rotation, three scene layouts.");
    }

    static void Capture(Scene scene, string name, int width, int height)
    {
        var cameraObject = new GameObject("UI capture camera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = AppPresentation.Background; camera.orthographic = true; camera.orthographicSize = 5;
        camera.transform.position = new Vector3(0, 0, -10000); camera.cullingMask = 1 << 5;
        var rt = new RenderTexture(width, height, 24); camera.targetTexture = rt;
        try
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (!canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) continue;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                    var scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null) { scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = width>height ? Mathf.Min(width / 1920f, height / 1080f) : Mathf.Min(width / 1080f, height / 1920f); }
                }
            Canvas.ForceUpdateCanvases();
            foreach(var root in scene.GetRootGameObjects())
                foreach(var layout in root.GetComponentsInChildren<AppResponsiveLayout>(true))layout.Reflow(((RectTransform)layout.transform).rect.size);
            Canvas.ForceUpdateCanvases();
            string[] primary={"StartButton","InformationButton","ExitButton","SettingsButton","ButtonHeart","ButtonDog","ButtonVessels","ButtonLungs"};
            foreach(var root in scene.GetRootGameObjects())foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                if(!button.gameObject.activeInHierarchy || !primary.Contains(button.name.Trim()))continue;
                var canvas=button.GetComponentInParent<Canvas>();if(canvas==null || canvas.renderMode==RenderMode.WorldSpace)continue;
                var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
                foreach(var corner in corners){var point=camera.WorldToScreenPoint(corner);Require(point.x>=-1 && point.y>=-1 && point.x<=width+1 && point.y<=height+1,name+": button outside viewport: "+button.name);}
            }
            camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = rt;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture); RenderTexture.active = previous;
        }
        finally { camera.targetTexture = null; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(cameraObject); }
    }
}
