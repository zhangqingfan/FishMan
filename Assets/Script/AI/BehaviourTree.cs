using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Node
    {
        protected MonoBehaviour mono;

        public enum ExecResult
        {
            Success,
            InProcess,
            Failure
        }
        protected Node(MonoBehaviour mono)
        {
            result = ExecResult.InProcess;
            nodeList = new List<Node>();
            this.mono = mono;
        }

        public ExecResult result;
        public List<Node> nodeList;
        public virtual IEnumerator Exec() { yield break; }
    }

    public class Selector : Node
    {
        public Selector(MonoBehaviour mono) : base(mono)
        {
        }

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
        public Sequence(MonoBehaviour mono) : base(mono)
        {
        }

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
            public Parallel(MonoBehaviour mono) : base(mono)
            {
            }

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
