using KimerA.ECS;
using UnityEngine;

namespace KimerA.Test
{
public sealed class ECSTest : MonoBehaviour
{
    private void Start()
    {
        new App()
            .WithConfig((ref AppConfig config) => config.FrameRate = 60)
            .Run();
    }
}
}