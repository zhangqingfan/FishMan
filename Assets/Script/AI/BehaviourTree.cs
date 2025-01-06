using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CoroutineMono : MonoBehaviour
{

}

namespace BehaviourTree
{
    public abstract class Node
    {
        protected static MonoBehaviour mono;
        static string lockObject = "lock";

        public enum ExecResult
        {
            Success,
            InProcess,
            Failure
        }
        public Node()
        {
            result = ExecResult.Failure;
            nodeList = new List<Node>();

            lock (lockObject)
            {
                if (mono == null)
                {
                    var go = new GameObject("AI_Mono");
                    mono = go.AddComponent<CoroutineMono>();
                }
            }
        }

        public ExecResult result;
        public List<Node> nodeList;
        public virtual IEnumerator Exec() { yield break; }
    }

    public class Selector : Node
    {
        public override IEnumerator Exec()
        {
            foreach (Node node in nodeList)
            {
                yield return mono.StartCoroutine(node.Exec());
                if (node.result == ExecResult.Failure)
                    break;
            }

            result = ExecResult.Success;
        }
    }

    public class Sequence : Node
    {
        public override IEnumerator Exec()
        {
            foreach (Node node in nodeList)
            {
                yield return mono.StartCoroutine(node.Exec());
            }

            result = ExecResult.Success;
        }

        public class Parallel : Node
        {
            public override IEnumerator Exec()
            {
                foreach (Node node in nodeList)
                {
                    mono.StartCoroutine(node.Exec());
                }

                while (true)
                {
                    foreach (Node node in nodeList)
                    {
                        if (node.result == ExecResult.InProcess)
                            break;

                        if (node.result != ExecResult.InProcess && node == nodeList[nodeList.Count - 1])
                        {
                            result = ExecResult.Success;
                            yield break;
                        }
                    }
                    yield return null;
                }
            }
        }
    }
}