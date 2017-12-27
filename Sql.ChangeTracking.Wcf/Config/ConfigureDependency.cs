using SimpleInjector;
using SimpleInjector.Integration.Wcf;
using Sql.ChangeTracking.Common;
using Sql.ChangeTracking.Wcf;

namespace SqlChangeTrackingProducerConsumer.DI
{
    public static class ConfigureDependency
    {
        public static Container container;

        public static Container Configure()
        {
            container = new Container();
            //container.Options.DefaultScopedLifestyle = new WcfOperationLifestyle();

            container.Register<IDatabaseHelper, DatabaseHelper>(Lifestyle.Singleton);
            container.Register<ISqlTrackingManager, SqlTrackingManager>(Lifestyle.Singleton);
            container.Register<IChangeTrackingSubscriptions, SqlChangeTrackingWcfService>(Lifestyle.Singleton);
            container.Verify();
            return container;
        }
    }
}
