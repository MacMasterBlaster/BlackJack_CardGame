﻿using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EnhancedHierarchy.Icons {
    internal sealed class RendererToggle : IconBase {

        private Renderer renderer;

        public override IconSide Side { get { return IconSide.All; } }
        public override string Name { get { return "Renderer"; } }

        public override void Init() {
            renderer = EnhancedHierarchy.Components.FirstOrDefault(c => c is Renderer) as Renderer;
        }

        public override float Width { get { return renderer ? base.Width : 0; } }

        public override void DoGUI(Rect rect) {
            if(!renderer)
                return;

            using(new GUIBackgroundColor(renderer.enabled ? Styles.backgroundColorEnabled : Styles.backgroundColorDisabled)) {
                GUI.changed = false;
                GUI.Toggle(rect, renderer, Styles.rendererContent, Styles.rendererToggleStyle);

                if(!GUI.changed)
                    return;

                var objs = GetSelectedObjectsAndCurrent().SelectMany(go => go.GetComponents<Renderer>());
                var active = !renderer.enabled;

                Undo.RecordObjects(objs.ToArray(), renderer.enabled ? "Disabled renderer" : "Enabled renderer");

                foreach(var obj in objs)
                    obj.enabled = active;
            }
        }

    }
}