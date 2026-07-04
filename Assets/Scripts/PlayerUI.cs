using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [SerializeField] private bool startTimerOnAwake = true;

    private float elapsedTime;
    private bool timerRunning;

    private void Start()
    {
        FindPlayerHealthIfMissing();

        if (startTimerOnAwake)
        {
            StartTimer();
        }

        UpdateHealthText();
        UpdateTimerText();
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            FindPlayerHealthIfMissing();
        }

        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
        }

        UpdateHealthText();
        UpdateTimerText();
    }

    private void FindPlayerHealthIfMissing()
    {
        if (playerHealth != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null) return;

        playerHealth = playerObject.GetComponent<Health>();
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;

        if (playerHealth == null)
        {
            healthText.text = "HP: -- / --";
            return;
        }

        healthText.text = "HP: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth;
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerText();
    }
}