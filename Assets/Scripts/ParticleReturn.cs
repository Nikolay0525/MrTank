using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleReturn : MonoBehaviour
    {
        private ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (ps != null && !ps.IsAlive(true))
            {
                if (DeathEffectPoolManager.Instance != null)
                {
                    DeathEffectPoolManager.Instance.ReturnDeathEffect(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
