using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Settings")]
    public float letterDelay = 0.05f;
    public float fadeTime = 0.15f;

    [Header("Dialogue Box Setup")]
    public GameObject dialogueBoxPrefab; // Assign prefab via Inspector

    private static DialogueManager current;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI dialogueText;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;
    private GameObject dialogueBoxInstance;

    private void Awake()
    {
        current = this;

        // Instantiate and set up the DialogueBox
        if (dialogueBoxPrefab == null)
        {
            Debug.LogError("Dialogue Box Prefab is not assigned in the inspector.");
            return;
        }

        dialogueBoxInstance = Instantiate(dialogueBoxPrefab, transform);
        dialogueBoxInstance.SetActive(false);

        canvasGroup = dialogueBoxInstance.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dialogueBoxInstance.AddComponent<CanvasGroup>();

        Transform textTransform = dialogueBoxInstance.transform.Find("DialogueText");
        if (textTransform == null)
        {
            Debug.LogError("DialogueText child not found in dialogue box prefab.");
            return;
        }

        dialogueText = textTransform.GetComponent<TextMeshProUGUI>();
        audioSource = dialogueBoxInstance.GetComponent<AudioSource>() ?? dialogueBoxInstance.AddComponent<AudioSource>();

        canvasGroup.alpha = 0f;
    }

    public static void Show(string text, string soundName)
    {
        if (current == null)
        {
            Debug.LogWarning("DialogueManager not found in scene.");
            return;
        }

        current.StartDialogue(text, soundName);
    }

    public void StartDialogue(string dialogue, string soundName)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            CleanupDialogue();
        }

        AudioClip typeSound = Resources.Load<AudioClip>($"SpeechSounds/{soundName}");
        if (typeSound == null)
            Debug.LogWarning($"Sound '{soundName}' not found in Resources/SpeechSounds/");

        dialogueBoxInstance.SetActive(true);
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue, typeSound));
    }

    private IEnumerator TypeDialogue(string sentence, AudioClip typeSound)
    {
        canvasGroup.alpha = 0f;
        dialogueText.text = "";

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        float lastSoundTime = -1f;
        float soundCooldown = 0.05f;

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;

            if (letter != ' ' && typeSound != null && Time.time - lastSoundTime > soundCooldown)
            {
                audioSource.PlayOneShot(typeSound);
                lastSoundTime = Time.time;
            }

            yield return new WaitForSeconds(letterDelay);
        }

        yield return new WaitForSeconds(0.5f);

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            yield return null;
        }

        CleanupDialogue();
    }

    private void CleanupDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueBoxInstance != null)
            dialogueBoxInstance.SetActive(false);

        typingCoroutine = null;
    }

    public void Cancel()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        CleanupDialogue();
    }
}
