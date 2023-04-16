using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
public class CBSManager
{
    public List<List<MapNode>> _gridGraph;
    public List<MAPFAgent> _MAPFAgents;
    public Vector2 dimensions;
    public SimplePriorityQueue<ConflictTreeNode> _openList = new SimplePriorityQueue<ConflictTreeNode>();
    public Dictionary<int, RRAStar> agentRRAStarDict = new Dictionary<int, RRAStar>();
    bool disjointSplitting = false;
    public Dictionary<MAPFAgent, List<MapNode>> Plan()
    {
        int expansions = 0;
        ConflictTreeNode rootNode = new ConflictTreeNode();
        rootNode.SetupSolution(_MAPFAgents);
        rootNode.CalculateAllAgentPaths(_gridGraph, dimensions, agentRRAStarDict);
        rootNode.CalculateNodeCost();
        _openList.Enqueue(rootNode, rootNode.nodeCost);
        while (_openList.Count != 0)
        {
            if (expansions >= 5000) return null;
            ConflictTreeNode workingNode = _openList.Dequeue();
            Collision firstCollision = workingNode.VerifyPaths();
            if(firstCollision == null)
            {
                return workingNode.GetSolution();
            }
            //Debug.Log("PATH IS NOT COLLISION FREE: ADDING NEW NODES");
            if (!disjointSplitting)
            {
                foreach (MAPFAgent agent in firstCollision.agents)
                {
                    expansions++;
                    //Debug.Log("BEGIN Agent " + agent.agentId);
                    ConflictTreeNode newNode = new ConflictTreeNode();
                    foreach (Constraint constraint in workingNode.constraints)
                    {
                        newNode.constraints.Add(constraint);
                    }
                    if (firstCollision.isVertex)
                    {
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.timestep, false));
                    }
                    else
                    {
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + ""+ firstCollision.node2.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.node2, firstCollision.timestep, false));
                    }
                    //Debug.Log("CONSTRAINT COUNT:" + newNode.constraints.Count);
                    newNode.parent = workingNode;
                    newNode.solution = new Dictionary<MAPFAgent, List<MapNode>>(workingNode.solution);
                    newNode.CalculatePathForAgent(_gridGraph, dimensions, agent, agentRRAStarDict[agent.agentId]);
                    newNode.CalculateNodeCost();
                    if (newNode.nodeCost != -1) // if a path has been found for all agents
                    {
                        _openList.Enqueue(newNode, newNode.nodeCost);
                    }
                    //Debug.Log("END Agent " + agent.agentId);

                }
            }
            else
            {
                int random = Random.Range(0, 2);
                MAPFAgent agent = firstCollision.agents[random];
                bool disjointToggle = false;
                for(int i = 0; i < 2; i++) //run twice and toggle disjointToggle after first run
                {
                    ConflictTreeNode newNode = new ConflictTreeNode();
                    foreach (Constraint constraint in workingNode.constraints)
                    {
                        newNode.constraints.Add(constraint);
                    }
                    if (firstCollision.isVertex)
                    {
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.timestep, disjointToggle));
                    }
                    else
                    {
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + ""+ firstCollision.node2.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(new Constraint(agent, firstCollision.node, firstCollision.node2, firstCollision.timestep, disjointToggle));
                    }
                    //Debug.Log("CONSTRAINT COUNT:" + newNode.constraints.Count);
                    newNode.parent = workingNode;
                    newNode.solution = new Dictionary<MAPFAgent, List<MapNode>>(workingNode.solution);
                    if (!disjointToggle)
                    {
                        newNode.CalculatePathForAgent(_gridGraph, dimensions, agent, agentRRAStarDict[agent.agentId]);
                    }
                    else
                    {
                        newNode.CalculateAllAgentPaths(_gridGraph, dimensions,agentRRAStarDict);
                    }
                    
                    newNode.CalculateNodeCost();
                    if (newNode.nodeCost != -1) // if a path has been found for all agents
                    {
                        _openList.Enqueue(newNode, newNode.nodeCost);
                    }
                    disjointToggle = !disjointToggle;
                }

            }

        }
        return null; //failed to find a solution
    }

    public CBSManager(List<List<MapNode>> gridGraph, List<MAPFAgent> MAPFAgents, Vector2 dimensions, bool disjointSplitting)
    {
        _gridGraph = gridGraph;
        _MAPFAgents = MAPFAgents;
        this.dimensions = dimensions;
        this.disjointSplitting = disjointSplitting;
    }
}


public class ConflictTreeNode
{
    public ConflictTreeNode parent;
    public HashSet<Constraint> constraints = new();
    public int nodeCost;
    int maxPathLength;
    public Dictionary<MAPFAgent, List<MapNode>> solution; //a dictionary assigning one path to one agent

    public Dictionary<MAPFAgent, List<MapNode>> GetSolution()
    {
        return solution;
    }
    public void SetupSolution(List<MAPFAgent> agents) //add empty path for each agent
    {
        solution = new Dictionary<MAPFAgent, List<MapNode>>();
        foreach (MAPFAgent agent in agents)
        {
            solution.Add(agent, new List<MapNode>());
        }
    }
    public void CalculateAllAgentPaths(List<List<MapNode>> _gridGraph, Vector2 dimensions, Dictionary<int, RRAStar> rraStarDict)
    {
        
        List<MAPFAgent> solutionAgents = new List<MAPFAgent>(solution.Keys);
        foreach (MAPFAgent agent in solutionAgents)
        {
            CalculatePathForAgent(_gridGraph, dimensions, agent, rraStarDict[agent.agentId]);
        }
        //Paths found successfully!
    }

