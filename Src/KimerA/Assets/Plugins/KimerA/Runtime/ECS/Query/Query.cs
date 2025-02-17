namespace KimerA.ECS
{
    using System.Collections.Generic;

    public struct Query<TQuery> : IQuery
        where TQuery : IQueryable
    {
        private readonly IEnumerable<TQuery> query;

        public IEnumerator<TQuery> GetEnumerator()
        {
            return query.GetEnumerator();
        }
    }

    public struct Query<TQuery, TFilter> : IQuery
        where TQuery : IQueryable
        where TFilter : IFilterable
    {
        private readonly IEnumerable<TQuery> query;

        private readonly TFilter filter;

        public IEnumerator<TQuery> GetEnumerator()
        {
            return query.GetEnumerator();
        }
    }
}