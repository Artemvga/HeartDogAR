using System.Collections.Generic;
using UnityEngine;

public class LungViewPanel : MonoBehaviour
{
    public RectTransform controls;
    public RectTransform panelToggle;
    readonly Dictionary<GameObject,bool> previous=new Dictionary<GameObject,bool>();
    bool restoreToggle;
    void OnEnable()
    {
        previous.Clear();
        if(controls!=null) foreach(Transform child in controls)
        {
            // Keep the full-screen gesture surface, so the user's model can still rotate.
            if(child.GetComponent<ModeController>()!=null) continue;
            previous[child.gameObject]=child.gameObject.activeSelf;
            child.gameObject.SetActive(false);
        }
        if(panelToggle!=null){restoreToggle=panelToggle.gameObject.activeSelf;panelToggle.gameObject.SetActive(false);}
    }
    void OnDisable()
    {
        foreach(var entry in previous)if(entry.Key!=null)entry.Key.SetActive(entry.Value);
        if(panelToggle!=null)panelToggle.gameObject.SetActive(restoreToggle);
    }
}
