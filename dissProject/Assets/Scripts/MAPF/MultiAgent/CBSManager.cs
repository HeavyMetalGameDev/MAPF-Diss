using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBSManager
{
    List<List<MAPFNode>> _gridGraph;
    MAPFAgent[] _MAPFAgents;
    ConflictTreeNode rootOfTree;
    List<ConflictTreeNode> cTNodeList = new List<ConflictTreeNode>();

    //public Dictionary<MAPFAgent, List<MAPFNode>> Plan()
    {

    }
}


public class ConflictTreeNode
{
    ConflictTreeNode parentNode;
    ConflictTreeNode leftNode;
    ConflictTreeNode rightNode;
    Hashtable constraints;
    int nodeCost;
    Dictionary<MAPFAgent, List<MAPFNode>> solution; //a dictionary assigning one path to one agent

    public void SetupSolution(MAPFAgent[] agents) //add empty path for each agent
    {
        foreach (MAPFAgent agent in agents)
        {
            solution.Add(agent, new List<MAPFNode>());
        }
    }
    public void CalculateNodePaths()
    {
        foreach( MAPFAgent agent in solution.Keys)
        {
            STAStar sTAStar = new STAStar();
            solution[agent] = sTAStar.GetSTAStarPath(agent);
        }
    }

    public void VerifyPaths() //check each agents path at each timestep and if there is a collision, return the collision
    {

    }
}


