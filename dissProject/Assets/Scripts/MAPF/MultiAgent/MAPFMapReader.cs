using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MAPFMapReader
{
    string _mapFileName;
    int _readIgnoreCounter = 4;
    List<string> _map = new List<string>();
    public Vector2 ReadMapFromFile(string fileName)
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
        return new Vector2(_map[0].Length, _map.Count);
    }

    public List<List<MAPFNode>> GetNodesFromMap()
    {
        List<List<MAPFNode>> nodes = new List<List<MAPFNode>>();
        MAPFNode newNode;
        for (int y = 0; y < _map.Count; y++)
        {
            List<MAPFNode> subNodes = new List<MAPFNode>();
            for (int x = 0; x < _map[y].Length; x++)
            {
                char nodeChar = _map[y][x];
                if (nodeChar.Equals('.'))
                {
                    newNode = new MAPFNode(new Vector2(x * 5, y * 5), NodeTypeEnum.WALKABLE);
                    subNodes.Add(newNode);
                }
                else if (nodeChar.Equals('@') || nodeChar.Equals('T'))
                {
                    newNode = new MAPFNode(new Vector2(x * 5, y * 5), NodeTypeEnum.NOT_WALKABLE);
                    subNodes.Add(newNode);
                }
            }
            nodes.Add(subNodes);
        }
        return nodes;
    }
}
