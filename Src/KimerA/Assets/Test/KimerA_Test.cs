using UnityEngine;

namespace KimerA.Test
{
    public sealed class KimerA_Test : MonoBehaviour
    {
        private void Awake()
        {
            var init = new KimerA_Init();
            init.AddNode(new NodeOne(1))
                .AddWithChildren(new NodeOne(1), cb =>
                {
                    cb.AddNode(new NodeTwo("child"));
                    cb.AddNode(new NodeTwo("child"))
                        .AddWithChildren(new NodeTwo("childchild"), cb =>
                        {
                            cb.AddNode(new NodeOne(123));
                        });
                })
                .InsertBy(
                    new NodeOne(114514),
                    predicate: static provider => provider is ImmutableNodeProvider<NodeOne> nodeProvider && nodeProvider.Instance.Value == 1,
                    transform: static provider => provider as ImmutableNodeProvider<NodeOne>
                )
                .Load();
        }
    }

    public class NodeOne : INode
    {
        public int Value { get; set; }

        public NodeOne(int val)
        {
            Value = val;
        }

        public void OnStartup(ref NodeActionState state)
        {
            state.ApplyInterceptor(node => node is NodeOne nodeOne && nodeOne.Value == 1, action =>
            {
                action += (ref NodeActionState _) => Debug.Log("NodeOne Value is 1");
                return action;
            });
        }
    }

    public class NodeTwo : INode
    {
        public string Value { get; set; }

        public NodeTwo(string val)
        {
            Value = val;
        }
    }
}