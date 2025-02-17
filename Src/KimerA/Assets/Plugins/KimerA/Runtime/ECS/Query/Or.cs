namespace KimerA.ECS
{
    public class Or<TFilter1, TFilter2> : IFilterable
        where TFilter1 : IFilterable
        where TFilter2 : IFilterable
    {
        
    }

    public class Or<TFilter1, TFilter2, TFilter3> : IFilterable
        where TFilter1 : IFilterable
        where TFilter2 : IFilterable
        where TFilter3 : IFilterable
    {
        
    }
}