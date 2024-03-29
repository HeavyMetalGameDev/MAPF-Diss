using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Diagnostics;
using System.Linq;
public class CBSManager
{
    public List<List<MapNode>> _gridGraph;
    public List<MAPFAgent> _MAPFAgents;
    public Vector2Int dimensions;
    public SimplePriorityQueue<ConstraintTreeNode> _openList = new SimplePriorityQueue<ConstraintTreeNode>();
    public Dictionary<int, RRAStar> agentRRAStarDict = new Dictionary<int, RRAStar>();
    bool disjointSplitting = false;
    public int sumOfCosts;
    public long executionTime;
    Stopwatch sw = new Stopwatch();

    public Dictionary<MAPFAgent, List<MapNode>> Plan()
    {
        sw.Start();
        int expansions = 0;
        ConstraintTreeNode rootNode = new ConstraintTreeNode();
        rootNode.SetupSolution(_MAPFAgents);
        rootNode.CalculateAllAgentPaths(_gridGraph, dimensions, agentRRAStarDict,disjointSplitting);
        rootNode.CalculateNodeCost();
        _openList.Enqueue(rootNode, rootNode.nodeCost);
        while (_openList.Count != 0)
        {
            ConstraintTreeNode workingNode = _openList.Dequeue();
            if (sw.ElapsedMilliseconds >=30000) //30 second timeout
            {
                return null;
            }
            Conflict firstCollision = workingNode.VerifyPaths();
            //Debug.Log("PROCESSING NODE " + workingNode.nodeID);
            if (firstCollision == null)
            {
                sw.Stop();
                executionTime = sw.ElapsedMilliseconds;
                sumOfCosts = workingNode.nodeCost;
                return workingNode.GetSolution();
            }

            //Debug.Log("PATH IS NOT COLLISION FREE: ADDING NEW NODES");
            if (!disjointSplitting)
            {
                foreach (MAPFAgent agent in firstCollision.agents)
                {
                    expansions++;
                    //Debug.Log("BEGIN Agent " + agent.agentId);
                    ConstraintTreeNode newNode = new ConstraintTreeNode();
                    newNode.nodeID = expansions;
                    foreach (Constraint constraint in workingNode.constraints)
                    {
                        newNode.constraints.Add(constraint);
                    }
                    if (firstCollision.isForNode)
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
                    newNode.maxPathLength = workingNode.maxPathLength;
                    newNode.solution = new Dictionary<MAPFAgent, List<MapNode>>(workingNode.solution);
                    newNode.goalTimeDict = new Dictionary<MAPFAgent, int>(workingNode.goalTimeDict);
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
                    expansions++;
                    ConstraintTreeNode newNode = new ConstraintTreeNode();
                    newNode.nodeID = expansions;
                    Constraint newConstraint;
                    foreach (Constraint constraint in workingNode.constraints)
                    {
                        newNode.constraints.Add(constraint);
                    }
                    if (firstCollision.isForNode)
                    {
                        newConstraint = new Constraint(agent, firstCollision.node, firstCollision.timestep, disjointToggle);
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(newConstraint);
                    }
                    else
                    {
                        newConstraint = new Constraint(agent, firstCollision.node, firstCollision.node2, firstCollision.timestep, disjointToggle);
                        //Debug.Log("CONSTRAINT " + firstCollision.node.position + ""+ firstCollision.node2.position + firstCollision.timestep + " FOR AGENT " + agent.agentId);
                        newNode.constraints.Add(newConstraint);
                    }
                    //Debug.Log("CONSTRAINT COUNT:" + newNode.constraints.Count);
                    newNode.maxPathLength = workingNode.maxPathLength;
                    newNode.solution = new Dictionary<MAPFAgent, List<MapNode>>(workingNode.solution); //copy solution dict
                    newNode.goalTimeDict = new Dictionary<MAPFAgent, int>(workingNode.goalTimeDict);
                    if (!disjointToggle) //if we added a negative constraint, only replan that agent
                    {
                        newNode.CalculatePathForAgent(_gridGraph, dimensions, agent, agentRRAStarDict[agent.agentId]); //we only need to replan this agent
                    }
                    else //if we added a positive constraint, check all agents to see if they need to be replanned
                    {
                        List<MAPFAgent> agentsToReplan = new(); //seperate list so that the dictionary is not changed while searching
                        foreach (MAPFAgent agentToCheck in newNode.solution.Keys)
                        {
                            
                            if (agentToCheck.agentId == agent.agentId) continue; //do not check the agent we just positively constrained
                            if (CheckIfPathViolatesConstraint(newNode.solution[agentToCheck], newConstraint))
                            {
                                agentsToReplan.Add(agentToCheck);
                            }
                        }
                        foreach(MAPFAgent agentReplan in agentsToReplan)
                        {
                            newNode.CalculatePathForAgent(_gridGraph, dimensions, agentReplan, agentRRAStarDict[agent.agentId]);
                        } 
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

    bool CheckIfPathViolatesConstraint(List<MapNode> path, Constraint constraint)
    {
        MapNode nodeToCheck;
        if (path.Count > constraint.timestep)
        {
            nodeToCheck = path[constraint.timestep];
        }
        else
        {
            nodeToCheck = path.Last();
        }
        if (constraint.isForNode)
        {

            if (nodeToCheck.Equals(constraint.node))
            {
                return true;
            }
            return false;
        }
        MapNode secondNodeToCheck;
        if (path.Count > constraint.timestep+1)
        {
            secondNodeToCheck = path[constraint.timestep +1];
        }
        else
        {
            secondNodeToCheck = path.Last();
        }
        if (nodeToCheck.Equals(constraint.node) && secondNodeToCheck.Equals(constraint.node2))
        {
            return true;
        }
        else if (nodeToCheck.Equals(constraint.node2) && secondNodeToCheck.Equals(constraint.node))
        {
            return true;
        }
        return false;


        /*if (path.Count <= constraint.timestep) return false;
        if (constraint.isForNode)
        {
            if (path[constraint.timestep].Equals(constraint.node))
            {
                return true;
            }
            return false;
        }
        else
        {
            if (path.Count <= constraint.timestep + 1) return false;
            if (path[constraint.timestep].Equals(constraint.node) && path[constraint.timestep+1].Equals(constraint.node2))
            {
                return true;
            }
            else if(path[constraint.timestep].Equals(constraint.node2) && path[constraint.timestep + 1].Equals(constraint.node))
            {
                return true;
            }
            return false;
        }*/

    }
    public CBSManager(List<List<MapNode>> gridGraph, List<MAPFAgent> MAPFAgents, Vector2Int dimensions, bool disjointSplitting)
    {
        _gridGraph = gridGraph;
        _MAPFAgents = MAPFAgents;
        this.dimensions = dimensions;
        this.disjointSplitting = disjointSplitting;
    }
}


public class ConstraintTreeNode
{
    public int nodeID;
    public HashSet<Constraint> constraints = new();
    public int nodeCost;
    public int maxPathLength;
    public int numberOfCollisions;
    public Dictionary<MAPFAgent, List<MapNode>> solution; //a dictionary assigning one path to one agent
    public Dictionary<MAPFAgent, int> goalTimeDict;

    public Dictionary<MAPFAgent, List<MapNode>> GetSolution()
    {
        return solution;
    }
    public void SetupSolution(List<MAPFAgent> agents) //add empty path for each agent
    {
        solution = new ();
        goalTimeDict = new();
        foreach (MAPFAgent agent in agents)
        {
            solution.Add(agent, new List<MapNode>());
            goalTimeDict.Add(agent,0);
        }
    }
    public void CalculateAllAgentPaths(List<List<MapNode>> _gridGraph, Vector2Int dimensions, Dictionary<int, RRAStar> rraStarDict, bool positive)
    {
        
        List<MAPFAgent> solutionAgents = new List<MAPFAgent>(solution.Keys);
        foreach (MAPFAgent agent in solutionAgents)
        {
            CalculatePathForAgent(_gridGraph, dimensions, agent, rraStarDict[agent.agentId]);
        }
        //Paths found successfully!
    }

    public void CalculatePathForAgent(List<List<MapNode>> _gridGraph, Vector2Int dimensions, MAPFAgent agent, RRAStar rraStar)
    {
        //Debug.Log("NEW AGENT");
        Dictionary<(Vector2Int,int), MAPFAgent> agentConstraints = new();
        Dictionary<(Vector2Int,Vector2Int,int), MAPFAgent> agentEdgeConstraints = new();
        int lastConstraintTimestep = 0;

        foreach (Constraint constraint in constraints)
        {

            //add a negative constraint if this is a negative constraint for this agent or a positive constraint for another agent
            if ((constraint.agent.agentId == agent.agentId && !constraint.isPositive) ||(constraint.agent.agentId != agent.agentId && constraint.isPositive))
            {
                if (constraint.isForNode)
                {
                    //Debug.Log(constraint.isPositive + " " + constraint.node + constraint.timestep +" FOR AGENT " +constraint.agent.agentId + " THIS IS " + agent.agentId);
                    agentConstraints.TryAdd((constraint.node.position,constraint.timestep), agent);
                }
                else
                {
                    agentEdgeConstraints.TryAdd((constraint.node.position,constraint.node2.position,constraint.timestep), agent);
                    agentEdgeConstraints.TryAdd((constraint.node2.position,constraint.node.position,constraint.timestep), agent);
                }
                if(lastConstraintTimestep< constraint.timestep)
                {
                    lastConstraintTimestep = constraint.timestep;
                }

            }
        }
        AStarManager sTAStar = new AStarManager();
        sTAStar.SetSTAStar(_gridGraph, dimensions,rraStar);
        sTAStar.rTable = agentConstraints;
        sTAStar.edgeTable = agentEdgeConstraints;

        List<MapNode> agentPath = sTAStar.GetSTAStarPath(agent,false,true, lastConstraintTimestep);
        if (agentPath == null) //if we failed to find a path for an agent, exit out
        {
            //Debug.Log("NO PATH");
            nodeCost = -1;
            return;
        }
        solution[agent] = agentPath;
        goalTimeDict[agent] = sTAStar.finalTimestepThisAgent;
        if (solution[agent].Count > maxPathLength) maxPathLength = solution[agent].Count;
        //Paths found successfully!
    }

    public MapNode GetNodeAtTimestep(List<MapNode> path, int timestep)
    {
        if (path.Count > timestep)
        {
            return path[timestep];
        }
        else
        {
            return path.Last();
        }
    }
    public Conflict VerifyPaths() //check each agents path at each timestep and if there is a collision, return the collision, return null if no collisions occur
    {
        Dictionary<(Vector2Int,int), MAPFAgent> positionsTimestep = new(); //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        Dictionary<(Vector2Int,Vector2Int,int),MAPFAgent> edgesTimestep = new();
        List<Conflict> collisions = new(0);
        for (int t=0; t <= maxPathLength; t++)
        {
            foreach (MAPFAgent agent in solution.Keys)
            {
                List<MapNode> agentPath = solution[agent];
                MapNode nodeAtTimestep = GetNodeAtTimestep(agentPath, t);
                //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsTimestep.ContainsKey((nodeAtTimestep.position,t)))
                {
                    MAPFAgent[] agents = { agent, positionsTimestep[(nodeAtTimestep.position,t)] };
                    //Debug.Log("COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " at " + agentPath[t].position +" time " + (t));
                    collisions.Add(new Conflict(agents, nodeAtTimestep, t));
                }
                positionsTimestep.TryAdd((nodeAtTimestep.position, t), agent);
                //Debug.Log("ADDED " + agentPath[t].position + agent.agentId);
                if (agentPath.Count <= t+1) continue; //if there isnt a node at the next timestep continue
                if (edgesTimestep.ContainsKey((agentPath[t].position,agentPath[t+1].position,t)))
                {
                    MAPFAgent[] agents = { agent, edgesTimestep[(agentPath[t].position,agentPath[t+1].position,t)] };
                    //Debug.Log("EDGE COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " edge " + agentPath[t].position +""+agentPath[t+1].position + "time " + (t));
                    collisions.Add( new Conflict(agents, agentPath[t], agentPath[t+1], t));
                }
               
                if(!agentPath[t].position.Equals(agentPath[t + 1].position)) //only add an edge if the agent is travelling to a different node
                {
                    edgesTimestep.TryAdd((agentPath[t].position,agentPath[t + 1].position,t), agent);
                    edgesTimestep.TryAdd((agentPath[t + 1].position,agentPath[t].position,t), agent); //reserve edge in opposite direction too
                }
                
            }
            

        }
        numberOfCollisions = collisions.Count;
        if(numberOfCollisions == 0)
        {
            return null;
        }
        else
        {
            return collisions[0];
        }
        
    }
    public void CalculateNodeCost() //sum of costs value
    {
        if (nodeCost == -1) return;
        /*foreach(List<MapNode> path in solution.Values) //if padding isnt used
        {
            nodeCost += path.Count-1;
        }*/
        //nodeCost = numberOfCollisions; //suboptimal version of CBS that will prioritise low collisions over path cost
        nodeCost = 0;
        foreach (int time in goalTimeDict.Values)
        {
            
            nodeCost += time;
        }
    }



}
public class Conflict
{
    public MAPFAgent[] agents;
    public MapNode node;
    public MapNode node2;
    public int timestep;
    public bool isForNode;

    public Conflict(MAPFAgent[] agents, MapNode node, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.timestep = timestep;
        isForNode = true;
    }
    public Conflict(MAPFAgent[] agents, MapNode node, MapNode node2, int timestep)
    {
        this.agents = agents;
        this.node = node;
        this.node2 = node2;
        this.timestep = timestep;
        isForNode = false;
    }
}

public class Constraint
{
    public MAPFAgent agent;
    public MapNode node;
    public MapNode node2;
    public int timestep;
    public bool isForNode;
    public bool isPositive;

    public Constraint(MAPFAgent agent, MapNode node, int timestep,bool isPositive)
    {
        this.agent = agent;
        this.node = node;
        this.timestep = timestep;
        isForNode = true;
        this.isPositive = isPositive;
    }
    public Constraint(MAPFAgent agent, MapNode node, MapNode node2, int timestep, bool isPositive) //edge constraint constructor
    {
        this.agent = agent;
        this.node = node;
        this.node2 = node2;
        this.timestep = timestep;
        isForNode = false;
        this.isPositive = isPositive;
    }

    public override string ToString()
    {
        if (isForNode)
        {
            return node + "" + timestep;
        }
        return node + "" + node2 + "" + timestep;
    }

}


