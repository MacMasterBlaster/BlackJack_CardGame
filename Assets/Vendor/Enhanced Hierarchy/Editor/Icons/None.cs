using UnityEngine;

namespace EnhancedHierarchy.Icons {
    internal sealed class None : IconBase {
        public override float Width { get { return 0f; } }
        public override string Name { get { return "None"; } }
        public override IconSide Side { get { return IconSide.All; } }
        public override void DoGUI(Rect rect) { }
    }
}