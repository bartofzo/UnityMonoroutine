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
            RemoveRunningAndFire(monoBehaviour);


            if (monoBehaviour.gameObject == null ||
                !monoBehaviour.gameObject.activeInHierarchy ||
                !monoBehaviour.gameObject.activeSelf)
            {
                // Catch Coroutine couldn't be started...
#if UNITY_EDITOR
                //Debug.Log("Can't start monoroutine because monobehaviour is disabled");
#endif
                // Invoke callback yes or no ?
                // yes for now
                callback?.Invoke();
                return;
            }



            var routine = new Tuple<Coroutine, Action>(monoBehaviour.StartCoroutine(WrapMonoroutine(coroutine, () =>
            {

                // When Coroutine fully finishes we end up here
                // Only fire callback when it is still present in the collection, which means it hasn't been fired yet

                if (routines.Remove(instanceId))
                    callback?.Invoke();


            })), callback); // insert the same callback also in the collection so we can invoke the callback when the routine gets interrupted


            if (routine.Item1 != null)
            {

                // apparently while I dont fully understand yet there's a chance a previous callback hasn't been removed/fired
                RemoveRunningAndFire(monoBehaviour);
  

                // We add the coroutine to the collection only if it is actually running (did not yield break imm)
                routines[instanceId] = routine;

            }
            else
            {
                // Coroutine yielded break immediately
                // we don't even add it to the collection and fire possible callback
                routine.Item2?.Invoke();
            }
        }

        private static void RemoveRunningAndFire(MonoBehaviour monoBehaviour)
        {
            int instanceId = monoBehaviour.GetInstanceID();
            if (routines.TryGetValue(instanceId, out var runningRoutine))
            {
                // Remove from routines to prevent infinite loop when the callback invokes a new monoroutine
                // on this monobehaviour
                routines.Remove(instanceId);

                // Stop already running 'mono' routine
                // Routine can be null (?)
                if (runningRoutine.Item1 != null)
                {
                    monoBehaviour.StopCoroutine(runningRoutine.Item1);
                }

                // When stopped inbetween, fire stored callback
                runningRoutine.Item2?.Invoke();

            }
        }

        /// <summary>
        /// Stops the currently running Monoroutine for this Monobehaviour. 
        /// </summary>
        public static void StopMonoroutine(this MonoBehaviour monoBehaviour)
        {
            int instanceId = monoBehaviour.GetInstanceID();

            if (routines.TryGetValue(instanceId, out var runningRoutine))
            {
                // Remove from collection
                routines.Remove(instanceId);

                // Stop running 'mono' routine
                // Routine can be null (?)
                if (runningRoutine.Item1 != null)
                    monoBehaviour.StopCoroutine(runningRoutine.Item1);

                // Stopped inbetween, fire stored callback
                runningRoutine.Item2?.Invoke();
            }
        }

        public static bool IsRunningMonoroutine(this MonoBehaviour monoBehaviour)
        {
            return routines.ContainsKey(monoBehaviour.GetInstanceID());
        }

        private static IEnumerator WrapMonoroutine(IEnumerator coroutine, Action callback)
        {
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }

            // this will wrap back to the creation of the tuple with coroutine and callback
            callback?.Invoke();
        }
    }
}