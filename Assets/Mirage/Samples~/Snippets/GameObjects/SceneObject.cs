using System.Collections;
using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: scene-object-class
    public class SceneObject : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeEquipment))]
        public EquippedItem equippedItem { get; set; }

        public GameObject ballPrefab;
        public GameObject boxPrefab;
        public GameObject cylinderPrefab;

        private void OnChangeEquipment(EquippedItem oldEquippedItem, EquippedItem newEquippedItem)
        {
            StartCoroutine(ChangeEquipment(newEquippedItem));
        }

        // Since Destroy is delayed to the end of the current frame, we use a coroutine
        // to clear out any child objects before instantiating the new one
        private IEnumerator ChangeEquipment(EquippedItem newEquippedItem)
        {
            while (transform.childCount > 0)
            {
                Destroy(transform.GetChild(0).gameObject);
                yield return null;
            }

            // Use the new value, not the SyncVar property value
            SetEquippedItem(newEquippedItem);
        }

        // SetEquippedItem is called on the client from OnChangeEquipment (above),
        // and on the server from CmdDropItem in the PlayerEquip script.
        public void SetEquippedItem(EquippedItem newEquippedItem)
        {
            switch (newEquippedItem)
            {
                case EquippedItem.ball:
                    Instantiate(ballPrefab, transform);
                    break;
                case EquippedItem.box:
                    Instantiate(boxPrefab, transform);
                    break;
                case EquippedItem.cylinder:
                    Instantiate(cylinderPrefab, transform);
                    break;
            }
        }
    }
    // CodeEmbed-End: scene-object-class

    public class SceneObjectClick : NetworkBehaviour
    {
        // CodeEmbed-Start: scene-object-mousedown
        private void OnMouseDown()
        {
            Client.Player.Identity.GetComponent<PlayerEquip>().CmdPickupItem(gameObject);
        }
        // CodeEmbed-End: scene-object-mousedown
    }
}
