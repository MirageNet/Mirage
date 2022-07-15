using Mono.Cecil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class ConstFieldTracker
    {
        private readonly string fieldName;
        private readonly TypeDefinition type;
        private readonly int max;
        private readonly string errorName;

        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="max">Throws if over this count</param>
        /// <param name="errorName">name of type to put in over max error</param>
        public ConstFieldTracker(string fieldName, TypeDefinition type, int max, string errorName)
        {
            this.fieldName = fieldName;
            this.type = type;
            this.max = max;
            this.errorName = errorName;
        }

        public int GetInBase()
        {
            return type.BaseType.Resolve().GetConst<int>(fieldName);
        }

        public void Set(int countInCurrent)
        {
            var totalSyncVars = GetInBase() + countInCurrent;

            if (totalSyncVars >= max)
            {
                throw new NetworkBehaviourException($"{type.Name} has too many {errorName}. Consider refactoring your class into multiple components", type);
            }
            type.SetConst(fieldName, totalSyncVars);
        }
    }
}
