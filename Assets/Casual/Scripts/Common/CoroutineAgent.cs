using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CoroutineAgent : MonoSingleton<CoroutineAgent>
{
    public static Coroutine StartCoroutine(string co, MonoBehaviour behaviour, object value = null)
    {
        return behaviour.StartCoroutine(co, value);
    }

    public static void StopCoroutine(string co, MonoBehaviour behaviour)
    {
        behaviour.StopCoroutine(co);
    }

    public static Coroutine StartCoroutine(IEnumerator ie, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(ie);
    }

    public static void StopCoroutine(IEnumerator ie, MonoBehaviour behaviour = null)
    {
        if (ie == null || MonoContext.IsApplicationQuit)
            return;

        (behaviour ?? Instance()).StopCoroutine(ie);
    }

    public static void StopCoroutine(Coroutine co, MonoBehaviour behaviour = null)
    {
        if (co == null || MonoContext.IsApplicationQuit)
            return;

        (behaviour ?? Instance()).StopCoroutine(co);
    }

    public static Coroutine EndOfFrameOperation(System.Action operation, MonoBehaviour behaviour = null)
    {
        return DelayOperation(Yielders.WaitForEndOfFrame, operation, behaviour);
    }

    public static Coroutine DelayOperation(float delay, System.Action operation, MonoBehaviour behaviour = null)
    {
        return DelayOperation(Yielders.WaitForSeconds(delay), operation, behaviour);
    }

    public static Coroutine DelayOperation(YieldInstruction delay, System.Action operation, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(DoDelayOperation(delay, operation));
    }

    private static IEnumerator DoDelayOperation(YieldInstruction delay, System.Action operation)
    {
        if (operation != null)
        {
            yield return delay;
            operation();
        }
    }

    public static Coroutine DelayOperation(MonoBehaviour behaviour, params KeyValuePair<YieldInstruction, System.Action>[] operations)
    {
        return (behaviour ?? Instance()).StartCoroutine(DelayOperation(operations));
    }

    private static IEnumerator DelayOperation(params KeyValuePair<YieldInstruction, System.Action>[] operations)
    {
        foreach (KeyValuePair<YieldInstruction, System.Action> operation in operations)
        {
            yield return operation.Key;
            operation.Value?.Invoke();
        }
    }

    public static KeyValuePair<YieldInstruction, System.Action> PackDelay(YieldInstruction delay, System.Action operation)
    {
        return new KeyValuePair<YieldInstruction, System.Action>(delay, operation);
    }

    public static Coroutine WaitOperation(System.Func<bool> waitFor, System.Action operation, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(DoWaitOperation(waitFor, operation));
    }

    private static IEnumerator DoWaitOperation(System.Func<bool> waitFor, System.Action operation = null)
    {
        if (operation != null)
        {
            if (waitFor != null)
            {
                while (!waitFor())
                {
                    yield return null;
                }
            }
            operation();
        }
    }
}
