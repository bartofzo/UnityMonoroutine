/**
 *
 * Handy collection of coroutines that can be used as monoroutines
 *
 *
 * (C) 2019 - Bart van de Sande / Nonline
 * 
 **/

using System.Collections;
using UnityEngine;

namespace UnityMonoroutine
{
    public static class Monoroutines
    {
        /// <summary>
        /// Starts at the current alpha value of the canvas group and fades to targetAlpha
        /// </summary>
        public static IEnumerator AlphaFade(this CanvasGroup canvasGroup, float targetAlpha, float d = 0.33f)
        {
            float t = 0;
            float startAlpha = canvasGroup.alpha;
            while (t < d)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / d);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
        }
    }
}