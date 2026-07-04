using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;

    public AudioClip BGMusic;
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (BGMusic != null && audioSource != null)
        {
            audioSource.clip = BGMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
