using UnityEngine;

namespace Mirage.Components
{
    public abstract class MirageBaseComponent<T> : MonoBehaviour, IComponentDesigner<T> where T : new()
    {
        /// <summary>
        ///     Convenient access to the type of component this component references. 
        /// </summary>
        protected internal T ComponentType;

        /// <summary>
        ///     Make sure to call base to this awake otherwise you need to manually call
        ///     see <see cref="IComponentDesigner{T}.CreateComponentInstance"/> to allow correct
        ///     initialization of these components.
        /// </summary>
        protected virtual void Awake()
        {
            CreateComponentInstance();
        }

        #region Implementation of IComponentDesigner<out T>

        public T CreateComponentInstance()
        {
            ComponentType = new T();

            return ComponentType;
        }

        #endregion
    }
}
