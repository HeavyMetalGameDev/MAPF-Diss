using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class ResultsWriter
{
    public static void WriteResult(string result, string algorithm, string map)
    {
        string path = Application.persistentDataPath + "/" + algorithm + "-" + map + ".txt";
        Debug.Log(path);
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(result);
        writer.Close();
    }
}
