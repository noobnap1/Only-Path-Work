using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Settings")]
    public float letterDelay = 0.05f;
    public float fadeTime = 0.15f;

    private static DialogueManager current;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI dialogueText;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        current = this;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Transform textTransform = transform.Find("DialogueText");
        if (textTransform == null)
        {
            Debug.LogError("DialogueText child not found.");
            return;
        }

        dialogueText = textTransform.GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false); // Start hidden
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

        gameObject.SetActive(true);
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
        dialogueText.text = "";
        gameObject.SetActive(false);
        typingCoroutine = null;
    }

    public void Cancel()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        CleanupDialogue();
    }
}
