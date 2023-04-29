using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioReader
{
    public List<MAPFAgent> ReadScenarioAgents(string mapName, int scenarioID, int agentCount, List<List<MapNode>> gridGraph, GameObject agentPrefab)
    {
        List<MAPFAgent> loadedAgents = new();
        //Debug.Log("Scenarios/" + mapName + ".map-scen-even/scen-even/" + mapName + "-even-" + scenarioID);
        TextAsset scenario = (TextAsset)Resources.Load("Scenarios/" + mapName + ".map-scen-even/scen-even/" + mapName +"-even-" + scenarioID);
        List<string> agentsScenarioData = new List<string>(scenario.text.Split("\n"));
        agentsScenarioData.RemoveAt(0);
        for (int i = 0; i < agentCount; i++)
        {
            
            List<string> agentData = new List<string>(agentsScenarioData[i].Split("\t"));
            if (agentData.Count <=1) break;
            (int, int) dimensions = (int.Parse(agentData[2]), int.Parse(agentData[3]));
            MAPFAgent newAgent = GameObject.Instantiate(agentPrefab).GetComponent<MAPFAgent>();

            (int, int) startCoordinates = (int.Parse(agentData[4]), int.Parse(agentData[5]));
            newAgent.currentNode = gridGraph[dimensions.Item2 - startCoordinates.Item2-1][startCoordinates.Item1];

            (int, int) destinationCoordinates = (int.Parse(agentData[6]), int.Parse(agentData[7]));
            newAgent.destinationNode = gridGraph[dimensions.Item2 - destinationCoordinates.Item2-1][destinationCoordinates.Item1];

            newAgent.agentId = i;

            newAgent.transform.position = new Vector3(startCoordinates.Item1 * 5, 0, (dimensions.Item2 - startCoordinates.Item2-1) * 5);
            loadedAgents.Add(newAgent);
        }
        return loadedAgents;
    }
}
