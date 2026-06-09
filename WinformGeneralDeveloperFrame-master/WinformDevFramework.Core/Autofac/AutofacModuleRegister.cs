using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Module = Autofac.Module;
using WinformDevFramework.Core.Logging;

namespace WinformDevFramework.Core.Autofac
{
    public class AutofacModuleRegister : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 注册日志服务
            LoggingConfig.RegisterLogger(builder);

            var basePath = AppContext.BaseDirectory;

            #region 带有接口层的服务注入

            var servicesDllFile = Path.Combine(basePath, "WinformDevFramework.Services.dll");
            var repositoryDllFile = Path.Combine(basePath, "WinformDevFramework.Repository.dll");

            if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
            {
                var msg = "Repository.dll和Services.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                throw new Exception(msg);
            }

            // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应对应 true 就行。
            //var cacheType = new List<Type>();
            //if (AppSettingsConstVars.RedisConfigEnabled)
            //{
            //    builder.RegisterType<RedisCacheAop>();
            //    cacheType.Add(typeof(RedisCacheAop));
            //}
            //else
            //{
            //    builder.RegisterType<MemoryCacheAop>();
            //    cacheType.Add(typeof(MemoryCacheAop));
            //}

            // 获取 Service.dll 程序集服务，并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            //支持属性注入依赖重复
            builder.RegisterAssemblyTypes(assemblysServices).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);


            // 获取 Repository.dll 程序集服务，并注册
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            //支持属性注入依赖重复
            builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            // 注册泛型仓库接口（使用反射避免直接引用）
            var genericRepositoryType = assemblysRepository.GetType("WinformDevFramework.Repository.GenericRepository`1");
            var baseRepositoryInterfaceType = assemblysRepository.GetType("WinformDevFramework.IRepository.IBaseRepository`1");
            if (genericRepositoryType != null && baseRepositoryInterfaceType != null)
            {
                builder.RegisterGeneric(genericRepositoryType).As(baseRepositoryInterfaceType).InstancePerDependency();
            }
            #endregion

        }
    }
}
