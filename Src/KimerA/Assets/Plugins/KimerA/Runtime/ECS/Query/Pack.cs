namespace KimerA.ECS
{
    public struct Pack<T1, T2> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = Item1;
            item2 = Item2;
        }
    }

    public struct Pack<T1, T2, T3> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
        }
    }

    public struct Pack<T1, T2, T3, T4> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
        where T4 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
        }
    }

    public struct Pack<T1, T2, T3, T4, T5> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
        where T4 : IQueryable
        where T5 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
        }
    }

    public struct Pack<T1, T2, T3, T4, T5, T6> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
        where T4 : IQueryable
        where T5 : IQueryable
        where T6 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public T6 Item6 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
        }
    }

    public struct Pack<T1, T2, T3, T4, T5, T6, T7> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
        where T4 : IQueryable
        where T5 : IQueryable
        where T6 : IQueryable
        where T7 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public T6 Item6 { get; set; }

        public T7 Item7 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
            item7 = Item7;
        }
    }

    public struct Pack<T1, T2, T3, T4, T5, T6, T7, T8> : IQueryable
        where T1 : IQueryable
        where T2 : IQueryable
        where T3 : IQueryable
        where T4 : IQueryable
        where T5 : IQueryable
        where T6 : IQueryable
        where T7 : IQueryable
        where T8 : IQueryable
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public T6 Item6 { get; set; }

        public T7 Item7 { get; set; }

        public T8 Item8 { get; set; }

        public readonly void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
            item7 = Item7;
            item8 = Item8;
        }
    }
}