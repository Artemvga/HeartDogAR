using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController _instance;
    public GameObject elementUI;
    public GameObject sliderLoad;
    public Slider slider;
    private bool isLoading;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SceneLoad(int index)
    {
        if (isLoading || index < 0 || index >= SceneManager.sceneCountInBuildSettings) return;
        isLoading = true;
        if (elementUI != null) elementUI.SetActive(false);
        if (sliderLoad != null) sliderLoad.SetActive(true);
        StartCoroutine(LoadAsynk(index));
    }

    private IEnumerator LoadAsynk(int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        while (!operation.isDone)
        {
            float progres = Mathf.Clamp01(operation.progress / 0.9f);
            if (slider != null) slider.value = progres;
            yield return null;
        }
    }
}