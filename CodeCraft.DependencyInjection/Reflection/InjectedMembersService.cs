using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CodeCraft.DependencyInjection.Relfection
{
    class InjectedMembersService : IInjectedMemberService
    {
        private readonly BindingFlags researchFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private delegate MemberInfo[] GetMembers(Type implementation);

        public IEnumerable<InjectedFieldInfo> GetInjectedFields(Type implementation)
            => GetInjectedMembers<InjectedFieldInfo, FieldInfo>(implementation, GetFields);

        public IEnumerable<InjectedPropertiesInfo> GetInjectedProperties(Type implementation)
            => GetInjectedMembers<InjectedPropertiesInfo, PropertyInfo>(implementation, GetProperties);

        private MemberInfo[] GetProperties(Type implementation)
        { if (typeof(object) == implementation) return new PropertyInfo[] { };
            return implementation.GetProperties(researchFlags).Concat(GetProperties(implementation.BaseType)).ToArray();
        }
        private MemberInfo[] GetFields(Type implementation)
        {
            if (typeof(object) == implementation) return new PropertyInfo[] { };
            return implementation.GetFields(researchFlags).Concat(GetFields(implementation.BaseType)).ToArray();
        }

        private IEnumerable<T> GetInjectedMembers<T, U>(Type implementation, GetMembers getMembersMethod) 
            where T : InjectedMemberInfo<U>, new() 
            where U : MemberInfo
            => getMembersMethod(implementation)
                .Select(members => new T
                {
                    MembersInfos = members as U,
                    InjectionAttribute = members.GetCustomAttribute<InjectionAttribute>()
                })
                .Where(x => x.InjectionAttribute != null);
    }
}
