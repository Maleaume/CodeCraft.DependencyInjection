using CodeCraft.DependencyInjection.Relfection;
using CodeCraft.DependencyInjection.Validator;
using System;
using System.Collections.Generic; 

namespace CodeCraft.DependencyInjection
{
     
    public class IoC : Singleton<IoC>
    {
        private readonly IConsistencyValidator ConcistencyValidator = new ConsistencyValidator();
        private readonly IoCContainer LazyImplementations = new IoCContainer();
        private readonly IInjectedMemberService InjectedMemberService = new InjectedMembersService();

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

        public T Resolve<T>(string name = "default")
        {
            if (typeof(T).IsInterface)
                return (T)Resolve(GenerateRegisterKey<T>(name));
            else
                return (T)Instanciate(typeof(T));

        }

        private object Resolve(ContainerKey registerKey) => LazyImplementations[registerKey].LazyInstance.Value;

        private T Instanciate<T>(string name) => (T)Instanciate(GenerateRegisterKey<T>(name));

        private object Instanciate(ContainerKey registerKey)
        {
            try
            {
                var implementation = LazyImplementations[registerKey].ImplementationType;
                return Instanciate(implementation);
            }
            catch (KeyNotFoundException ex)
            {
                throw new IocException($"the key: {registerKey.InterfaceType.Name}(\"{registerKey.Name}\") does not exist in  container.", ex);
            }
        }

        private object Instanciate(Type implementation )
        { 
            object instance;
           // var constructor = implementation.GetConstructors()[0];
          //  ParameterInfo[] constructorParameters = constructor.GetParameters();
           // if (constructorParameters.Length == 0)
                instance = Activator.CreateInstance(implementation);
            /* else
             {
                 var parameters = new List<object>(constructorParameters.Length);
                 foreach (var parameterInfo in constructorParameters)
                 {
                     var registerParamKey = GenerateRegisterKey(parameterInfo.ParameterType, registerKey.Name);
                     parameters.Add(ResolveNewInstance(registerParamKey));
                 }
                 instance = constructor.Invoke(parameters.ToArray());
             }*/
            // Dependencies By Injection.
            SetInjectedFields(instance, implementation);
            SetInjectedProperties(instance, implementation);
            return instance;
        }

        public object Resolve(InjectionType injectionType, ContainerKey key)
        {
            switch (injectionType)
            {
                case InjectionType.Singleton: return Resolve(key);
                case InjectionType.NewInstance: return ResolveNewInstance(key);
                default: throw new ArgumentException();
            }
        }

        public T ResolveNewInstance<T>(string name = "default") => (T)ResolveNewInstance(GenerateRegisterKey<T>(name));

        private object ResolveNewInstance(ContainerKey registerKey)
        {
            object instance = Instanciate(registerKey);
            var implementation = LazyImplementations[registerKey].ImplementationType;
            SetInjectedFields(instance, implementation);
            SetInjectedProperties(instance, implementation); 
            return instance;  
        }

        void SetInjectedFields(object instance, Type implementation)
        {
            foreach (var kp in InjectedMemberService.GetInjectedFields(implementation))
            {
                var registerParamKey = GenerateRegisterKey(kp.MembersInfos.FieldType, kp.InjectionAttribute.Name);
                var value = Resolve(kp.InjectionAttribute.InjectionType, registerParamKey);
                kp.MembersInfos.SetValue(instance, value);
            }
        }

        void SetInjectedProperties(object instance, Type implementation)
        {
            foreach (var kp in InjectedMemberService.GetInjectedProperties(implementation))
            {
                var registerParamKey = GenerateRegisterKey(kp.MembersInfos.PropertyType, kp.InjectionAttribute.Name);
                var value = Resolve(kp.InjectionAttribute.InjectionType, registerParamKey);
                kp.MembersInfos.SetValue(instance, value);
            }
        }

        public void RegisterInstance<Interface>(Interface implementation, string name = "default")
        {
            if (Equals(implementation, default(Interface))) throw new ArgumentNullException("implementation", "Implementation cannot be null");
            var registerKey = GenerateRegisterKey<Interface>(name);
            LazyImplementations[registerKey] = (implementation.GetType(), new Lazy<object>(() => implementation));
        }
    }
}
