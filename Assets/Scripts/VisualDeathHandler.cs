using UnityEngine;

public class VisualDeathHandler : MonoBehaviour
{
    public void HandleDeathVisuals()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr != null)
            {
                sr.color = Color.black;
            }
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    public void ResetVisuals()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in colliders)
        {
            col.enabled = true; 
        }
    }
}