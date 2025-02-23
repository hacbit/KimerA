# KimerA

KimerA 工具集提供了一些实用工具类，和一些 Unity 编辑器拓展插件。除此之外，本工具集也致力于提供比 Unity 本身所支持的更高版本的 Dotnet / C# 的一些特性。



## Experimental Warning :warning:

***框架处于实验性阶段，请勿用于生产***



## Works with

-   Unity Editor 2022.3 (LTS) or later
-   Visual Studio, Visual Studio Code, Rider



## Install

使用 Package Manager 并通过下面的 git 链接来下载：

```
https://github.com/hacbit/KimerA.git?path=Src/KimerA/Assets/Plugins/KimerA
```



## Preparations for use in Unity

由于项目依赖了一些高版本的 C# 特性，所以导入项目后需要做以下准备

-   在你的 Unity 项目的 `Assets/` 目录下创建一个文件 `csc.rsp` ，写入如下内容：`-langversion:preview -nullable` 
    -   对于 Unity2022.2 及以上版本，unity 内置了 .NET SDK 6，可以通过设置编译参数 `-langversion:10` 使用 C#10
    -   对于 Unity2022.3.12f1 及以上版本，unity 内部更新了 .NET SDK 6，因此 C#11 的特性可通过设置 preview 使用

-   对于 IDE，需要使用新的 csproj 文件（否则在 IDE 中相关的代码还是会报错），工具集内置了 CsprojModifier 工具自动修改 csproj 文件（如有必要，可以手动打开该工具并重新生成），如果需要设置新的属性，可以修改 `Assets/Plugins/KimerA/Editor/CsprojModifier/CsprojModifierConfig.props` 文件，或者创建一个新的配置文件，并通过 `KimerA > Csproj Modifier` 工具添加该配置文件。（如果属性存在冲突，该属性的新值会覆盖旧值）



## Features

### 支持 C#11 部分特性

使用方法见上文。

>   **注：不支持全部特性，比如主构造函数，集合表达式等**

如果你使用**程序集（Assembly Definition）**，并且程序集也要**使用高版本特性**，需要在该 .asmdef 文件同目录下也创建一个 `csc.rsp` 文件，并写入需要的 **Roslyn 编译参数**。

然后找到 `Assets/Plugins/KimerA/CsprojModifier/CsprojsToChange.json` 文件，其默认的内容应该是：

```json
{
    "Csprojs": [
        "Assembly-CSharp.csproj",
        "Assembly-CSharp-Editor.csproj",
        "KimerA*.csproj"
    ]
}
```

将 Unity 给你的程序集自动生成的 .csproj 文件的文件名**添加到这个 json 文件**中即可，**支持正则表达式**。

工具集的 Csproj Modifier 工具会在每次代码编译后读取该文件并修改指定的 .csproj 文件以支持你的 IDE。

###  CollectionsMarshal 支持

关于这个也放在了我的另一个项目：https://github.com/hacbit/CollectionsMarshalForUnity



### 易拆装，高可定制化的模块

以工具集内置的存档系统为例：

你需要把需要存档的字段添加 `[Archivable]` ，并且需要给这个字段所在的 class 添加 `[ArchiveTo<>]` ，该通用 Attribute 的泛型参数需要接受具有 `[ArchiveReceiver]` 的类。

然后只需要在某一处调用注册函数即可（如果是 Mono 类，一般考虑 Awake；如果是普通类，直接在构造函数注册即可）。

你可以很轻松的就把原有代码移植过来，并且几乎不会影响你的代码。（你也仅需要移除相关的 Attribute 就可以。

经过上述步骤，被 `[ArchiveReceiver]` 标记的类在事实上就成为了一个 archive system，然后你可以随意在你的类中添加其他代码来拓展原有的系统

>   partial 关键字是必须的

```cs
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

```

