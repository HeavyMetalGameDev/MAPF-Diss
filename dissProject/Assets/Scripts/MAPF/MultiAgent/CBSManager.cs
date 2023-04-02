using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBSManager
{
    public List<List<MAPFNode>> _gridGraph;
    public MAPFAgent[] _MAPFAgents;
    public ConflictTreeNode rootOfTree;
    public Vector2 dimensions;
    public List<ConflictTreeNode> cTNodeList = new List<ConflictTreeNode>();

    /*public Dictionary<MAPFAgent, List<MAPFNode>> Plan()
    {
        ConflictTreeNode rootNode = new ConflictTreeNode();
        rootNode.SetupSolution(_MAPFAgents);
        rootNode.CalculateNodePaths(_gridGraph, dimensions);

    }
    */
    
}


public class ConflictTreeNode
{
    ConflictTreeNode parentNode;
    ConflictTreeNode leftNode;
    ConflictTreeNode rightNode;
    Hashtable constraints = new Hashtable();
    int nodeCost;
    int maxPathLength;
    Dictionary<MAPFAgent, List<MAPFNode>> solution; //a dictionary assigning one path to one agent

    public Collision EvaluateNode(MAPFAgent[] agents, List<List<MAPFNode>> _gridGraph, Vector2 dimensions)
    {
        SetupSolution(agents);
        CalculateNodePaths(_gridGraph, dimensions);
        return VerifyPaths();
    }
    public void SetupSolution(MAPFAgent[] agents) //add empty path for each agent
    {
        foreach (MAPFAgent agent in agents)
        {
            solution.Add(agent, new List<MAPFNode>());
        }
    }
    public void CalculateNodePaths(List<List<MAPFNode>> _gridGraph, Vector2 dimensions)
    {
        foreach( MAPFAgent agent in solution.Keys)
        {
            //setup constraints for this agent
            Hashtable agentConstraints = new Hashtable();
            foreach(string constraint in constraints)
            {
                if((MAPFAgent)constraints[constraint] == agent)
                {
                    agentConstraints.Add(constraint, agent);
                }
            }

            STAStar sTAStar = new STAStar();
            sTAStar.SetSTAStar(_gridGraph, dimensions);
            sTAStar.rTable = agentConstraints;
            solution[agent] = sTAStar.GetSTAStarPath(agent);
            if (solution[agent].Count > maxPathLength) maxPathLength = solution[agent].Count;
        }
    }

    public Collision VerifyPaths() //check each agents path at each timestep and if there is a collision, return the collision, return null if no collisions occur
    {
        Hashtable positionsAtTimestep; //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        for (int t=0; t < maxPathLength; t++) //increment timestep
        {
            positionsAtTimestep = new Hashtable();
            foreach (MAPFAgent agent in solution.Keys)
            {
                if (agent.path.Count < t) continue; //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.Contains(agent.path[t]))
                {
                    return new Collision(agent, (MAPFAgent)positionsAtTimestep[agent.path[t]], agent.path[t], t);
                }
                positionsAtTimestep.Add(agent.path[t], agent);
            }
            
        }
        return null;
    }
    public void CalculateNodeCost()
    {
        foreach(List<MAPFNode> path in solution.Values)
        {
            nodeCost += path.Count-1;
        }
    }



}
public class Collision
{
    public MAPFAgent agent1;
    public MAPFAgent agent2;
    public MAPFNode node;
    public int timestep;

    public Collision(MAPFAgent agent1, MAPFAgent agent2, MAPFNode node, int timestep)
    {
        this.agent1 = agent1;
        this.agent2 = agent2;
        this.node = node;
        this.timestep = timestep;
    }
}



