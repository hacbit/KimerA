using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;

struct Test
{
    public int A;

    public static implicit operator Test(int value)
    {
        return new Test { A = value };
    }
}

partial class CollectionsMarshalTest : MonoBehaviour
{
    public List<Test> list = new(10000);
    public List<Test> list2 = new(10000);

    public Dictionary<int, Test> dict = new(1000);
    public Dictionary<int, Test> dict2 = new(1000);

    void Start()
    {
        var ls = new List<Test> {1, 2, 3, 4, 5};

        var span = CollectionsMarshal.AsSpan(ls);
        span[0].A = 100;
        Debug.Assert(ls[0].A == 100);

        span[3].A = 400;
        Debug.Assert(ls[3].A == 400);

        CollectionsMarshal.SetCount(ls, 6);
        Debug.Assert(ls.Count == 6);
        Debug.Assert(ls[5].A == 0);

        var d = new Dictionary<int, Test> {{1, 1}, {2, 2}, {3, 3}, {4, 4}, {5, 5}};
        ref var val = ref CollectionsMarshal.GetValueRefOrNullRef(d, 1);
        val.A = 100;
        Debug.Assert(d[1].A == 100);

        val = ref CollectionsMarshal.GetValueRefOrNullRef(d, 6);
        Debug.Assert(Unsafe.IsNullRef(ref val));

        ref var val2 = ref CollectionsMarshal.GetValueRefOrAddDefault(d, 6, out var exists);
        Debug.Assert(exists == false);
        Debug.Assert(d[6].A == 0);
        val2.A = 200;
        Debug.Assert(d[6].A == 200);

        Debug.Log("Done");
    }

    [Button]
    private void TestList()
    {
        list.Clear();
        list2.Clear();
        for (int i = 0; i < 10000; i++)
        {
            list.Add(i);
            list2.Add(i);
        }

        long collectionsMarshalTime = MeasurePerformance(UsingCollectionsMarshal_List);
        Debug.Log($"UsingCollectionsMarshal: {collectionsMarshalTime} ticks");

        long listDirectlyTime = MeasurePerformance(UsingListDirectly);
        Debug.Log($"UsingDirectly: {listDirectlyTime} ticks");

        UnityEditor.EditorApplication.isPaused = true;
    }

    [Button]
    private void TestDict()
    {
        dict.Clear();
        dict2.Clear();
        for (int i = 0; i < 1000; i++)
        {
            dict.Add(i, i);
            dict2.Add(i, i);
        }

        long collectionsMarshalTime = MeasurePerformance(UsingCollectionsMarshal_Dict);
        Debug.Log($"UsingCollectionsMarshal: {collectionsMarshalTime} ticks");

        long dictDirectlyTime = MeasurePerformance(UsingDictDirectly);
        Debug.Log($"UsingDirectly: {dictDirectlyTime} ticks");

        UnityEditor.EditorApplication.isPaused = true;
    }

    private long MeasurePerformance(Action action)
    {
        const int iterations = 10000;
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Profiler.BeginSample(action.Method.Name);
        for (int i = 0; i < iterations; i++)
        {
            action();
        }
        Profiler.EndSample();

        stopwatch.Stop();
        return stopwatch.ElapsedTicks;
    }

    private void UsingCollectionsMarshal_List()
    {
        var span = CollectionsMarshal.AsSpan(list);
        for (int i = 0; i < span.Length; i++)
        {
            span[i].A++;
        }
    }

    private void UsingListDirectly()
    {
        for (int i = 0; i < list2.Count; i++)
        {
            var item = list2[i];
            item.A++;
            list2[i] = item;
        }
    }

    private void UsingCollectionsMarshal_Dict()
    {
        var keys = new List<int>(dict.Keys);
        foreach (var key in keys)
        {
            ref var item = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
            item.A++;
        }
    }

    private void UsingDictDirectly()
    {
        var keys = new List<int>(dict2.Keys);
        foreach (var key in keys)
        {
            var item = dict2[key];
            item.A++;
            dict2[key] = item;
        }
    }
}