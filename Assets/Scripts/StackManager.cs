using System.Collections.Generic;
using UnityEngine;

public static class StackManager
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void ShuffleWithLocked<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    public static void SwapPosition(GameObject thing, GameObject other)
    {
        Vector3 p1 = new Vector3(thing.transform.position.x,
            thing.transform.position.y, thing.transform.position.z);

        thing.transform.position = new Vector3(other.transform.position.x, 
            other.transform.position.y,other.transform.position.z);

        other.transform.position = p1;
    }

    public static int TotalSolutionDistance(List<Blip> VoxelBars)
    {
        int total = 0;
        for(int i = 0; i < VoxelBars.Count - 1; i++)
        {
            // Unknowns have 0 distance to and from
            if (VoxelBars[i].data == null || VoxelBars[i+1].data == null)
                continue;

            int a = VoxelBars[i].data.Distance(VoxelBars[i + 1].data);
            //Debug.Log(i + "->" + (i + 1) + ": " + a);
            total += a;
        }

        //Debug.Log("Permutation Total: " + total);
        return total;
    }
}
