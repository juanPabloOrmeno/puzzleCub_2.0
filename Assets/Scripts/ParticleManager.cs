using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour {

    public GameObject clearFXPrefab;
    public float clearFXLifetime = 1f;
    public int clearFXSortingOrder = 30;

    public void ClearPieceFXAt(int x, int y, int z = -1)
    {
        if (clearFXPrefab != null)
        {
            Vector3 targetPosition = new Vector3(x, y, z);
            GameObject clearFX = Instantiate(clearFXPrefab, targetPosition, Quaternion.identity) as GameObject;
            ParticleSystem[] particleSystems = clearFX.GetComponentsInChildren<ParticleSystem>();
            CenterEffectOnTarget(clearFX, particleSystems, targetPosition);
            SetParticleSortingOrder(clearFX);

            ParticlePlayer particlePlayer = clearFX.GetComponentInChildren<ParticlePlayer>();

            if (particlePlayer != null)
            {
                particlePlayer.Play();
                return;
            }

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Play(true);
            }

            Destroy(clearFX, clearFXLifetime);
        }
    }

    private void CenterEffectOnTarget(GameObject clearFX, ParticleSystem[] particleSystems, Vector3 targetPosition)
    {
        if (clearFX == null || particleSystems == null || particleSystems.Length == 0)
        {
            return;
        }

        Vector3 center = Vector3.zero;
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            center += particleSystem.transform.position;
        }

        center /= particleSystems.Length;
        clearFX.transform.position += targetPosition - center;
    }

    private void SetParticleSortingOrder(GameObject clearFX)
    {
        ParticleSystemRenderer[] renderers = clearFX.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer particleRenderer in renderers)
        {
            particleRenderer.sortingOrder = clearFXSortingOrder;
        }
    }

    public void BreakTileFXAt(int breakableValue, int x, int y, int z = -1)
    {
        GameObject breakFX = null;
        ParticlePlayer particlePlayer = null;

        if (breakFX != null)
        {
            particlePlayer = breakFX.GetComponent<ParticlePlayer>();

            if (particlePlayer != null)
            {
                particlePlayer.Play();
               
            }
        }
    }
}
