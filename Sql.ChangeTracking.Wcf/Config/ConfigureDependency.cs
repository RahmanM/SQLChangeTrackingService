using SimpleInjector;

namespace ServiceTopShelf.DI
{
    public static class ConfigureDependency
    {
        public static Container Configure()
        {
            var container = new Container();
            container.Register<IDatabaseHelper, DatabaseHelper>();
            container.Register<ISqlTrackingManager, SqlTrackingManager>();
            container.Verify();
            return container;
        }
    }
}
