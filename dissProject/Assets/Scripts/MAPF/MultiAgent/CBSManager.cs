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
            //Debug.Log("PATH IS NOT COLLISION FREE: ADDING NEW NODES");

            foreach(MAPFAgent agent in firstCollision.agents)
            {
                //Debug.Log("BEGIN Agent " + agent.agentId);
                ConflictTreeNode newNode = new ConflictTreeNode();
                foreach(Constraint constraint in workingNode.constraints)
                {
                    newNode.constraints.Add(constraint);
                }
                if (firstCollision.isVertex)
                {
                    //Debug.Log("CONSTRAINT " + firstCollision.node.position + firstCollision.timestep);
                    newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.timestep));
                }
                else
                {
                    //Debug.Log("CONSTRAINT " + firstCollision.node.position + ""+ firstCollision.node2.position + firstCollision.timestep);
                    newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.node2, firstCollision.timestep));
                }
                //Debug.Log("CONSTRAINT COUNT:" + newNode.constraints.Count);
                newNode.SetupSolution(_MAPFAgents);
                newNode.CalculateNodePaths(_gridGraph,dimensions);
                newNode.CalculateNodeCost();
                if (newNode.nodeCost != -1) // if a path has been found for all agents
                {
                    _openList.Enqueue(newNode, newNode.nodeCost);
                }
                //Debug.Log("END Agent " + agent.agentId);
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
    public HashSet<Constraint> constraints = new();
    public int nodeCost;
    int maxPathLength;
    Dictionary<MAPFAgent, List<MAPFNode>> solution; //a dictionary assigning one path to one agent

    public Dictionary<MAPFAgent, List<MAPFNode>> GetSolution()
    {
        return solution;
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
        Hashtable agentConstraints;
        Hashtable agentEdgeConstraints;
        
        foreach (MAPFAgent agent in solutionAgents)
        {
            agentConstraints = new();
            agentEdgeConstraints = new();
            foreach (Constraint constraint in constraints)
            {
                
                if (constraint.agent.agentId == agent.agentId)
                {
                    
                    if (constraint.isVertex)
                    {
                        agentConstraints.Add(constraint.node.position + "" + constraint.timestep, agent);
                    }
                    else
                    {
                        agentEdgeConstraints.Add(constraint.node.position + "" + constraint.node2.position + constraint.timestep, agent);
                    }
                    
                }
            }
            STAStar sTAStar = new STAStar();
            sTAStar.SetSTAStar(_gridGraph, dimensions);
            sTAStar.rTable = agentConstraints;
            sTAStar.edgeTable = agentEdgeConstraints;
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
        Hashtable edgesAtTimestep;
        for (int t=0; t < maxPathLength; t++)
        {

            positionsAtTimestep = new Hashtable();
            edgesAtTimestep = new Hashtable();
            foreach (MAPFAgent agent in solution.Keys)
            {
                
                List<MAPFNode> agentPath = solution[agent];
                if (agentPath.Count <= t) continue; //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.ContainsKey(agentPath[t].position))
                {
                    MAPFAgent[] agents = { agent, (MAPFAgent)positionsAtTimestep[agentPath[t].position] };
                    //Debug.Log("COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " at " + agentPath[t].position +" time " + (t));
                    return new Collision(agents, agentPath[t], t);
                }
                if (agentPath.Count <= t+1) continue; //if there is a node at the next timestep continue
                if (edgesAtTimestep.ContainsKey(agentPath[t].position + "" + agentPath[t+1].position))
                {
                    MAPFAgent[] agents = { agent, (MAPFAgent)edgesAtTimestep[agentPath[t].position + "" + agentPath[t+1].position] };
                    //Debug.Log("EDGE COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " edge " + agentPath[t].position +""+agentPath[t+1].position + "time " + (t));
                    return new Collision(agents, agentPath[t], agentPath[t+1], t);
                }
                positionsAtTimestep.Add(agentPath[t].position, agent);
                edgesAtTimestep.Add(agentPath[t].position + "" + agentPath[t + 1].position, agent);
                edgesAtTimestep.Add(agentPath[t+1].position + "" + agentPath[t].position, agent); //reserve edge in opposite direction too
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
    public MAPFNode node2;
    public int timestep;
    public bool isVertex;

    public Collision(MAPFAgent[] agents, MAPFNode node, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.timestep = timestep;
        isVertex = true;
    }
    public Collision(MAPFAgent[] agents, MAPFNode node, MAPFNode node2, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.node2 = node2;
        this.timestep = timestep;
        isVertex = false;
    }
}

public class Constraint
{
    public MAPFAgent agent;
    public MAPFNode node;
    public MAPFNode node2;
    public int timestep;
    public bool isVertex;

    public Constraint(MAPFAgent agent, MAPFNode node, int timestep)
    {
        this.agent = agent;
        this.node = node;
        this.timestep = timestep;
        isVertex = true;
    }
    public Constraint(MAPFAgent agent, MAPFNode node, MAPFNode node2, int timestep) //edge constraint constructor
    {
        this.agent = agent;
        this.node = node;
        this.node2 = node2;
        this.timestep = timestep;
        isVertex = false;
    }

}



