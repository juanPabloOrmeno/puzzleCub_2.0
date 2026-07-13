using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicClips;
    [Range(0, 1)]
    public float musicVolume = 0.5f;
    [Range(0, 1)]
    public float fxVolume = 1.0f;
    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;
    AudioSource source;

    public AudioClip[] winSound;
    public AudioClip[] loseSound;
    public AudioClip[] gambareSound;
    public AudioClip[] pieceSound;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source ??= gameObject.AddComponent<AudioSource>();
        PlayRandomMusic();
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX" + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;
            source.loop = false;
            source.spatialBlend = 0f;

            source.volume = volume;
            source.Play();
            

            Destroy(go, clip.length);
            return source;
        }

        return null;
    }

    public AudioSource PlayClipAtPointR(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX" + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;
            source.loop = true;
            source.spatialBlend = 0f;

            source.volume = volume;
            source.Play();

            return source;
        }
        return null;
    }


    public void stopMusic() {
        if (source != null)
        {
            source.Pause();
        }
    }

    public void playMusic()
    {
        if (source != null)
        {
            source.Play();
        }
    }


    public void stopSound(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource.gameObject);
        }
    }



    public AudioSource PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips != null)
        {
            if (clips.Length != 0)
            {
                int randomIndex = Random.Range(0, clips.Length);

                if (clips[randomIndex] != null)
                {
                    
                    source = PlayClipAtPointR(clips[randomIndex], position, volume);
                    return source;
                }
            }
        }
        return null;
    }

    

    public void PlayRandomMusic()
    {
        PlayRandom(musicClips, Vector3.zero, musicVolume);

    }


    public void PlaySoundWinner()
    {
        AudioClip clip = GetRandomClip(winSound);
        if (clip == null)
        {
            Debug.LogWarning("No hay sonido de victoria asignado en SoundManager.winSound.");
            return;
        }

        PlayClipAtPoint(clip, Vector3.zero, fxVolume);
    }

    public void PlaySoundLoser()
    {
        AudioClip clip = GetRandomClip(loseSound);
        if (clip == null)
        {
            Debug.LogWarning("No hay sonido de derrota asignado en SoundManager.loseSound.");
            return;
        }

        PlayClipAtPoint(clip, Vector3.zero, fxVolume);
    }

    public void PlaySoundGambare()
    {
        AudioClip clip = GetRandomClip(gambareSound);
        if (clip == null)
        {
            Debug.LogWarning("No hay sonido de gambare asignado en SoundManager.gambareSound.");
            return;
        }

        PlayClipAtPoint(clip, Vector3.zero, fxVolume);
    }

    public void PlaySoundPiece()
    {
        AudioClip clip = GetRandomClip(pieceSound);
        if (clip == null)
        {
            Debug.LogWarning("No hay sonido de pieza asignado en SoundManager.pieceSound.");
            return;
        }

        PlayClipAtPoint(clip, Vector3.zero, fxVolume);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }

}
