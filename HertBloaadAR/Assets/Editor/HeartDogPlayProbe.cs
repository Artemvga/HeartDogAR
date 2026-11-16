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
    static PointerEventData pointer;
    static Vector3 originalScale, zoomedScale;
    static Quaternion originalRotation;
    static HeartbeatController heartbeat;
    static Animator heartAnimator;
    static float animationStart, slowProgress, pausedAt;
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
                var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                all.First(t => t.name == "ButtonHeart").GetComponent<Button>().onClick.Invoke();
                Check(mode.gameObject.activeInHierarchy, "Heart panel disabled the gesture receiver");
                Check(!mode.rotateUpButton.gameObject.activeInHierarchy && !mode.rotateRightButton.gameObject.activeInHierarchy, "Rotation arrows still visible");
                var results = new System.Collections.Generic.List<RaycastResult>();
                pointer = new PointerEventData(EventSystem.current) { position = new Vector2(Screen.width * .5f, Screen.height * .8f) };
                EventSystem.current.RaycastAll(pointer, results);
                Check(results.Count > 0 && results[0].gameObject == mode.gameObject, "Model gestures blocked by UI: " + (results.Count > 0 ? results[0].gameObject.name : "no hits"));
                originalScale = mode.targetObject.localScale;
                mode.OnScroll(new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, 3) });
                Check(mode.targetObject.localScale.x > originalScale.x * 1.4f, "Heart wheel zoom failed in rotation mode");
                zoomedScale = mode.targetObject.localScale;
                mode.rotationController.smoothRotation = false;
                originalRotation = mode.targetObject.localRotation;
                mode.OnPointerDown(pointer);
                pointer.position += new Vector2(150, 0); mode.OnDrag(pointer); mode.OnPointerUp(pointer);
                Check(Quaternion.Angle(originalRotation, mode.targetObject.localRotation) > 5, "Heart drag rotation failed");
                heartbeat = UnityEngine.Object.FindFirstObjectByType<HeartbeatController>();
                heartbeat.SetSpeedMultiplier(.5f);
                heartAnimator = mode.targetObject.GetComponent<Animator>();
                Check(heartAnimator != null && Mathf.Approximately(heartAnimator.speed, .5f), "Visible heart is not connected to speed slider");
                phase = 3; deadline = EditorApplication.timeSinceStartup + .2;
            }
            else if (phase == 3)
            {
                Check(Vector3.Distance(mode.targetObject.localScale, zoomedScale) < .001f, "Animation overwrote zoom");
                animationStart = heartAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                phase = 4; deadline = EditorApplication.timeSinceStartup + .5;
            }
            else if (phase == 4)
            {
                slowProgress = heartAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - animationStart;
                Check(slowProgress > .01f, "Heart animation is not playing");
                heartbeat.SetSpeedMultiplier(2f);
                animationStart = heartAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                phase = 6; deadline = EditorApplication.timeSinceStartup + .5;
            }
            else if (phase == 6)
            {
                float fastProgress = heartAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime - animationStart;
                Check(fastProgress > slowProgress * 2.5f, "Slider does not visibly accelerate animation: " + slowProgress + " / " + fastProgress);
                var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var outerHeart = all.First(t => t.name == "HeartPol").GetComponent<Animator>();
                Check(Mathf.Approximately(outerHeart.speed, 2f), "Second heart animation speed is out of sync");
                all.First(t => t.name == "LungUI").gameObject.SetActive(false);
                all.First(t => t.name == "HeartUI").gameObject.SetActive(false);
                all.First(t => t.name == "ButtonLungs").GetComponent<Button>().onClick.Invoke();
                var lungs = UnityEngine.Object.FindFirstObjectByType<LungModelBreathing>();
                Check(lungs != null && lungs.gameObject.activeInHierarchy, "Lung model missing");
                Check(lungs.GetComponentsInChildren<MeshRenderer>(true).Length == 0, "Floating lung particles still exist");
                Check(mode.gameObject.activeInHierarchy, "Lung panel disabled gestures");
                lungs.TogglePause(); Check(lungs.paused, "Lung pause failed");
                pausedAt = lungs.CyclePosition;
                phase = 7; deadline = EditorApplication.timeSinceStartup + .2;
            }
            else if (phase == 7)
            {
                var lungs = UnityEngine.Object.FindFirstObjectByType<LungModelBreathing>();
                Check(Mathf.Approximately(lungs.CyclePosition, pausedAt), "Paused lungs keep moving");
                lungs.TogglePause();
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
                File.WriteAllText("Library/HeartDogChecks/PlayMode.txt", "PASS: navigation; heart gestures and persistent zoom; actual animation accelerates with slider; both hearts synchronized; no rotation arrows; lungs have no particles and pause correctly; cycle arrows replaced.");
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
