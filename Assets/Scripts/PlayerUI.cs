using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;

    [Header("Health Bar")]
    [SerializeField] private Image healthFillImage;

    [Header("Timer Text")]
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

        UpdateHealthBar();
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

        UpdateHealthBar();
        UpdateTimerText();
    }

    private void FindPlayerHealthIfMissing()
    {
        if (playerHealth != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null) return;

        playerHealth = playerObject.GetComponent<Health>();
    }

    private void UpdateHealthBar()
    {
        if (healthFillImage == null) return;

        if (playerHealth == null)
        {
            healthFillImage.fillAmount = 0f;
            return;
        }

        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(healthPercent);
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