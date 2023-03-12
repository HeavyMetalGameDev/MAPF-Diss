using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MapReader
{
    string _mapFileName;
    int _readIgnoreCounter = 4;
    List<string> _map = new List<string>();
    public Node[] ReadNodesFromFile(string fileName)
    {
        List<Node> nodes = new List<Node>();
        Node newNode;
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
            for(int y = 0; y <_map.Count;y++)
            {
                for(int x = 0; x< _map[y].Length; x++)
                {
                    char nodeChar = _map[y][x];
                    if (nodeChar.Equals('.'))
                    {
                        newNode = new Node(new Vector2(x * 5, y * 5), NodeTypeEnum.WALKABLE);
                        nodes.Add(newNode);
                    }
                    else if (nodeChar.Equals('@'))
                    {
                        newNode = new Node(new Vector2(x * 5, y * 5), NodeTypeEnum.NOT_WALKABLE);
                        nodes.Add(newNode);
                    }
                }
            }
            return nodes.ToArray();
        }
    }
}
