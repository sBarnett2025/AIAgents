using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    public List<KeyValuePair<float, Vector2Int>> list = new List<KeyValuePair<float, Vector2Int>>();


    public void Enqueue(KeyValuePair<float, Vector2Int> newVar)
    {
        list.Add(newVar);
    }

    public KeyValuePair<float, Vector2Int> Dequeue()
    {
        float keepHighest = float.MinValue;
        int iTracker = 0;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Key > keepHighest)
            {
                keepHighest = list[i].Key;
                iTracker = i;
            }
        }
        KeyValuePair<float, Vector2Int> var = list[iTracker];
        list.RemoveAt(iTracker);
        return var;
    }




}
