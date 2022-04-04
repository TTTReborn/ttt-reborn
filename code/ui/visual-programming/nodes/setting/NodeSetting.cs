using System;

using Sandbox;
using Sandbox.UI;

namespace TTTReborn.UI.VisualProgramming
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeSettingAttribute : LibraryAttribute
    {
        public NodeSettingAttribute(string name) : base("ttt_nodesetting_" + name) { }
    }

    public abstract class NodeSetting : Panel
    {
        public string LibraryName { get; set; }

        public Node Node
        {
            get => _node;
            set
            {
                _node = value;

                if (Input != null)
                {
                    Input.Node = _node;
                }

                if (Output != null)
                {
                    Output.Node = _node;
                }
            }
        }
        private Node _node;

        public NodeConnectionPanel<NodeConnectionEndPoint> Input;
        public NodeConnectionPanel<NodeConnectionStartPoint> Output;
        public PanelContent Content;

        public NodeSetting() : base()
        {
            LibraryName = Utils.GetLibraryName(GetType());

            Input = new(this)
            {
                Node = Node
            };

            Content = new()
            {
                Parent = this
            };

            Output = new(this)
            {
                Node = Node
            };

            AddClass("nodesetting");
        }

        public static NodeSettingAttribute GetAttribute<T>() where T : NodeSetting => Library.GetAttribute(typeof(T)) as NodeSettingAttribute;

        public NodeSettingAttribute GetAttribute() => Library.GetAttribute(GetType()) as NodeSettingAttribute;

        public void ToggleInput(bool toggle)
        {
            Input.Enabled = toggle;
        }

        public void ToggleOutput(bool toggle)
        {
            Output.Enabled = toggle;
        }
    }
}
