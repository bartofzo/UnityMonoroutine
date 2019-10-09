/**
 *
 * Extension class for a Monobehaviour that allows for easy execution a single Coroutine at a time.
 *
 * Usage:
 *
 * Using UnityMonoroutine;
 *
    // Fades in CanvasGroup and then fades it out again
    canvasGroup.alpha = 0;
    this.StartMonoroutine(canvasGroup.AlphaFade(1, 1),
        () => this.StartMonoroutine(canvasGroup.AlphaFade(0, 1),
        () => Debug.Log("Finished!")));
 * 
 * Example of such a routine would be a fade in/fadeout routine, where one can interrupt the other.
 *
 * Provides an optional callback that is garantueed to be called when the coroutine stops (either by finishing execution or by interruption)
 * 
 * (C) 2019 - Bart van de Sande / Nonline
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityMonoroutine
{
    public static class Monoroutine
    {
        private static Dictionary<int, Tuple<Coroutine, Action>> routines = new Dictionary<int, Tuple<Coroutine, Action>>();

        /// <summary>
        /// Starts a Monoroutine on this Monobehaviour. Only a single Monoroutine will run at a given time for a Monobehaviour.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">The coroutine to start</param>
        /// <param name="callback">Optional callback to invoke when coroutine has finished executing or is interrupted</param>
        public static void StartMonoroutine(this MonoBehaviour monoBehaviour, IEnumerator coroutine, Action callback = null)
        {
            int instanceId = monoBehaviour.GetInstanceID();

            if (routines.TryGetValue(instanceId, out var runningRoutine))
            {
                // Remove from routines to prevent infinite loop when the callback invokes a new monoroutine
                // on this monobehaviour
                routines.Remove(instanceId);

                // Stop already running 'mono' routine
                monoBehaviour.StopCoroutine(runningRoutine.Item1);

                // When stopped inbetween, fire stored callback
                runningRoutine.Item2?.Invoke();
            }

            if (!monoBehaviour.gameObject.activeInHierarchy ||
                !monoBehaviour.gameObject.activeSelf)
            {
                // Catch Coroutine couldn't be started...
                Debug.Log("Can't start monoroutine because monobehaviour is disabled");
                callback?.Invoke();
                return;
            }

            routines[instanceId] = new Tuple<Coroutine, Action>(monoBehaviour.StartCoroutine(WrapMonoroutine(coroutine, () =>
            {
                // Callback for when coroutine has fully completed
                routines.Remove(instanceId);
                callback?.Invoke();

            })), callback);
        }

        /// <summary>
        /// Stops the currently running Monoroutine for this Monobehaviour. 
        /// </summary>
        public static void StopMonoroutine(this MonoBehaviour monoBehaviour)
        {
            int instanceId = monoBehaviour.GetInstanceID();

            if (routines.TryGetValue(instanceId, out var runningRoutine))
            {
                // Stop running 'mono' routine
                monoBehaviour.StopCoroutine(runningRoutine.Item1);

                // Stopped inbetween, fire stored callback
                runningRoutine.Item2?.Invoke();

                // Remove from collection
                routines.Remove(instanceId);
            }
        }

        private static IEnumerator WrapMonoroutine(IEnumerator coroutine, Action callback)
        {
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }
            callback?.Invoke();
        }
    }
}