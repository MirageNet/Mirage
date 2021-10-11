using UnityEngine;

namespace Mirage.HeadlessBenchmark
{
    public class NetworkColor : NetworkBehaviour
    {
        [SerializeField] Color server = Color.blue;
        [SerializeField] Color client = Color.red;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(() => SetColor(server));
            Identity.OnStartClient.AddListener(() => SetColor(client));
        }

        private void SetColor(Color color)
        {
            color.a = .7f;

            Renderer renderer = GetComponent<Renderer>();
            if (renderer is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.color = color;
            }
            else
            {
                Material material = renderer.material;
                material.color = color;
            }
        }
    }
}
