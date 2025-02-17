namespace KimerA.ECS
{
    public partial interface IPlugin
    {
        public void OnPluginLoad(App app)
        {
            RegisterSystems_Inject(app);
        }

        void RegisterSystems_Inject(App app)
        {

        }
    }
}