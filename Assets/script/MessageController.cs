using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    public Text messageText; // Assign in the Inspector
    public GameObject messagePanel; // Assign in the Inspector

    void Start()
    {
        // Initially hide the message panel
        messagePanel.SetActive(false);
    }

    public void ShowMessage(string message, float duration)
    {
        StartCoroutine(DisplayMessage(message, duration));
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        messageText.text = message;
        messagePanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        messagePanel.SetActive(false);
    }
}