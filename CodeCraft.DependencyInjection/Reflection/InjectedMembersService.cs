using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CodeCraft.DependencyInjection.Relfection
{
    class InjectedMembersService : IInjectedMemberService
    {
        private delegate MemberInfo[] GetMembers(Type implementation);

        public IEnumerable<InjectedFieldInfo> GetInjectedFields(Type implementation)
            => GetInjectedMembers<InjectedFieldInfo, FieldInfo>(implementation, GetFields);

        public IEnumerable<InjectedPropertiesInfo> GetInjectedProperties(Type implementation)
            => GetInjectedMembers<InjectedPropertiesInfo, PropertyInfo>(implementation, GetProperties);

        private MemberInfo[] GetProperties(Type implementation)
            => implementation.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private MemberInfo[] GetFields(Type implementation)
            => implementation.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

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
