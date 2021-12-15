using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineAgent : MonoSingleton<CoroutineAgent>
{
    public static Coroutine StartCoroutine(string co, MonoBehaviour behaviour, object value = null)
    {
        return behaviour.isActiveAndEnabled ? behaviour.StartCoroutine(co, value) : null;
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

    public static Coroutine WaitForEndOfFrame(System.Action operation, MonoBehaviour behaviour = null)
    {
        return DelayOperation(Yielders.WaitForEndOfFrame, operation, behaviour);
    }

    public static Coroutine WaitForSeconds(float delay, System.Action operation, MonoBehaviour behaviour = null)
    {
        return DelayOperation(Yielders.WaitForSeconds(delay), operation, behaviour);
    }

    public static Coroutine DelayOperation(YieldInstruction delay, System.Action operation, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(DoDelayOperation(delay, operation));
    }

    private static IEnumerator DoDelayOperation(YieldInstruction delay, System.Action operation)
    {
        yield return delay;
        operation?.Invoke();
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

    public static Coroutine WaitUntil(System.Func<bool> waitFor, System.Action operation, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(CoWaitUtil(waitFor, operation));
    }

    private static IEnumerator CoWaitUtil(System.Func<bool> waitFor, System.Action operation = null)
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

    public static Coroutine DoUntil(System.Func<bool> waitFor, System.Action<float> operation, MonoBehaviour behaviour = null)
    {
        return (behaviour ?? Instance()).StartCoroutine(CoDoUtil(waitFor, operation));
    }

    private static IEnumerator CoDoUtil(System.Func<bool> waitFor, System.Action<float> operation = null)
    {
        if (waitFor != null)
        {
            while (!waitFor())
            {
                operation?.Invoke(Time.deltaTime);
                yield return null;
            }
        }
    }
}
