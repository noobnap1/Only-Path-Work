using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public string dialogueBoxPrefabName = "DialogueBox"; // prefab name in Resources
    public float letterDelay = 0.05f;
    public float fadeTime = 0.15f;

    private static DialogueManager current;

    private GameObject dialogueBox;
    private TextMeshProUGUI dialogueText;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (current == null)
            current = this;
        else if (current != this)
            Destroy(gameObject); // enforce singleton
    }

    // Static entry point - call from anywhere
    public static void Show(string text, string soundName)
    {
        if (current != null)
            current.StartDialogue(text, soundName);
        else
            Debug.LogWarning("DialogueManager instance not found in scene.");
    }

    public void StartDialogue(string dialogue, string soundName)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            CleanupDialogueBox();
        }

        AudioClip typeSound = Resources.Load<AudioClip>($"SpeechSounds/{soundName}");
        if (typeSound == null)
            Debug.LogWarning($"Sound '{soundName}' not found in Resources/SpeechSounds/");

        ShowDialogue(dialogue, typeSound);
    }

    private void ShowDialogue(string dialogue, AudioClip typeSound)
    {
        // Instantiate DialogueBox prefab from Resources every time
        GameObject prefab = Resources.Load<GameObject>(dialogueBoxPrefabName);
        if (prefab == null)
        {
            Debug.LogError($"DialogueBox prefab '{dialogueBoxPrefabName}' not found in Resources.");
            return;
        }

        dialogueBox = Instantiate(prefab);
        dialogueBox.name = "DialogueBox (Runtime)";

        Transform textTransform = dialogueBox.transform.Find("DialogueText");
        if (textTransform == null)
        {
            Debug.LogError("DialogueText not found in DialogueBox prefab.");
            return;
        }

        dialogueText = textTransform.GetComponent<TextMeshProUGUI>();
        audioSource = dialogueBox.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = dialogueBox.AddComponent<AudioSource>();

        if (dialogueBox.GetComponent<CanvasGroup>() == null)
            dialogueBox.AddComponent<CanvasGroup>();

        typingCoroutine = StartCoroutine(TypeDialogue(dialogue, typeSound));
    }

    private IEnumerator TypeDialogue(string sentence, AudioClip typeSound)
    {
        CanvasGroup cg = dialogueBox.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        dialogueText.text = "";

        // Fade In
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }
        cg.alpha = 1f;

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

        yield return new WaitForSeconds(2f);

        // Fade Out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            yield return null;
        }

        CleanupDialogueBox();
    }

    private void CleanupDialogueBox()
    {
        if (dialogueBox != null)
            Destroy(dialogueBox);

        dialogueBox = null;
        dialogueText = null;
        audioSource = null;
        typingCoroutine = null;
    }

    public void Cancel()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        CleanupDialogueBox();
    }
}
