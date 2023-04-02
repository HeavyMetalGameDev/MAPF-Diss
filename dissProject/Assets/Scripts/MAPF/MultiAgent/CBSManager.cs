using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
public class CBSManager
{
    public List<List<MAPFNode>> _gridGraph;
    public MAPFAgent[] _MAPFAgents;
    public ConflictTreeNode rootOfTree;
    public Vector2 dimensions;
    public SimplePriorityQueue<ConflictTreeNode> _openList = new SimplePriorityQueue<ConflictTreeNode>();

    public Dictionary<MAPFAgent, List<MAPFNode>> Plan()
    {
        ConflictTreeNode rootNode = new ConflictTreeNode();
        rootNode.SetupSolution(_MAPFAgents);
        rootNode.CalculateNodePaths(_gridGraph, dimensions);
        rootNode.CalculateNodeCost();
        _openList.Enqueue(rootNode, rootNode.nodeCost);
        while (_openList.Count != 0)
        {
            ConflictTreeNode workingNode = _openList.Dequeue();
            Collision firstCollision = workingNode.VerifyPaths();
            if(firstCollision == null)
            {
                return workingNode.GetSolution();
            }

            foreach(MAPFAgent agent in firstCollision.agents)
            {
                ConflictTreeNode newNode = new ConflictTreeNode();
                newNode.constraints = workingNode.constraints;
                newNode.constraints.Add(firstCollision.node.position + "" + firstCollision.timestep, agent);
                newNode.SetupSolution(_MAPFAgents);
                newNode.CalculateNodePaths(_gridGraph,dimensions);
                newNode.CalculateNodeCost();
                if (newNode.nodeCost != -1) // if a path has been found for all agents
                {
                    _openList.Enqueue(newNode, newNode.nodeCost);
                }
            }
            
        }
        return null; //failed to find a solution
    }

    public CBSManager(List<List<MAPFNode>> gridGraph, MAPFAgent[] MAPFAgents, Vector2 dimensions)
    {
        _gridGraph = gridGraph;
        _MAPFAgents = MAPFAgents;
        this.dimensions = dimensions;
    }
    
    
}


public class ConflictTreeNode
{
    public Hashtable constraints = new Hashtable();
    public int nodeCost;
    int maxPathLength;
    Dictionary<MAPFAgent, List<MAPFNode>> solution; //a dictionary assigning one path to one agent

    public Dictionary<MAPFAgent, List<MAPFNode>> GetSolution()
    {
        return solution;
    }
    public Collision EvaluateNode(MAPFAgent[] agents, List<List<MAPFNode>> _gridGraph, Vector2 dimensions)
    {
        SetupSolution(agents);
        CalculateNodePaths(_gridGraph, dimensions);
        return VerifyPaths();
    }
    public void SetupSolution(MAPFAgent[] agents) //add empty path for each agent
    {
        solution = new Dictionary<MAPFAgent, List<MAPFNode>>();
        foreach (MAPFAgent agent in agents)
        {
            solution.Add(agent, new List<MAPFNode>());
        }
    }
    public void CalculateNodePaths(List<List<MAPFNode>> _gridGraph, Vector2 dimensions)
    {
        List<MAPFAgent> solutionAgents = new List<MAPFAgent>(solution.Keys);
        foreach( MAPFAgent agent in solutionAgents)
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
            List<MAPFNode> agentPath = sTAStar.GetSTAStarPath(agent);
            if(agentPath== null) //if we failed to find a path for an agent, exit out
            {
                nodeCost = -1;
                return;
            }
            solution[agent] = agentPath;
            if (solution[agent].Count > maxPathLength) maxPathLength = solution[agent].Count;
        }
        //Paths found successfully!
    }

    public Collision VerifyPaths() //check each agents path at each timestep and if there is a collision, return the collision, return null if no collisions occur
    {
        Hashtable positionsAtTimestep; //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        for (int t=0; t < maxPathLength; t++) //increment timestep
        {
            positionsAtTimestep = new Hashtable();
            foreach (MAPFAgent agent in solution.Keys)
            {
                List<MAPFNode> agentPath = agent.path;
                if (agentPath.Count <= t) continue; //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.Contains(agentPath[t]))
                {
                    MAPFAgent[] agents = { agent, (MAPFAgent)positionsAtTimestep[agentPath[t]] };
                    return new Collision(agents, agentPath[t], t);
                }
                positionsAtTimestep.Add(agentPath[t], agent);
            }
            
        }
        return null;
    }
    public void CalculateNodeCost()
    {
        if (nodeCost == -1) return;
        foreach(List<MAPFNode> path in solution.Values)
        {
            nodeCost += path.Count-1;
        }
    }



}
public class Collision
{
    public MAPFAgent[] agents;
    public MAPFNode node;
    public int timestep;

    public Collision(MAPFAgent[] agents, MAPFNode node, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.timestep = timestep;
    }
}



