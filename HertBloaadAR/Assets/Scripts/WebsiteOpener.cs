using UnityEngine;

public class WebsiteOpener : MonoBehaviour
{
    public void OpenWebsite(string url)
    {
        Application.OpenURL(url);
    }
}