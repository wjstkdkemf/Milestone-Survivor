
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionRange = 2f;
    public LayerMask interactableLayer;
    private Interactable currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        //Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer, QueryTriggerInteraction.Collide);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        if (colliders.Length > 0)
        {
            currentInteractable = colliders[0].GetComponent<Interactable>();
            if (currentInteractable != null)
            {
                // Show interaction prompt (e.g., "Press E to talk")
                // This can be handled by a UI manager
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                // Hide interaction prompt
            }
            currentInteractable = null;
        }
    }
}
