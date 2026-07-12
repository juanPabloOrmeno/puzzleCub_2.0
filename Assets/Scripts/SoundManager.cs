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

    void Start()
    {
        source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
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

}
