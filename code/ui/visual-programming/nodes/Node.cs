using System;

using Sandbox;

namespace TTTReborn.UI.VisualProgramming
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeAttribute : LibraryAttribute
    {
        public NodeAttribute(string name) : base(name)
        {

        }
    }

    public abstract class Node : Modal
    {
        public string LibraryName { get; set; }

        public Node(Sandbox.UI.Panel parent = null) : base(parent)
        {
            Header.DragHeader.IsFreeDraggable = true;
            Header.DragHeader.IsLocked = false;

            AddClass("node");
            AddClass("box-shadow");

            LibraryName = GetAttribute().Name;
        }

        public static NodeAttribute GetAttribute<T>() where T : Node
        {
            return Library.GetAttribute(typeof(T)) as NodeAttribute;
        }

        public NodeAttribute GetAttribute()
        {
            return Library.GetAttribute(GetType()) as NodeAttribute;
        }
    }
}
