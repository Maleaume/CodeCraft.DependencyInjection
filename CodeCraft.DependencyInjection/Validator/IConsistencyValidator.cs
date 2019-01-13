namespace CodeCraft.DependencyInjection.Validator
{
    interface IConsistencyValidator
    {
        bool CheckConsistency<Interface, Implementation>()
            where Interface : class
            where Implementation : class;
    }
}