    public void CalculatePathForAgent(List<List<MapNode>> _gridGraph, Vector2 dimensions, MAPFAgent agent, RRAStar rraStar)
    {
        Dictionary<(Vector2,int), MAPFAgent> agentConstraints;
        Dictionary<(Vector2,Vector2, int), MAPFAgent> agentEdgeConstraints;
        Dictionary<string, MAPFAgent> agentPositiveConstraints;
        agentConstraints = new();
        agentEdgeConstraints = new();
        agentPositiveConstraints = new();
        foreach (Constraint constraint in constraints)
        {

            if ((constraint.agent.agentId == agent.agentId && !constraint.isPositive) ||(constraint.agent.agentId != agent.agentId && constraint.isPositive))
            {
                if (constraint.isVertex)
                {
                    agentConstraints.Add((constraint.node.position,constraint.timestep), agent);
                }
                else
                {
                    agentEdgeConstraints.Add((constraint.node.position,constraint.node2.position,constraint.timestep), agent);
                    agentEdgeConstraints.Add((constraint.node2.position,constraint.node.position,constraint.timestep), agent);
                }

            }
            else if (constraint.agent.agentId == agent.agentId && constraint.isPositive)
            {
                if (constraint.isVertex)
                {
                    agentPositiveConstraints.Add(constraint.node.position + "" + constraint.timestep, agent);
                }
                else
                {
                    agentPositiveConstraints.Add(constraint.node.position + "" + constraint.node2.position + constraint.timestep, agent);
                    agentPositiveConstraints.Add(constraint.node2.position + "" + constraint.node.position + constraint.timestep, agent);
                }
            }
        }
        STAStar sTAStar = new STAStar();
        sTAStar.SetSTAStar(_gridGraph, dimensions,rraStar);
        sTAStar.rTable = agentConstraints;
        sTAStar.edgeTable = agentEdgeConstraints;
        sTAStar.positiveConstraints = agentPositiveConstraints;
        List<MapNode> agentPath = sTAStar.GetSTAStarPath(agent,false);
        if (agentPath == null) //if we failed to find a path for an agent, exit out
        {
            nodeCost = -1;
            return;
        }
        solution[agent] = agentPath;
        if (solution[agent].Count > maxPathLength) maxPathLength = solution[agent].Count;
        //Paths found successfully!
    }

    public Collision VerifyPaths() //check each agents path at each timestep and if there is a collision, return the collision, return null if no collisions occur
    {
        Dictionary<Vector2, MAPFAgent> positionsAtTimestep; //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        Dictionary<(Vector2,Vector2),MAPFAgent> edgesAtTimestep;
        for (int t=0; t <= maxPathLength; t++)
        {

            positionsAtTimestep = new();
            edgesAtTimestep = new ();
            foreach (MAPFAgent agent in solution.Keys)
            {
                Debug.Log(agent.agentId);
                List<MapNode> agentPath = solution[agent];
                if (agentPath.Count <= t)
                {
                    //positionsAtTimestep.Add(agentPath[^1].position, agent);
                    continue;
                }
                //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.ContainsKey(agentPath[t].position))
                {
                    MAPFAgent[] agents = { agent, positionsAtTimestep[agentPath[t].position] };
                    //Debug.Log("COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " at " + agentPath[t].position +" time " + (t));
                    return new Collision(agents, agentPath[t], t);
                }
                positionsAtTimestep.Add(agentPath[t].position, agent);
                if (agentPath.Count <= t+1) continue; //if there isnt a node at the next timestep continue
                if (edgesAtTimestep.ContainsKey((agentPath[t].position,agentPath[t+1].position)))
                {
                    MAPFAgent[] agents = { agent, edgesAtTimestep[(agentPath[t].position,agentPath[t+1].position)] };
                    //Debug.Log("EDGE COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " edge " + agentPath[t].position +""+agentPath[t+1].position + "time " + (t));
                    return new Collision(agents, agentPath[t], agentPath[t+1], t);
                }
               
                if(!agentPath[t].position.Equals(agentPath[t + 1].position)) //only add an edge if the agent is travelling to a different node
                {
                    edgesAtTimestep.Add((agentPath[t].position,agentPath[t + 1].position), agent);
                    edgesAtTimestep.Add((agentPath[t + 1].position,agentPath[t].position), agent); //reserve edge in opposite direction too
                }
                
            }
            
        }
        return null;
    }
    public void CalculateNodeCost()
    {
        if (nodeCost == -1) return;
        foreach(List<MapNode> path in solution.Values)
        {
            nodeCost += path.Count-1;
        }
    }



}
public class Collision
{
    public MAPFAgent[] agents;
    public MapNode node;
    public MapNode node2;
    public int timestep;
    public bool isVertex;

    public Collision(MAPFAgent[] agents, MapNode node, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.timestep = timestep;
        isVertex = true;
    }
    public Collision(MAPFAgent[] agents, MapNode node, MapNode node2, int timestep)
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
    public MapNode node;
    public MapNode node2;
    public int timestep;
    public bool isVertex;
    public bool isPositive;

    public Constraint(MAPFAgent agent, MapNode node, int timestep,bool isPositive)
    {
        this.agent = agent;
        this.node = node;
        this.timestep = timestep;
        isVertex = true;
        this.isPositive = isPositive;
    }
    public Constraint(MAPFAgent agent, MapNode node, MapNode node2, int timestep, bool isPositive) //edge constraint constructor
    {
        this.agent = agent;
        this.node = node;
        this.node2 = node2;
        this.timestep = timestep;
        isVertex = false;
        this.isPositive = isPositive;
    }

    public override string ToString()
    {
        if (isVertex)
        {
            return node + "" + timestep;
        }
        return node + "" + node2 + "" + timestep;
    }

}




