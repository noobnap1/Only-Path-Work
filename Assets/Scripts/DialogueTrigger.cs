using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea] public string dialogueText = "{Dialogue}";
    public string soundName = "{Sound}";

    [Header("Input")]
    public InputActionReference interactActionRef; // Assign Player/Interact here in Inspector

    [Header("References")]
    public DialogueManager dialogueManager; // Drag your DialogueManager here in Inspector

    private bool playerIsNear = false;

    private void OnEnable()
    {
        if (interactActionRef != null)
        {
            interactActionRef.action.Enable();
            interactActionRef.action.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (interactActionRef != null)
        {
            interactActionRef.action.performed -= OnInteract;
            interactActionRef.action.Disable();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerIsNear && dialogueManager != null)
        {
            dialogueManager.ShowDialogue(dialogueText, soundName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = false;
    }
}
