using KimerA.Utils;
using UnityEngine;

[ArchiveTo<MyArchive>]
public partial class TestArchive : MonoBehaviour
{
    [Archivable]
    private int ArchiveField = 0;
    [Archivable]
    public string ArchiveProperty { get; private set; } = "Hello";
    
    private void Awake()
    {
		MyArchive.Instance.TryRegister(this);
    }
    
    private void Start()
    {
        MyArchive.Instance.Save();
        ArchiveField = 114514;
        ArchiveProperty = "1919810";
        MyArchive.Instance.Load();
        Debug.Assert(ArchiveField is 0, "Load Field Failed");
        Debug.Assert(ArchiveProperty is "Hello", "Load Property Failed");
        Debug.Log("Test Passed");
    }
}

[ArchiveReceiver]
public partial class MyArchive {}