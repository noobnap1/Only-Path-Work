using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea] public string dialogueText = "{Dialogue}";
    public string soundName = "{Sound}";

    private bool playerIsNear = false;

    void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Show(dialogueText, soundName);
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
