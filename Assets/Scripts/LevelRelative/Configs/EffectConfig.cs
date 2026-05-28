using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class EffectConfig : MonoBehaviour
    {
        [Header("Hide after one animation cycle")]
        public bool playOneTime = true;

        [Header("Lifetime Settings(Don't work with playOneTime enabled)")]
        public float duration = 2f;

        [Header("Rotation Settings")]
        public AlignmentMode alignmentMode = AlignmentMode.None;

        private Animator anim;
        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (playOneTime && anim != null)
            {
                StartCoroutine(WaitForAnimation());
            }
            else
            {
                StartCoroutine(ReturnToPoolAfterTime());
            }
        }

        private IEnumerator WaitForAnimation()
        {
            yield return new WaitForEndOfFrame();

            float animLength = anim.GetCurrentAnimatorStateInfo(0).length;

            yield return new WaitForSeconds(animLength);

            PoolManager.Instance.ReturnObject(gameObject);
        }

        private IEnumerator ReturnToPoolAfterTime()
        {
            yield return new WaitForSeconds(duration);

            PoolManager.Instance.ReturnObject(gameObject);
        }
    }
}
