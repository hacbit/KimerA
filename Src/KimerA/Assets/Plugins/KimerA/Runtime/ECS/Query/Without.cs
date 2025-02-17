namespace KimerA.ECS
{
    public class Without<TQuery> : IFilterable
        where TQuery : IQueryable
    {
        
    }

    public class Without<TQuery1, TQuery2> : IFilterable
        where TQuery1 : IQueryable
        where TQuery2 : IQueryable
    {
        
    }

    public class Without<TQuery1, TQuery2, TQuery3> : IFilterable
        where TQuery1 : IQueryable
        where TQuery2 : IQueryable
        where TQuery3 : IQueryable
    {
        
    }
}