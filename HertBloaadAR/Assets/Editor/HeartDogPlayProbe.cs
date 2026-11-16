using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class HeartDogPlayProbe
{
    static double deadline;
    static int phase;
    static ModeController mode;
    static GameObject target;
    static PointerEventData pointer;
    static HeartDogPlayProbe() { EditorApplication.update += Tick; }
    [MenuItem("Tools/HeartDog/Run play mode checks")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save scene changes before running the HeartDog play mode checks.");
        Directory.CreateDirectory("Library/HeartDogChecks");
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
        SessionState.SetBool("HeartDogProbe", true);
        EditorApplication.isPlaying = true;
    }
    static void Check(bool condition, string text) { if (!condition) throw new Exception(text); }
    static void Tick()
    {
        if (!SessionState.GetBool("HeartDogProbe", false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        try
        {
            if (phase == 0) { deadline = EditorApplication.timeSinceStartup + 1; phase = 1; }
            if (EditorApplication.timeSinceStartup < deadline) return;
            if (phase == 1)
            {
                var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Check(all.Any(t => t.name == "App safe area"), "Presentation not initialized in Play Mode");
                var menu = all.First(t => t.name == "MenuUI"); var choice = all.First(t => t.name == "ChoiseMode");
                all.First(t => t.name.Trim() == "StartButton").GetComponent<Button>().onClick.Invoke();
                Check(choice.gameObject.activeInHierarchy && !menu.gameObject.activeInHierarchy, "Start navigation failed");
                all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                all.First(t => t.name == "Назад" && t.parent == choice).GetComponent<Button>().onClick.Invoke();
                Check(menu.gameObject.activeInHierarchy && !choice.gameObject.activeInHierarchy, "Back navigation failed");
                SceneManager.LoadScene("ModelHeart"); phase = 2; deadline = EditorApplication.timeSinceStartup + 1;
            }
            else if (phase == 2)
            {
                mode = UnityEngine.Object.FindFirstObjectByType<ModeController>();
                Check(mode != null && mode.targetObject != null, "Default organ target missing");
                target = new GameObject("Hold test target"); mode.SetTargetObject(target.transform);
                pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
                ExecuteEvents.Execute(mode.rotateRightButton.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                phase = 3; deadline = EditorApplication.timeSinceStartup + .2;
            }
            else if (phase == 3)
            {
                Check(Quaternion.Angle(target.transform.rotation, Quaternion.identity) < .01f, "Hold runs every frame before delay");
                phase = 4; deadline = EditorApplication.timeSinceStartup + .4;
            }
            else if (phase == 4)
            {
                ExecuteEvents.Execute(mode.rotateRightButton.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                float angle = Quaternion.Angle(target.transform.rotation, Quaternion.identity);
                Check(angle > 0 && angle < 5, "Hold repeat cadence incorrect: " + angle);
                mode.SwitchControlMode();
                Check(mode.scaleUpButton.gameObject.activeSelf && !mode.rotateUpButton.gameObject.activeSelf, "Mode switching failed");
                var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                all.First(t=>t.name=="ButtonLungs").GetComponent<Button>().onClick.Invoke();
                var diagram=UnityEngine.Object.FindFirstObjectByType<LungModelBreathing>();
                Check(diagram != null && diagram.gameObject.activeInHierarchy,"Lung demonstration not visible");
                Check(!mode.rotateUpButton.gameObject.activeInHierarchy,"Arrow controls still visible in lung demonstration");
                Check(diagram.AirParticleCount==28,"Missing rig airflow particles");
                diagram.TogglePause(); Check(diagram.paused,"Lung pause failed");
                SceneManager.LoadScene("CycleHeart"); phase=5; deadline=EditorApplication.timeSinceStartup+1;
            }
            else if(phase==5)
            {
                var catalog=Resources.Load<FlowMeshCatalog>("UI/FlowMeshCatalog");
                int count=0;
                foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(catalog.arrowMeshes.Contains(filter.sharedMesh))
                    {count++;Check(filter.GetComponent<Renderer>().forceRenderingOff,"Cycle 3D arrow reappeared during playback");Check(filter.GetComponent<FlowParticleReplacement>()!=null,"Missing particle replacement");}
                Check(count>=9,"Expected cycle flow paths are missing");
                File.WriteAllText("Library/HeartDogChecks/PlayMode.txt", "PASS: navigation, controls, lung pause, airflow, and cycle playback with 9 legacy arrows replaced.");
                Finish();
            }
        }
        catch (Exception e) { File.WriteAllText("Library/HeartDogChecks/PlayMode.txt", e.ToString()); Finish(); }
    }
    static void Finish()
    {
        SessionState.SetBool("HeartDogProbe", false); phase = 0;
        EditorApplication.isPlaying = false;
        Debug.Log("HeartDog play mode checks finished. See Library/HeartDogChecks/PlayMode.txt.");
    }
}
