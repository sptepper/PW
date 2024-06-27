using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Voxelizer : MonoBehaviour
{
    // Dimension
    public const float VOX_DIM = 1;

    // Time
    public const float CYCLE_S = 120;
    public const float SWAP_S = 0.1f;

    public const int FORCE_SIZE = 40;

    public const int MAX_SWAP_CHECKS = 400;

    // CARNEROS Leak
    public List<Tuple<int, int>> LEAK_LOCK = new List<Tuple<int, int>>()
    {
        Tuple.Create(1, 38), //10
        Tuple.Create(4, 25), //9
        Tuple.Create(7, 20), //8
        Tuple.Create(10, 18), //7
        Tuple.Create(13, 29), //6
        Tuple.Create(16, 14), //5
    };

    [SerializeField]
    private GameObject Voxel_Prefab;

    [SerializeField]
    private GameObject Noxel_Prefab;

    [SerializeField]
    private GameObject Mystery_Prefab;

    public List<Blip> VoxelBars = new List<Blip>();

    public List<(int, int)> InversePairs = new List<(int, int)>();

    public Dictionary<int,GameObject> OrderedBars = new Dictionary<int,GameObject>();

    // Routines
    private IEnumerator imagecycle;
    private IEnumerator swapsort;

    private void Start()
    {
        Debug.Log("Voxelizing Bars");
        int height = 0;
        foreach (BarData.Bar b in BarData.BarLoader.instance.bars)
            VoxelizeBar(b, height++);

        // Lock known
        foreach(var l in LEAK_LOCK)
            VoxelBars[l.Item1].lockPos = l.Item2;

        // Add unknown Greys
        while(VoxelBars.Count < FORCE_SIZE)
            AddMysteryBar();

        RandomizeBars();


        Debug.Log("Beginning Cycle");
        imagecycle = CycleBars(CYCLE_S);
        StartCoroutine(imagecycle);

        Debug.Log("Beginning Swaps");
        //swapsort = SwapToMaxDist(SWAP_S);
        swapsort = SwapToMinDist(SWAP_S);
        StartCoroutine(swapsort);

        //FindInverseBars();
        //PositionInverses();


        //VoxelBars.Sort(CompareBySolveSort);

        //PositionBars();


    }

    public int CompareBySolveSort(Blip a, Blip b)
    {
        int c = CompareByFirstOn(a, b);
        if( c == 0)
        {
            c = CompareByFirstChange(a, b);
            if(c == 0)
            {
                c = CompareBySecondChange(a, b);
                if(c == 0)
                    return CompareByNumberOfChanges(a, b);
            }
        }
        return c;
    }

    public int CompareByNumberOfChanges(Blip a, Blip b)
    {
        return a.data.Number_Of_Changes().CompareTo(b.data.Number_Of_Changes());
    }

    public int CompareByFillPercent(Blip a, Blip b)
    {
        return a.data.On_Percent().CompareTo(b.data.On_Percent());
    }

    public int CompareByFirstOn(Blip a, Blip b)
    {
        return a.data.First_On().CompareTo(b.data.First_On());
    }

    public int CompareByFirstChange(Blip a, Blip b)
    {
        return a.data.First_Change().CompareTo(b.data.First_Change());
    }

    public int CompareBySecondChange(Blip a, Blip b)
    {
        return a.data.Second_Change().CompareTo(b.data.Second_Change());
    }

    private IEnumerator CycleBars(float t)
    {
        while (true)
        {
            yield return new WaitForSeconds(t);
            RandomizeBars();
        }
    }

    private IEnumerator SwapToMinDist(float t)
    {
        while (true)
        {
            yield return new WaitForSeconds(t);

            FindTwoToSwapMinimize();
            Debug.Log("Solution Distance: " + StackManager.TotalSolutionDistance(VoxelBars));
        }
    }

    private IEnumerator SwapToMaxDist(float t)
    {
        while (true)
        {
            yield return new WaitForSeconds(t);

            FindTwoToSwapMaximize();
            Debug.Log("Solution Distance: " + StackManager.TotalSolutionDistance(VoxelBars));
        }
    }

    private void FindTwoToSwapMinimize()
    {
        int init_dist = StackManager.TotalSolutionDistance(VoxelBars);

        int n = VoxelBars.Count;
        int k = rng.Next(n);
        int l = rng.Next(n);
        StackManager.Swap(VoxelBars, k, l);

        int i = 0;
        while(init_dist <= StackManager.TotalSolutionDistance(VoxelBars) &&  i < MAX_SWAP_CHECKS)
        {
            //swap back
            StackManager.Swap(VoxelBars, k, l);

            k = rng.Next(n);
            l = rng.Next(n);
            StackManager.Swap(VoxelBars, k, l);
            i++;
        }

        PositionBars();
    }

    private void FindTwoToSwapMaximize()
    {
        int init_dist = StackManager.TotalSolutionDistance(VoxelBars);

        int n = VoxelBars.Count;
        int k = rng.Next(n);
        int l = rng.Next(n);
        StackManager.Swap(VoxelBars, k, l);

        int i = 0;
        while (init_dist >= StackManager.TotalSolutionDistance(VoxelBars) && i < MAX_SWAP_CHECKS)
        {
            //swap back
            StackManager.Swap(VoxelBars, k, l);

            k = rng.Next(n);
            l = rng.Next(n);
            StackManager.Swap(VoxelBars, k, l);
            i++;
        }

        PositionBars();
    }

    public void AddMysteryBar()
    {
        GameObject barObj = new GameObject("[M]Blip" + VoxelBars.Count);
        Blip blip = barObj.AddComponent<Blip>();
        blip.ID = VoxelBars.Count;
        blip.data = null;

        for(int x = 0; x < BarData.Bar.LEN; x++)
        {
            var v = CreateMysteryVoxel(x);
            v.transform.SetParent(barObj.transform);
        }
        PositionBar(VoxelBars.Count, barObj);
        VoxelBars.Add(blip);
    }

    public void VoxelizeBar(BarData.Bar bar, int y)
    {
        GameObject barObj = new GameObject("Blip" + y);
        Blip blip = barObj.AddComponent<Blip>();
        blip.ID = y;
        blip.data = bar;

        int x = 0;
        foreach(bool b in bar.data)
        {
            var v = CreateVoxel(x, b);
            v.transform.SetParent(barObj.transform);
            x++;
        }

        PositionBar(y, barObj);
        VoxelBars.Add(blip);
    }

    private GameObject CreateVoxel(int col, bool b)
    {
        GameObject cube;
        if (b)
            cube = Instantiate(Voxel_Prefab);
        else
            cube = Instantiate(Noxel_Prefab);

        cube.transform.localScale = new Vector3(VOX_DIM, VOX_DIM, VOX_DIM);
        cube.transform.position = new Vector3(col * VOX_DIM,0, 0);
        return cube;
    }

    private GameObject CreateMysteryVoxel(int col)
    {
        GameObject cube = Instantiate(Mystery_Prefab);

        cube.transform.localScale = new Vector3(VOX_DIM, VOX_DIM, VOX_DIM);
        cube.transform.position = new Vector3(col * VOX_DIM, 0, 0);
        return cube;
    }

    private void PositionBars()
    {
        //Swap Locked Bars
        //foreach (Blip b in VoxelBars)
        for(int i = 0; i < VoxelBars.Count; i++)
        {
            if (VoxelBars[i].lockPos != -1)
                StackManager.Swap(VoxelBars, VoxelBars.IndexOf(VoxelBars[i]), VoxelBars[i].lockPos);
            //StackManager.SwapPosition(b.gameObject, VoxelBars[b.lockPos].gameObject);
        }

        int x = 0;
        foreach (Blip b in VoxelBars)
            PositionBar(x++, b.gameObject);

        StackManager.TotalSolutionDistance(VoxelBars);

        // Cosmetic Only
        RepositionLockedBars();
    }

    private void PositionInverses()
    {
        int x = 0;
        foreach ((int,int) i in InversePairs)
        {
            Blip b = VoxelBars.Find(b => b.ID == i.Item1);
            PositionBar(x++, b.gameObject);

            Blip b2 = VoxelBars.Find(b => b.ID == i.Item2);
            PositionBar(x++, b2.gameObject);
            x++;
            x++;
        }
    }

    private void PositionBar(int row, GameObject BarGroup)
    {
        BarGroup.transform.position = new Vector3(0, row * VOX_DIM, 0);
    }


    private void RepositionLockedBars()
    {
        foreach (Blip b in VoxelBars)
        {
            if (b.lockPos != -1)
                StackManager.SwapPosition(b.gameObject, VoxelBars[b.lockPos].gameObject);
        }
    }

    private static System.Random rng = new System.Random();

    private void RandomizeBars()
    {
        StackManager.Shuffle(VoxelBars);
        PositionBars();
    }

    private void FindInverseBars()
    {
        foreach (Blip b in VoxelBars)
        {
            foreach(Blip b2 in VoxelBars)
            {
                if (b.data.Distance(b2.data) == BarData.Bar.LEN)
                    InversePairs.Add((b.ID, b2.ID));
            }
        }
    }
}
