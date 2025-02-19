using KimerA.Utils;
using UnityEngine;

[ArchiveTo<MyArchive>]
public partial class NotMono
{
    public static NotMono Instance = new();

    [Archivable]
    public int ArchiveField = 2333;

    NotMono()
    {
        MyArchive.Instance.TryRegister(this);
    }
}

[ArchiveTo<MyArchive>]
public partial class TestArchive : MonoBehaviour
{
    [Archivable]
    private int ArchiveField = 114514;
    [Archivable]
    private string ArchiveProperty { get; set; } = "1919810";

    private void Awake()
    {
        MyArchive.Instance.TryRegister(this);
    }

    private void Start()
    {
        MyArchive.Instance.Save();

        NotMono.Instance.ArchiveField = 123456;
        ArchiveField = 123456;
        ArchiveProperty = "123456";

        MyArchive.Instance.Load();

        Debug.Assert(NotMono.Instance.ArchiveField == 2333);
        Debug.Assert(ArchiveField == 114514);
        Debug.Assert(ArchiveProperty == "1919810");

        Debug.Log("Test passed!");
    }
}

[ArchiveReceiver]
public partial class MyArchive {}
