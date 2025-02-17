namespace KimerA.ECS
{
    public readonly struct Res<TRes> : IQuery where TRes : IResource
    {
        private readonly TRes res;

        public Res(TRes res) => this.res = res;

        public TRes Get() => res;

        public bool IsEmpty() => res == null;

        public bool TryGet(out TRes res)
        {
            res = this.res;
            return res != null;
        }
    }
}