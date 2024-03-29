using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MAPFMapReader
{
    string _mapFileName;
    List<string> _map = new List<string>();
    public Vector2Int ReadMapFromFile(string fileName)
    {
        _mapFileName = fileName;
        var loadedMap = (TextAsset)Resources.Load("BenchmarkMaps/" + fileName);
        _map = new List<string>(loadedMap.text.Split("\n"));
        _map.RemoveRange(0, 4);
        _map.Reverse();
        return new Vector2Int(_map[0].Length, _map.Count);
    }

    public List<List<MapNode>> GetNodesFromMap()
    {
        List<List<MapNode>> nodes = new List<List<MapNode>>();
        MapNode newNode;
        for (int y = 0; y < _map.Count; y++)
        {
            List<MapNode> subNodes = new List<MapNode>();
            for (int x = 0; x < _map[y].Length; x++)
            {
                char nodeChar = _map[y][x];
                if (nodeChar.Equals('.'))
                {
                    newNode = new MapNode(new Vector2Int(x * 5, y * 5), NodeTypeEnum.WALKABLE);
                    subNodes.Add(newNode);
                }
                else if (nodeChar.Equals('@') || nodeChar.Equals('T'))
                {
                    newNode = new MapNode(new Vector2Int(x * 5, y * 5), NodeTypeEnum.NOT_WALKABLE);
                    subNodes.Add(newNode);
                }
            }
            nodes.Add(subNodes);
        }
        return nodes;
    }
}
