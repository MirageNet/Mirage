using System.Collections.Generic;
using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Examples.Additive
{
    // This script demonstrates the NetworkAnimator and how to leverage
    // the built-in observers system to track players.
    // Note that all ProximityCheckers should be restricted to the Player layer.
    public class ShootingTankBehaviour : NetworkBehaviour
    {
        [SyncVar]
        public Quaternion rotation;

        NetworkAnimator networkAnimator;

        [Server(error = false)]
        void Start()
        {
            networkAnimator = GetComponent<NetworkAnimator>();
        }

        [Range(0, 1)]
        public float turnSpeed = 0.1f;

        void Update()
        {
            if (IsServer && Identity.ServerObjectManager.InterestManager.ObserverSystems.Count > 0)
                ShootNearestPlayer();

            if (IsClient)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed);
        }

        [Server]
        void ShootNearestPlayer()
        {
            GameObject target = null;
            float distance = 100f;

            foreach (ObserverData observerData in Identity.ServerObjectManager.InterestManager.ObserverSystems)
            {
                foreach (KeyValuePair<NetworkIdentity, INetworkPlayer> players in observerData.Observers)
                {
                    GameObject tempTarget = players.Value.Identity.gameObject;
                    float tempDistance = Vector3.Distance(tempTarget.transform.position, transform.position);

                    if (target == null || distance > tempDistance)
                    {
                        target = tempTarget;
                        distance = tempDistance;
                    }
                }
            }

            if (target != null)
            {
                transform.LookAt(target.transform.position + Vector3.down);
                rotation = transform.rotation;
                networkAnimator.SetTrigger("Fire");
            }
        }
    }
}
