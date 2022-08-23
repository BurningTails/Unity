using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject authorScreen;
    public GameObject settingsScreen;
    public Slider slider;
    public Text progressText;

    public void CreateSketchButton()
    {
        StartCoroutine(LoadAsynchronously("SketchEditorScene"));
    }

    public void OpenSketchButton()
    {

    }

    public void SettingsButton()
    {
        settingsScreen.SetActive(true);
    }

    public void SettingsBackButton()
    {
        settingsScreen.SetActive(false);
    }
    
    public void AuthorButton()
    {
        authorScreen.SetActive(true);
    }

    public void AuthorBackButton()
    {
        authorScreen.SetActive(false);
    }


    public void QuitButton()
    {
        Application.Quit();
    }






    IEnumerator LoadAsynchronously (string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            slider.value = progress;
            progressText.text = progress * 100f + "%";
            
            yield return null;
        }
    }
}
