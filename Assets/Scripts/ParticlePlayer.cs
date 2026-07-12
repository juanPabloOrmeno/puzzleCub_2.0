using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ParticlePlayer : MonoBehaviour {

    public ParticleSystem[] allParticles;
    public float lifetime = 1f;

   

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        Destroy(gameObject, lifetime);
    }

    public void Play()
    {
        if (allParticles == null || allParticles.Length == 0)
        {
            allParticles = GetComponentsInChildren<ParticleSystem>();
        }

        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop();
            ps.Play();
        }
    }
}
