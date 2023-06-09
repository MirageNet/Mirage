using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class SyncVarHookTests : WeaverTestBase
    {
        private string TypeName()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var ClassName = nameof(SyncVarHookTests);
            // standard format for test name
            return $"{ClassName}.{name}.{name}";
        }


        [Test, BatchSafe(BatchType.Success)]
        public void FindsPrivateHook()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsPublicHook()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsStaticHook()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsHookWithNetworkIdentity()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsHookWithGameObject()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsHookWithOtherOverloadsInOrder()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsHookWithOtherOverloadsInReverseOrder()
        {
            IsSuccess();
        }

        [Test]
        public void ErrorWhenNoHookFound()
        {
            HasError($"Could not find hook for 'health', hook name 'onChangeHealth', hook type Automatic. See SyncHookType for valid signatures",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ErrorWhenNoHookWithCorrectParametersFound()
        {
            HasError($"Could not find hook for 'health', hook name 'onChangeHealth', hook type Automatic. See SyncHookType for valid signatures",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ErrorForWrongTypeOldParameter()
        {
            HasError($"Wrong type for Parameter in hook for 'health', hook name 'onChangeHealth'.",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ErrorForWrongTypeNewParameter()
        {
            HasError($"Wrong type for Parameter in hook for 'health', hook name 'onChangeHealth'.",
                $"System.Int32 {TypeName()}::health");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void FindsHookEvent()
        {
            IsSuccess();
        }

        [Test]
        public void ErrorWhenHookNotAction()
        {
            HasError("Hook Event for 'health' is invalid 'SyncVarHookTests.ErrorWhenHookNotAction.DoStuff', Error Type: Not System.Action",
                $"SyncVarHookTests.ErrorWhenHookNotAction.DoStuff {TypeName()}::OnChangeHealth");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SuccessGenericAction()
        {
            IsSuccess();
        }

        [Test]
        public void ErrorWhenEventArgsAreWrong()
        {
            HasError("Hook Event for 'health' is invalid 'System.Action`2<System.Int32,System.Single>', Error Type: Param mismatch",
                $"System.Action`2<System.Int32,System.Single> {TypeName()}::OnChangeHealth");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void SyncVarHookServer()
        {
            IsSuccess();
        }

        [Test]
        public void SyncVarHookServerError()
        {
            HasError($"'invokeHookOnServer' is set to true but no hook was implemented. Please implement hook or set 'invokeHookOnServer' back to false or remove for default false.",
                $"System.Int32 {TypeName()}::health");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void AutomaticHookMethod1()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void AutomaticHookMethod2()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void AutomaticHookEvent1()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void AutomaticHookEvent2()
        {
            IsSuccess();
        }

        [Test]
        public void AutomaticNotFound()
        {
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type Automatic. See SyncHookType for valid signatures",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void AutomaticFound2Methods()
        {
            HasError("Mutliple hooks found for 'health', hook name 'onChangeHealth'. Please set HookType or remove one of the overloads",
                $"System.Int32 {TypeName()}::health");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitEvent1Found()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitEvent2Found()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitMethod1Found()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitMethod2Found()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitMethod1FoundWithOverLoad()
        {
            IsSuccess();
        }

        [Test, BatchSafe(BatchType.Success)]
        public void ExplicitMethod2FoundWithOverLoad()
        {
            IsSuccess();
        }

        [Test]
        public void ExplicitMethod1NotFound()
        {
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type MethodWith1Arg. See SyncHookType for valid signatures",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ExplicitMethod2NotFound()
        {
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type MethodWith2Arg. See SyncHookType for valid signatures",
                $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ExplicitEvent1NotFound()
        {
            HasError("Hook Event for 'health' is invalid 'System.Action`2<System.Int32,System.Int32>', Error Type: Arg mismatch",
                 $"System.Action`2<System.Int32,System.Int32> {TypeName()}::onChangeHealth");
        }

        [Test]
        public void ExplicitEvent2NotFound()
        {
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type EventWith2Arg. See SyncHookType for valid signatures",
                 $"System.Int32 {TypeName()}::health");
        }
    }
}
