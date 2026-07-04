using UnityEngine;

public static class SoundEffectPlayer
{
    public static void PlaySound(
        AudioClip clip,
        Vector3 position,
        float volume = 1f,
        float pitchRandomness = 0f
    )
    {
        if (clip == null) return;

        GameObject audioObject = new GameObject("One Shot Sound");
        audioObject.transform.position = position;

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;

        if (pitchRandomness > 0f)
        {
            audioSource.pitch = Random.Range(
                1f - pitchRandomness,
                1f + pitchRandomness
            );
        }

        audioSource.Play();

        Object.Destroy(audioObject, clip.length / Mathf.Abs(audioSource.pitch));
    }
}