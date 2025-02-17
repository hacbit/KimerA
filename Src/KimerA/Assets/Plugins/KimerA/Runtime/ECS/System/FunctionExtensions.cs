namespace KimerA.ECS
{
    using System;

    [GenericGenerate(0, 16,
    namespaceName: @"System.Runtime.CompilerServices, System",
    methodTemplate: @"
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static App AddSystem{TypeArguments}(this App app, {TypeName}{TypeArguments} system)
    {Where}
{
    app.AddSystem(new SystemFunction{TypeArguments}(system));
    return app;
}", typeName: nameof(Action), typePrefix: "T", constraint: "ISystemParam")]
    public static partial class FunctionExtensions
    {
        // Inject code here
    }
}