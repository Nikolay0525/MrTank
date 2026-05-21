using UnityEngine;

namespace Assets.Scripts
{
    public class RandomVisualVariant : MonoBehaviour
    {
        [Header("Visual Variants")]
        [Tooltip("Assign child GameObjects here. Only one will be activated on spawn.")]
        public GameObject[] variants;

        private void OnEnable()
        {
            if (variants == null || variants.Length == 0) return;

            foreach (GameObject variant in variants)
            {
                if (variant != null)
                {
                    variant.SetActive(false);
                }
            }

            int randomIndex = Random.Range(0, variants.Length);

            if (variants[randomIndex] != null)
            {
                variants[randomIndex].SetActive(true);
            }
        }
    }
}
