using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace Quill.WebApi.Service
{
    public class ServiceContainer
    {
        /// <summary>
        /// 版本号长度（按“.”分隔成数组的最大数组长度）
        /// </summary>
        const int _maxVersionLength = 5;

        /// <summary>
        /// 乘数（用于计算版本号对应的数值）
        /// </summary>
        static int _multiplier = 0;

        /// <summary>
        /// 接口及其实例版本号的容器
        /// </summary>
        static Dictionary<Type, IEnumerable<decimal?>> _versionContainer;

        /// <summary>
        /// 依赖注入的容器
        /// </summary>
        public static IContainer Container { get; private set; }

        public static void Initialize(IEnumerable<Assembly> assemblies, Action<ContainerBuilder> action = null)
        {
            var types = assemblies.SelectMany(s => s.GetTypes());

            Initialize(types, action);
        }

        public static void Initialize(IEnumerable<Type> types, Action<ContainerBuilder> action = null)
        {
            var builder = new ContainerBuilder();

            RegisterInjectionService(builder, types);

            RegisterMultiVersionService(builder, types);

            if (action != null) action(builder);

            Container = builder.Build();
        }

        private static void RegisterInjectionService(ContainerBuilder builder, IEnumerable<Type> types)
        {
            var allTypes = types.Where(o => typeof(IInjectionService).IsAssignableFrom(o) && o != typeof(IInjectionService)).ToList();

            var basicTypes = allTypes.Where(o => o.IsInterface).ToList();

            var implementTypes = allTypes.Where(o => o.IsClass).ToList();

            foreach (var basicType in basicTypes)
            {
                var childTypes = implementTypes.Where(o => basicType.IsAssignableFrom(o)).ToList();
                if (!childTypes.Any())
                    continue;

                foreach (var childType in childTypes)
                    builder.RegisterType(childType).As(basicType).SingleInstance();
            }
        }

        private static void RegisterMultiVersionService(ContainerBuilder builder, IEnumerable<Type> types)
        {
            _versionContainer = new Dictionary<Type, IEnumerable<decimal?>>();

            var unit = decimal.MaxValue.ToString().Length / _maxVersionLength;
            _multiplier = (int)Math.Pow(10, unit);

            var allTypes = types.Where(o => typeof(IMultiVersionService).IsAssignableFrom(o) && o != typeof(IMultiVersionService)).ToList();

            var basicTypes = allTypes.Where(o => o.IsInterface).ToList();

            var implements = allTypes.Where(o => o.IsClass)
                                     .Select(o => new { Type = o, Attribute = (ServiceVersionAttribute)Attribute.GetCustomAttribute(o, typeof(ServiceVersionAttribute)) })
                                     .Where(o => o.Attribute != null)
                                     .ToList();

            foreach (var basicType in basicTypes)
            {
                var children = implements.Where(o => basicType.IsAssignableFrom(o.Type)).ToList();
                if (!children.Any())
                    continue;

                var versions = new List<decimal?>();
                foreach (var child in children)
                {
                    var versionValue = GetVersionValue(child.Attribute.Version);
                    versions.Add(versionValue);

                    builder.RegisterType(child.Type)
                           .As(basicType)
                           .Named(versionValue.ToString(), basicType)
                           .SingleInstance();
                }

                _versionContainer[basicType] = versions.Distinct().OrderByDescending(o => o);
            }
        }

        public static T Get<T>() where T : class, IMultiVersionService
        {
            return Get<T>(null);
        }

        public static T Get<T>(string version) where T : class, IMultiVersionService
        {
            var type = typeof(T);

            IEnumerable<decimal?> versions = null; //版本越高，排序越靠前

            if (!_versionContainer.TryGetValue(type, out versions))
                throw new ArgumentException(string.Format("service '{0}' not found", type.Name));

            var requestVersion = GetVersionValue(version);

            var actualVersion = versions.FirstOrDefault(o => o <= requestVersion) ?? versions.Last(); //未找到对应的版本时，返回最低版本

            return Container.ResolveOptionalNamed<T>(actualVersion.ToString());
        }

        public static decimal GetVersionValue(string version)
        {
            try
            {
                var value = 0M;

                if (string.IsNullOrEmpty(version))
                    return value;

                var list = version.Split('.')
                                  .Select(o => Convert.ToUInt32(o))
                                  .ToList();

                if (list.Count > _maxVersionLength)
                    throw new ArgumentException();

                if (list.Any(o => o > _multiplier * 10))
                    throw new ArgumentException();

                for (var i = 0; i < list.Count; i++)
                {
                    var power = _maxVersionLength - i - 1;
                    value += list[i] * (decimal)Math.Pow(_multiplier, power);
                }

                return value;
            }
            catch
            {
                throw new ArgumentException(string.Format("service version '{0}' error", version));
            }
        }
    }


    /// <summary>
    /// 自动注入接口，继承的Service将自动注入
    /// </summary>
    public interface IInjectionService { }


    /// <summary>
    /// 多版本控制接口，继承的Service将实现多版本控制
    /// </summary>
    public interface IMultiVersionService { }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ServiceVersionAttribute : Attribute
    {
        public ServiceVersionAttribute() { }

        public ServiceVersionAttribute(string version) : this() { this.Version = version; }

        public string Version { get; set; }
    }
}
