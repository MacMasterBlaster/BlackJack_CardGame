using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedHierarchy.Icons;
using UnityEditor;
using UnityEngine;

namespace EnhancedHierarchy {

    internal enum IconSide {
        RightOfName = 1,
        RightMost = 2,
        LeftOfName = 4,
        SafeArea = RightOfName | RightMost,
        All = SafeArea | LeftOfName
    }

    internal abstract class IconBase {

        private const float DEFAULT_WIDTH = 16f;

        public static None none = new None();

        public virtual string Name { get { return GetType().Name; } }
        public virtual float Width { get { return DEFAULT_WIDTH; } }

        public virtual IconSide Side { get { return IconSide.SafeArea; } }

        public static IconBase[] AllLeftIcons { get; private set; }
        public static IconBase[] AllRightIcons { get; private set; }
        public static IconBase[] AllLeftOfNameIcons { get; private set; }

        static IconBase() {
            var baseType = typeof(IconBase);

            icons = baseType.Assembly.GetTypes()
                .Where(t => t != baseType && baseType.IsAssignableFrom(t))
                .Select(t => (IconBase)Activator.CreateInstance(t))
                .ToDictionary(t => t.Name);

            AllLeftIcons = icons.Select(i => i.Value).Where(i => (i.Side & IconSide.RightOfName) != 0).ToArray();
            AllRightIcons = icons.Select(i => i.Value).Where(i => (i.Side & IconSide.RightMost) != 0).ToArray();
            AllLeftOfNameIcons = icons.Select(i => i.Value).Where(i => (i.Side & IconSide.LeftOfName) != 0).ToArray();
        }

        public virtual void Init() { }
        public abstract void DoGUI(Rect rect);

        private static readonly Dictionary<string, IconBase> icons = new Dictionary<string, IconBase>();

        protected static ChildrenChangeMode AskChangeModeIfNecessary(List<GameObject> objs, ChildrenChangeMode reference, string title, string message) {
            var isControl = Event.current.control || Event.current.command;

            switch(reference) {
                case ChildrenChangeMode.ObjectOnly:
                    if(!isControl)
                        return ChildrenChangeMode.ObjectOnly;
                    else
                        return ChildrenChangeMode.ObjectAndChildren;

                case ChildrenChangeMode.ObjectAndChildren:
                    if(!isControl)
                        return ChildrenChangeMode.ObjectAndChildren;
                    else
                        return ChildrenChangeMode.ObjectOnly;

                default:
                    foreach(var obj in objs)
                        if(obj && obj.transform.childCount > 0)
                            try {
                                return (ChildrenChangeMode)EditorUtility.DisplayDialogComplex(title, message, "Yes, change children", "No, this object only", "Cancel");
                            }
                            finally {
                                //Unity bug, DisplayDialogComplex makes the unity partially lose focus
                                if(EditorWindow.focusedWindow)
                                    EditorWindow.focusedWindow.Focus();
                            }

                    return ChildrenChangeMode.ObjectOnly;
            }
        }

        protected static List<GameObject> GetSelectedObjectsAndCurrent() {
            if(!Preferences.ChangeAllSelected || Selection.gameObjects.Length <= 1)
                return new List<GameObject> { EnhancedHierarchy.CurrentGameObject };

            var selection = new List<GameObject>(Selection.gameObjects);

            for(var i = 0; i < selection.Count; i++)
                if(EditorUtility.IsPersistent(selection[i]))
                    selection.RemoveAt(i);

            if(!selection.Contains(EnhancedHierarchy.CurrentGameObject))
                selection.Add(EnhancedHierarchy.CurrentGameObject);

            selection.Remove(null);
            return selection;
        }

        public static bool operator ==(IconBase left, IconBase right) {
            if(ReferenceEquals(left, right))
                return true;

            if(ReferenceEquals(left, null))
                return false;

            if(ReferenceEquals(right, null))
                return false;

            return left.Name == right.Name;
        }

        public static bool operator !=(IconBase left, IconBase right) {
            return !(left == right);
        }

        public override string ToString() {
            return Name;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj as IconBase == this;
        }

        public static implicit operator IconBase(string name) {
            try { return icons[name]; }
            catch { return none; }
        }

        public static implicit operator string(IconBase icon) {
            return icon.ToString();
        }

    }

}