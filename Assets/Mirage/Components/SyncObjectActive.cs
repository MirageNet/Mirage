namespace Mirage.Components
{
    public class SyncObjectActive : NetworkBehaviour
    {
        // todo update this to work with "invoke on sender" when syncvar has that setting
        [SyncVar(hook = nameof(ActiveChanged), invokeHookOnServer = true)]
        private bool _active;

        private void ActiveChanged(bool nowActive)
        {
            if (gameObject.activeSelf != nowActive)
                gameObject.SetActive(nowActive);
        }

        private void OnEnable()
        {
            _active = true;
        }

        private void OnDisable()
        {
            _active = false;
        }
    }
}
