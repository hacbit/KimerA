namespace KimerA.ECS
{
    using System;

    public class SystemParams
    {
        private readonly ISystemParam[] @params;

        private readonly Type[] paramTypes;

        public SystemParams(params ISystemParam[] systemParams)
        {
            @params = systemParams;
            paramTypes = new Type[systemParams.Length];
            for (var i = 0; i < systemParams.Length; i++)
            {
                paramTypes[i] = systemParams[i].GetType();
            }
        }

        public T? Get<T>()
            where T : ISystemParam
        {
            for (var i = 0; i < @params.Length; i++)
            {
                if (paramTypes[i] == typeof(T))
                {
                    return (T)@params[i];
                }
            }

            return default;
        }

        
    }
}