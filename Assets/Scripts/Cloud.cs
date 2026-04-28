using UnityEngine;

namespace Assets.Scripts
{
    public class Cloud : MonoBehaviour
    {
        private float speed;
        private float despawnX;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Setup(float cloudSpeed, float despawnPosition, Sprite visual)
        {
            speed = cloudSpeed;
            despawnX = despawnPosition;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = visual;
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            if (transform.position.x <= despawnX)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
