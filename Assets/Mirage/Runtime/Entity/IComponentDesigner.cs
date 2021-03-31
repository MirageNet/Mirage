namespace Mirage.Components
{
    public interface IComponentDesigner<out T>
    {
        /// <summary>
        ///     Create new instance of specific component to allow usage of
        ///     base class files of mirage to be more generic and agnostic free.
        /// </summary>
        /// <returns></returns>
        public T CreateComponentInstance();
    }
}
