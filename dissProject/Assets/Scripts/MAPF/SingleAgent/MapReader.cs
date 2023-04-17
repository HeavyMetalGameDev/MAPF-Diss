using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MapReader
{
    string _mapFileName;
    int _readIgnoreCounter = 4;
    List<string> _map = new List<string>();
    public Vector2Int ReadMapFromFile(string fileName)
    {
        _mapFileName = fileName;
        using (StreamReader reader = new StreamReader(_mapFileName))
        {
            string row;
            while ((row = reader.ReadLine()) != null)
            {
                if (_readIgnoreCounter > 0)
                {
                    _readIgnoreCounter--;
                    continue;
                }
                _map.Add(row);
            }
            _map.Reverse();

        }
        return new Vector2Int(_map[0].Length, _map.Count);
    }

    public  Node[] GetNodesFromMap()
    {
        List<Node> nodes = new List<Node>();
        Node newNode;
        for (int y = 0; y < _map.Count; y++)
        {
            for (int x = 0; x < _map[y].Length; x++)
            {
                char nodeChar = _map[y][x];
                if (nodeChar.Equals('.'))
                {
                    newNode = new Node(new Vector2Int(x * 5, y * 5), NodeTypeEnum.WALKABLE);
                    nodes.Add(newNode);
                }
                else if (nodeChar.Equals('@') || nodeChar.Equals('T'))
                {
                    //newNode = new Node(new Vector2Int(x * 5, y * 5), NodeTypeEnum.NOT_WALKABLE);
                    //nodes.Add(newNode);
                }
            }
        }
        return nodes.ToArray();
    }
}
