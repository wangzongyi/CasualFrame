using System.Collections.Generic;
using UnityEngine;

public class Yielders
{
    public class WaitCache
    {
        public float WaitTime;
        public int InvokeTimes = 0;
        public WaitForSeconds Yielder;
    }

    public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly int MaxYielderCount = 50;

    private static Dictionary<float, LinkedListNode<WaitCache>> waitHash;

    private static LinkedList<WaitCache> waitQueue;

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (waitHash == null)
            waitHash = new Dictionary<float, LinkedListNode<WaitCache>>();

        if (waitQueue == null)
            waitQueue = new LinkedList<WaitCache>();

        WaitForSeconds waitForSeconds;

        if (!waitHash.ContainsKey(seconds))
        {
            LinkedListNode<WaitCache> newNode = new LinkedListNode<WaitCache>(new WaitCache()
            {
                WaitTime = seconds,
                InvokeTimes = 1,
                Yielder = new WaitForSeconds(seconds),
            });
            waitHash[seconds] = newNode;
            waitQueue.AddLast(newNode);
            waitForSeconds = newNode.Value.Yielder;
        }
        else
        {
            LinkedListNode<WaitCache> currentNode = waitHash[seconds];
            LinkedListNode<WaitCache> nextNode = currentNode.Next;
            currentNode.Value.InvokeTimes += 1;
            if (nextNode != null && currentNode.Value.InvokeTimes > nextNode.Value.InvokeTimes)
            {
                waitQueue.Remove(currentNode);
                waitQueue.AddAfter(nextNode, currentNode);
            }
            waitForSeconds = currentNode.Value.Yielder;
        }

        if (waitQueue.Count > MaxYielderCount)
        {
            LinkedListNode<WaitCache> node = waitQueue.First;
            if (waitHash.ContainsKey(node.Value.WaitTime))
            {
                waitHash.Remove(node.Value.WaitTime);
                waitQueue.Remove(node);
            }
        }

        return waitForSeconds;
    }

    public static void ClearYielders()
    {
        if (waitHash != null)
            waitHash.Clear();

        if (waitQueue != null)
            waitQueue.Clear();
    }
}
