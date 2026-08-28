using UnityEngine;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    public class PhysicsSimulator : NetworkBehaviour
    {
        private PhysicsScene physicsScene;
        private PhysicsScene2D physicsScene2D;
        private bool simulatePhysicsScene;
        private bool simulatePhysicsScene2D;

        private void Awake()
        {
            // IsServer is not set until the scene object is spawned, which happens after all sub scenes are loaded.
            // So set up in OnStartServer instead of Start
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            physicsScene = gameObject.scene.GetPhysicsScene();
            simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;

            physicsScene2D = gameObject.scene.GetPhysicsScene2D();
            simulatePhysicsScene2D = physicsScene2D.IsValid() && physicsScene2D != Physics2D.defaultPhysicsScene;
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            if (simulatePhysicsScene)
                physicsScene.Simulate(Time.fixedDeltaTime);

            if (simulatePhysicsScene2D)
                physicsScene2D.Simulate(Time.fixedDeltaTime);
        }
    }
}
