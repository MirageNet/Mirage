using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class SyncVarHookTests : TestsBuildFromTestName
    {
        private string TypeName()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var ClassName = nameof(SyncVarHookTests);
            // standard format for test name
            return $"{ClassName}.{name}.{name}";
        }


        [Test]
        public void FindsPrivateHook()
        {
            IsSuccess();
        }

        [Test]
        public void FindsPublicHook()
        {
            IsSuccess();
        }

        [Test]
        public void FindsStaticHook()
        {
            IsSuccess();
        }

        [Test]
        public void FindsHookWithNetworkIdentity()
        {
            IsSuccess();
        }

        [Test]
        public void FindsHookWithGameObject()
        {
            IsSuccess();
        }

        [Test]
        public void FindsHookWithOtherOverloadsInOrder()
        {
            IsSuccess();
        }

        [Test]
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

        [Test]
        public void FindsHookEvent()
        {
            IsSuccess();
        }

        [Test]
        public void ErrorWhenHookNotAction()
        {
            HasError($"Hook Event for 'health' needs to be type 'System.Action<,>' but was 'SyncVarHookTests.ErrorWhenHookNotAction.DoStuff' instead",
                $"SyncVarHookTests.ErrorWhenHookNotAction.DoStuff {TypeName()}::OnChangeHealth");
        }

        [Test]
        public void ErrorWhenNotGenericAction()
        {
            HasError($"Hook Event for 'health' needs to be type 'System.Action<,>' but was 'System.Action' instead",
                $"System.Action {TypeName()}::OnChangeHealth");
        }

        [Test]
        public void ErrorWhenEventArgsAreWrong()
        {
            HasError($"Hook Event for 'health' needs to be type 'System.Action<,>' but was 'System.Action`2<System.Int32,System.Single>' instead",
                $"System.Action`2<System.Int32,System.Single> {TypeName()}::OnChangeHealth");
        }

        [Test]
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

        [Test]
        public void AutomaticHookMethod1()
        {
            IsSuccess();
        }

        [Test]
        public void AutomaticHookMethod2()
        {
            IsSuccess();
        }

        [Test]
        public void AutomaticHookEvent1()
        {
            IsSuccess();
        }

        [Test]
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

        [Test]
        public void ExplicitEvent1Found()
        {
            IsSuccess();
        }

        [Test]
        public void ExplicitEvent2Found()
        {
            IsSuccess();
        }

        [Test]
        public void ExplicitMethod1Found()
        {
            IsSuccess();
        }

        [Test]
        public void ExplicitMethod2Found()
        {
            IsSuccess();
        }

        [Test]
        public void ExplicitMethod1FoundWithOverLoad()
        {
            IsSuccess();
        }

        [Test]
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
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type EventWith1Arg. See SyncHookType for valid signatures",
                 $"System.Int32 {TypeName()}::health");
        }

        [Test]
        public void ExplicitEvent2NotFound()
        {
            HasError("Could not find hook for 'health', hook name 'onChangeHealth', hook type EventWith2Arg. See SyncHookType for valid signatures",
                 $"System.Int32 {TypeName()}::health");
        }
    }
}
