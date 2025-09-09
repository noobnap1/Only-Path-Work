using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    [Header("Settings")]
    public float letterDelay = 0.05f;
    public float fadeTime = 0.15f;

    [Header("References")]
    public TextMeshProUGUI dialogueText; // Drag your TMP text here

    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();

        if (dialogueText == null)
            Debug.LogError("DialogueText not assigned in Inspector!");

        canvasGroup.alpha = 0f;
        gameObject.SetActive(true); // keep active, just invisible
    }

    public void ShowDialogue(string text, string soundName = "")
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            CleanupDialogue();
        }

        AudioClip typeSound = null;
        if (!string.IsNullOrEmpty(soundName))
        {
            typeSound = Resources.Load<AudioClip>($"SpeechSounds/{soundName}");
            if (typeSound == null)
                Debug.LogWarning($"Sound '{soundName}' not found in Resources/SpeechSounds/");
        }

        typingCoroutine = StartCoroutine(TypeDialogue(text, typeSound));
    }

    private IEnumerator TypeDialogue(string sentence, AudioClip typeSound)
    {
        dialogueText.text = "";

        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f));

        float lastSoundTime = 0f;
        float soundCooldown = 0.05f;

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;

            if (letter != ' ' && typeSound != null && Time.time - lastSoundTime >= soundCooldown)
            {
                audioSource.PlayOneShot(typeSound);
                lastSoundTime = Time.time;
            }

            yield return new WaitForSeconds(letterDelay);
        }

        yield return new WaitForSeconds(0.5f);

        // Fade out
        yield return StartCoroutine(FadeCanvas(1f, 0f));

        CleanupDialogue();
    }

    private IEnumerator FadeCanvas(float start, float end)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = end;
    }

    private void CleanupDialogue()
    {
        dialogueText.text = "";
        canvasGroup.alpha = 0f;
        typingCoroutine = null;
    }

    public void Cancel()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        CleanupDialogue();
    }
}
