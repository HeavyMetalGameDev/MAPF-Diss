using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{

    public string selectedAlgorithm = "AStar";
    public string selectedMap = "empty-8-8";
    public int agentCount = 1;
    public int scenario = 1;

    public void HandleAlgorithmDropdown(int val)
    {
        switch (val)
        {
            case 0:
                selectedAlgorithm = "AStar";
                break;
            case 1:
                selectedAlgorithm = "CAStar";
                break;
            case 2:
                selectedAlgorithm = "HCAStar";
                break;
            case 3:
                selectedAlgorithm = "CBS";
                break;
            case 4:
                selectedAlgorithm = "CBS-DS";
                break;

        }
    }
    public void HandleMapDropdown(int val)
    {
        switch (val)
        {
            case 0:
                selectedMap = "empty-8-8";
                break;
            case 1:
                selectedMap = "maze-32-32-4";
                break;
            case 2:
                selectedMap = "brc202d";
                break;


        }
    }
    public void HandleScenarioDropdown(int val)
    {
        scenario = val + 1;
    }

    public void HandleAgentCount(string input)
    {
        if (input == "") return;
        agentCount = int.Parse(input);
    }

    public void Confirm()
    {
        DontDestroyOnLoad(this);
        SceneManager.LoadScene(1);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) return;
        MAPFGraphGrid graphGridObject = GameObject.FindGameObjectWithTag("Grid").GetComponent<MAPFGraphGrid>();
        graphGridObject._mapName = selectedMap;
        graphGridObject._agentCount = agentCount;
        graphGridObject.algorithmToUse = selectedAlgorithm;
        graphGridObject.scenarioNum = scenario;
        graphGridObject.Execute();
        Destroy(gameObject);
    }
}
