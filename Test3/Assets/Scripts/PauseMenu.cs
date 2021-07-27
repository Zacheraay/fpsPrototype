using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseGamePanel;
    public GameObject controlsPanel;

    public Slider sensitivitySlider;
    public Text mouseSensitivityValue;

    [SerializeField]
    Behaviour[] componentsToDisable;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(pauseGamePanel.activeSelf)
            {
                Resume();
            }
            else
            {
                ActivatePanel();
                controlsPanel.SetActive(false);
            }
        }

        mouseSensitivityValue.text = sensitivitySlider.value.ToString();
    }

    private void ActivatePanel()
    {
        pauseGamePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for(int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    public void Resume()
    {
        pauseGamePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = true;
        }
    }

    public void Controls()
    {
        pauseGamePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
