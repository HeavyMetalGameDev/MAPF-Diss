using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConflictManager
{
    bool[,,] _conflictTable;

    public void SetTable(int maxX, int maxY, int maxTimestep)
    {
        _conflictTable = new bool[maxX, maxY, maxTimestep];
    }

    public bool LookupReservation(int x, int y, int timestep)
    {
        if(_conflictTable[x, y, timestep])
        {
            return true;
        }
        _conflictTable[x, y, timestep] = true;
        return false;
    }
}
