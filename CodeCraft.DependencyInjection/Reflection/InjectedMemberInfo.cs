using System.Reflection;

namespace CodeCraft.DependencyInjection.Relfection
{ 
    abstract class InjectedMemberInfo<T> where T : MemberInfo
    {
        public T MembersInfos { get; set; }
        public InjectionAttribute InjectionAttribute { get; set; }
    } 
}
