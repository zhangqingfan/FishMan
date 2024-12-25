using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Node : MonoBehaviour
    {
        public enum ExecResult
        {
            Success,
            InProcess,
            Failure
        }
        public Node()
        {
            result = ExecResult.InProcess;
            nodeList = new List<Node>();
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
                yield return StartCoroutine(node.Exec());
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
                yield return StartCoroutine(node.Exec());
            }

            result = ExecResult.Success;
        }

        public class Parallel : Node
        {
            public override IEnumerator Exec()
            {
                foreach (Node node in nodeList)
                {
                    StartCoroutine(node.Exec());
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
