using System;
using System.Collections.Generic;

namespace CodeCraft.DependencyInjection.Relfection
{
    interface IInjectedMemberService
    {
        IEnumerable<InjectedFieldInfo> GetInjectedFields(Type implementation);
        IEnumerable<InjectedPropertiesInfo> GetInjectedProperties(Type implementation);
    }
}
