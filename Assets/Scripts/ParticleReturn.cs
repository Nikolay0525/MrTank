using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class ParticleReturn : MonoBehaviour
    {
        private ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            // Disable the game object when the particle system stops playing
            if (ps != null && !ps.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }
    }

}
