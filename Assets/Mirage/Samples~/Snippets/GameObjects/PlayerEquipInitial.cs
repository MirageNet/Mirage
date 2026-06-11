using System.Collections;
using UnityEngine;

namespace Mirage.Snippets.GameObjects.Initial
{
    // CodeEmbed-Start: player-equip-initial
    public enum EquippedItem : byte
    {
        nothing,
        ball,
        box,
        cylinder
    }

    public class PlayerEquip : NetworkBehaviour
    {
        public GameObject sceneObjectPrefab;

        public GameObject rightHand;

        public GameObject ballPrefab;
        public GameObject boxPrefab;
        public GameObject cylinderPrefab;

        [SyncVar(hook = nameof(OnChangeEquipment))]
        public EquippedItem equippedItem { get; set; }

        private void OnChangeEquipment(EquippedItem oldEquippedItem, EquippedItem newEquippedItem)
        {
            StartCoroutine(ChangeEquipment(newEquippedItem));
        }

        // Since Destroy is delayed to the end of the current frame, we use a coroutine
        // to clear out any child objects before instantiating the new one
        private IEnumerator ChangeEquipment(EquippedItem newEquippedItem)
        {
            while (rightHand.transform.childCount > 0)
            {
                Destroy(rightHand.transform.GetChild(0).gameObject);
                yield return null;
            }

            switch (newEquippedItem)
            {
                case EquippedItem.ball:
                    Instantiate(ballPrefab, rightHand.transform);
                    break;
                case EquippedItem.box:
                    Instantiate(boxPrefab, rightHand.transform);
                    break;
                case EquippedItem.cylinder:
                    Instantiate(cylinderPrefab, rightHand.transform);
                    break;
            }
        }

        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha0) && equippedItem != EquippedItem.nothing)
                CmdChangeEquippedItem(EquippedItem.nothing);
            if (Input.GetKeyDown(KeyCode.Alpha1) && equippedItem != EquippedItem.ball)
                CmdChangeEquippedItem(EquippedItem.ball);
            if (Input.GetKeyDown(KeyCode.Alpha2) && equippedItem != EquippedItem.box)
                CmdChangeEquippedItem(EquippedItem.box);
            if (Input.GetKeyDown(KeyCode.Alpha3) && equippedItem != EquippedItem.cylinder)
                CmdChangeEquippedItem(EquippedItem.cylinder);
        }

        [ServerRpc]
        private void CmdChangeEquippedItem(EquippedItem selectedItem)
        {
            equippedItem = selectedItem;
        }
    }
    // CodeEmbed-End: player-equip-initial
}
