using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class SyncVarHookTests : TestsBuildFromTestName
    {
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

        static string OldNewMethodFormat(string hookName, string ValueType)
        {
            return string.Format("void {0}({1} oldValue, {1} newValue)", hookName, ValueType);
        }

        [Test]
        public void ErrorWhenNoHookFound()
        {
            HasError($"Could not find hook for 'health', hook name 'onChangeHealth'. Method signature should be {OldNewMethodFormat("onChangeHealth", "System.Int32")}",
                "System.Int32 SyncVarHookTests.ErrorWhenNoHookFound.ErrorWhenNoHookFound::health");
        }

        [Test]
        public void ErrorWhenNoHookWithCorrectParametersFound()
        {
            HasError($"Could not find hook for 'health', hook name 'onChangeHealth'. Method signature should be {OldNewMethodFormat("onChangeHealth", "System.Int32")}",
                "System.Int32 SyncVarHookTests.ErrorWhenNoHookWithCorrectParametersFound.ErrorWhenNoHookWithCorrectParametersFound::health");
        }

        [Test]
        public void ErrorForWrongTypeOldParameter()
        {
            HasError($"Wrong type for Parameter in hook for 'health', hook name 'onChangeHealth'. Method signature should be {OldNewMethodFormat("onChangeHealth", "System.Int32")}",
                "System.Int32 SyncVarHookTests.ErrorForWrongTypeOldParameter.ErrorForWrongTypeOldParameter::health");
        }

        [Test]
        public void ErrorForWrongTypeNewParameter()
        {
            HasError($"Wrong type for Parameter in hook for 'health', hook name 'onChangeHealth'. Method signature should be {OldNewMethodFormat("onChangeHealth", "System.Int32")}",
                "System.Int32 SyncVarHookTests.ErrorForWrongTypeNewParameter.ErrorForWrongTypeNewParameter::health");
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
                "SyncVarHookTests.ErrorWhenHookNotAction.DoStuff SyncVarHookTests.ErrorWhenHookNotAction.ErrorWhenHookNotAction::OnChangeHealth");
        }

        [Test]
        public void ErrorWhenNotGenericAction()
        {
            HasError($"Hook Event for 'health' needs to be type 'System.Action<,>' but was 'System.Action' instead",
                "System.Action SyncVarHookTests.ErrorWhenNotGenericAction.ErrorWhenNotGenericAction::OnChangeHealth");
        }

        [Test]
        public void ErrorWhenEventArgsAreWrong()
        {
            HasError($"Hook Event for 'health' needs to be type 'System.Action<,>' but was 'System.Action`2<System.Int32,System.Single>' instead",
                "System.Action`2<System.Int32,System.Single> SyncVarHookTests.ErrorWhenEventArgsAreWrong.ErrorWhenEventArgsAreWrong::OnChangeHealth");
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
                "System.Int32 SyncVarHookTests.SyncVarHookServerError.SyncVarHookServerError::health");
        }
    }
}
