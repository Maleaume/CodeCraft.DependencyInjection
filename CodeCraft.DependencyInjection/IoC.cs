using CodeCraft.Core.BaseArchitecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeCraft.DependencyInjection
{
    public class IoC : Singleton<IoC>
    {
        private readonly IConsistencyValidator ConcistencyValidator = new ConsistencyValidator();
        private readonly IoCContainer LazyImplementations = new IoCContainer();


        public bool IsRegister<Interface>(string name = "default")
        {
            var registerKey = GenerateRegisterKey<Interface>(name);
            return LazyImplementations.IsRegistered(registerKey);
        }

        private ContainerKey GenerateRegisterKey<Interface>(string name)
           => GenerateRegisterKey(typeof(Interface), name);

        private ContainerKey GenerateRegisterKey(Type interfaceType, string name)
            => new ContainerKey
            {
                InterfaceType = interfaceType,
                Name = name
            };
        
        public void RegisterType<Interface, Implementation>(string name = "default")
            where Interface : class
            where Implementation : class
        {
            if (!ConcistencyValidator.CheckConsistency<Interface, Implementation>()) throw new TypeAccessException();

            var registerKey = GenerateRegisterKey<Interface>(name);
            LazyImplementations[registerKey] = (typeof(Implementation), CreateLazyInstance<Interface>(name));
        }

        private Lazy<object> CreateLazyInstance<Interface>(string name)
            => new Lazy<object>(() => ResolveNewInstance<Interface>(name));

        public T Resolve<T>(string name = "default") => (T)Resolve(GenerateRegisterKey<T>(name));

        private object Resolve(ContainerKey registerKey) => LazyImplementations[registerKey].LazyInstance.Value;

        private T Instanciate<T>(string name) => (T)Instanciate(GenerateRegisterKey<T>(name));

        private object Instanciate(ContainerKey registerKey)
        {
            object instance;
            var implementation = LazyImplementations[registerKey].ImplementationType;
            var constructor = implementation.GetConstructors()[0];
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
                instance = Activator.CreateInstance(implementation);
            else
            {
                var parameters = new List<object>(constructorParameters.Length);
                foreach (var parameterInfo in constructorParameters)
                {
                    var registerParamKey = GenerateRegisterKey(parameterInfo.ParameterType, registerKey.Name);
                    parameters.Add(ResolveNewInstance(registerParamKey));
                }
                instance = constructor.Invoke(parameters.ToArray());
            }
            // Dependencies By Injection. 
            return instance;
        }

        public T ResolveNewInstance<T>(string name = "default") => (T)ResolveNewInstance(GenerateRegisterKey<T>(name));

        private object ResolveNewInstance(ContainerKey registerKey)
        {
            object instance = Instanciate(registerKey);

            ///////////////////////////////////// 
            var implementation = LazyImplementations[registerKey].ImplementationType;
            var injectedFileds = GetInjectedFields(implementation);

            foreach (var kp in injectedFileds)

            {
                var registerParamKey = GenerateRegisterKey(kp.FieldInfo.FieldType, registerKey.Name);
                var value = Resolve(registerParamKey);
                kp.FieldInfo.SetValue(instance, value);
            }
            ///////////////////// 
            return instance;
        }

        public void RegisterInstance<Interface>(Interface implementation, string name = "default")
        {
            if (Equals(implementation, default(Interface))) throw new ArgumentNullException("implementation", "Implementation cannot be null");
            var registerKey = GenerateRegisterKey<Interface>(name);
            LazyImplementations[registerKey] = (implementation.GetType(), new Lazy<object>(() => implementation));
        }

        private IEnumerable<InjectedFieldInfo> GetInjectedFields(Type implementation)
            => implementation
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Select(field => new InjectedFieldInfo
                {
                    FieldInfo = field,
                    InjectionAttribute = field.GetCustomAttribute<FieldInjectionAttribute>()
                })
                .Where(x => x.InjectionAttribute != null);


        class InjectedFieldInfo
        {
            public FieldInfo FieldInfo { get; set; }
            public FieldInjectionAttribute InjectionAttribute { get; set; }
        }

    }
}
