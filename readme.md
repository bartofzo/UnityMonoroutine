Extension class for a Monobehaviour that allows for easy execution a single Coroutine at a time.

## Usage:

`
using UnityMonoroutine;

// Fades in CanvasGroup and then fades it out again
canvasGroup.alpha = 0;
this.StartMonoroutine(canvasGroup.AlphaFade(1, 1),
    () => this.StartMonoroutine(canvasGroup.AlphaFade(0, 1),
    () => Debug.Log("Finished!")));
`

Example of such a routine would be a fade in/fadeout routine, where one can interrupt the other.
Provides an optional callback that is garantueed to be called when the coroutine stops (either by finishing execution or by interruption)

2019 - Bart van de Sande / Nonline
