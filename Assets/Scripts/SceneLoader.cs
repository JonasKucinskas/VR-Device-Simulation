using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject playCanvas;
    [SerializeField] private GameObject levelCanvas;
    [SerializeField] private Transform pauseMenu;
    [SerializeField] private XROrigin origin;
    [SerializeField] private Transform pausePosition;
    private Vector3 oldPosition;
    private bool gamePaused = false;
    private bool waitForInput = false;
    private bool isLevel = false;
    private void Awake()
    {
        oldPosition = new Vector3(0,0,0);
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            bool triggerValue;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick,
                              out triggerValue);
            if (triggerValue && !waitForInput)
            {
                //Debug.Log(pauseMenu.gameObject.name);
                waitForInput = true;
                Invoke("PauseGame", 0.5f);
            }
        }
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void LoadLevelScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        isLevel = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode arg1)
    {
        SceneManager.SetActiveScene(scene);
        Debug.Log(SceneManager.GetActiveScene().name);
        if (isLevel)
            LevelManager.Load();
        isLevel = false;
    }

    public void LoadLevelCanvas()
    {
        playCanvas.SetActive(false);
        var info = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] files = info.GetFiles();
        int i = 0;
        levelCanvas.SetActive(true);
        foreach (Button bt in levelCanvas.GetComponentsInChildren<Button>())
        {
            if (bt.GetComponentInChildren<TextMeshProUGUI>().text != "Back")
                bt.interactable = false;
        }
        foreach (Button bt in levelCanvas.GetComponentsInChildren<Button>())
        {
            string filename = "";
            if (i < files.Length)
                filename = Path.GetFileNameWithoutExtension(files[i].Name);
            else
                break;
            bt.GetComponentInChildren<TextMeshProUGUI>().text = filename;
            bt.interactable = true;
            i++;
        }
    }
    public void QuitGame() // only works when game is running from build
    {
        Application.Quit();
    }
    public void BackToMain()
    {
        levelCanvas.SetActive(false);
        playCanvas.SetActive(true);
    }
    public void PauseGame()
    {
        if (!gamePaused)
        {
            pauseMenu.gameObject.SetActive(true);
            //pauseMenu.transform.SetPositionAndRotation(pauseMenuPosition.position, pauseMenuPosition.rotation);
            //Time.timeScale = 0f;
            //Debug.Log("old pos: " + oldPosition);
            oldPosition = origin.transform.position;
            origin.transform.SetPositionAndRotation(pausePosition.position, pausePosition.rotation);
            gamePaused = true;
            Debug.Log("Paused");
        }
        else
        {
            //Debug.Log("old pos: " + oldPosition);
            origin.transform.position = oldPosition;
            pauseMenu.gameObject.SetActive(false);
            //Time.timeScale = 1f;
            gamePaused = false;
            Debug.Log("Resumed");
        }
        waitForInput = false;
    }
}