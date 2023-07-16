using System;
using Mirage.CodeGen;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal abstract class BaseMethodHelper
    {
        public abstract string MethodName { get; }

        protected readonly ModuleDefinition _module;
        protected readonly TypeDefinition _typeDefinition;

        public ILProcessor Worker { get; private set; }
        public MethodDefinition Method { get; private set; }

        public BaseMethodHelper(ModuleDefinition module, TypeDefinition typeDefinition)
        {
            _module = module;
            _typeDefinition = typeDefinition;
        }

        /// <summary>
        /// Adds method to current type
        /// </summary>
        /// <returns></returns>
        public void AddMethod()
        {
            Method = _typeDefinition.AddMethod(MethodName,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    ReturnValue);

            AddParameters();
            Method.Body.InitLocals = true;
            Worker = Method.Body.GetILProcessor();

            AddLocals();
            WriteBaseCall();
        }

        protected virtual Type ReturnValue => typeof(void);
        protected abstract void AddParameters();
        protected abstract void AddLocals();

        protected virtual void WriteBaseCall()
        {
            var baseMethod = _typeDefinition.BaseType.GetMethodInBaseType(MethodName);
            if (baseMethod == null)
                return;

            // load base.
            Worker.Append(Worker.Create(OpCodes.Ldarg_0));
            // load args
            foreach (var param in Method.Parameters)
            {
                Worker.Append(Worker.Create(OpCodes.Ldarg, param));
            }
            // call base method
            Worker.Append(Worker.Create(OpCodes.Call, _module.ImportReference(baseMethod)));
        }

        public MethodDefinition GetManualOverride()
        {
            return _typeDefinition.GetMethod(MethodName);
        }
        public bool HasManualOverride()
        {
            return _typeDefinition.GetMethod(MethodName) != null;
        }
    }
}
