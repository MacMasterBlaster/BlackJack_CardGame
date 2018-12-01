using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EnhancedHierarchy {
    /// <summary>
    /// Class containing method extensions for getting private and internal members.
    /// </summary>
    internal static partial class ReflectionHelper {

        public const BindingFlags FULL_BINDING = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public delegate void OnObjectIconChange(Texture2D icon);

        private static bool gameObjectStylesTypeLoaded = false;
        private static Type gameObjectTreeViewStylesType;
        private static Assembly[] assemblies;

        private static readonly Type hierarchyWindowType = FindClass("UnityEditor.SceneHierarchyWindow");
        private static readonly Type iconSelectorType = FindClass("UnityEditor.IconSelector");
        private static readonly Type[] IsExpandedParamsTypes = new[] { typeof(int) };
        private static readonly Assembly editorAssembly = typeof(Editor).Assembly;

        private static EditorWindow hierarchyWindowInstance;

        public static bool HierarchyFocused {
            get {
                return EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType() == hierarchyWindowType;
            }
        }

        public static Color PlaymodeTint {
            get {
                try {
                    if(!EditorApplication.isPlayingOrWillChangePlaymode)
                        return Color.white;
                    else
                        return FindClass("UnityEditor.HostView").GetFieldValue<object>("kPlayModeDarken").GetPropertyValue<Color>("Color");
                }
                catch(Exception e) {
                    if(Preferences.DebugEnabled)
                        Debug.LogException(e);
                    return Color.white;
                }
            }
        }

        public static EditorWindow HierarchyWindowInstance {
            get {
                if(hierarchyWindowInstance)
                    return hierarchyWindowInstance;

                var lastHierarchy = (EditorWindow)null;

                try {
                    lastHierarchy = hierarchyWindowType.GetFieldValue<EditorWindow>("s_LastInteractedHierarchy");
                }
                catch(Exception e) {
                    if(Preferences.DebugEnabled)
                        Debug.LogException(e);
                }

                if(lastHierarchy != null)
                    return hierarchyWindowInstance = lastHierarchy;
                else
                    return hierarchyWindowInstance = (EditorWindow)Resources.FindObjectsOfTypeAll(hierarchyWindowType).FirstOrDefault();
            }
        }

        public static void ShowIconSelector(Object targetObj, Rect activatorRect, bool showLabelIcons) {
            ShowIconSelector(targetObj, activatorRect, showLabelIcons, icon => { });
        }

        public static void ShowIconSelector(Object targetObj, Rect activatorRect, bool showLabelIcons, OnObjectIconChange onObjectChange) {
            using(ProfilerSample.Get())
                try {
                    var instance = ScriptableObject.CreateInstance(iconSelectorType);
                    var update = new EditorApplication.CallbackFunction(() => { });

                    instance.InvokeMethod("Init", targetObj, activatorRect, showLabelIcons);

                    update += () => {
                        if(instance)
                            return;

                        onObjectChange(GetObjectIcon(targetObj));
                        EditorApplication.update -= update;
                    };

                    EditorApplication.update += update;
                }
                catch(Exception e) {
                    Debug.LogWarning("Failed to open icon selector\n" + e);
                }
        }

        public static void SetObjectIcon(Object obj, Texture2D texture) {
            using(ProfilerSample.Get()) {
                typeof(EditorGUIUtility).InvokeMethod("SetIconForObject", obj, texture);
                EditorUtility.SetDirty(obj);
            }
        }

        public static Texture2D GetObjectIcon(Object obj) {
            using(ProfilerSample.Get())
                return typeof(EditorGUIUtility).InvokeMethod<Texture2D>("GetIconForObject", obj);
        }

        public static bool GetTransformIsExpanded(GameObject go) {
            using(ProfilerSample.Get())
                try {
                    var data = TreeView.GetPropertyValue<object>("data");
                    var isExpanded = data.InvokeMethod<bool>("IsExpanded", IsExpandedParamsTypes, go.GetInstanceID());

                    return isExpanded;
                }
                catch(Exception e) {
                    Preferences.NumericChildExpand.Value = false;
                    Debug.LogException(e);
                    Debug.LogWarningFormat("Disabled \"{0}\" because it failed to get hierarchy info", Preferences.NumericChildExpand.Label.text);
                    return false;
                }
        }

        public static void SetHierarchySelectionNeedSync() {
            using(ProfilerSample.Get())
                try {
                    if(HierarchyWindowInstance)
                        SceneHierarchyOrWindow.SetPropertyValue("selectionSyncNeeded", true);
                }
                catch(Exception e) {
                    Debug.LogWarningFormat("Enabling \"{0}\" because it caused an exception", Preferences.AllowSelectingLocked.Label.text);
                    Debug.LogException(e);
                    Preferences.AllowSelectingLocked.Value = true;
                }
        }

        private static object SceneHierarchy { get { return HierarchyWindowInstance.GetPropertyValue<object>("sceneHierarchy"); } }

        private static object SceneHierarchyOrWindow {
            get {
#if UNITY_2018_3_OR_NEWER
                return HierarchyWindowInstance.GetPropertyValue<object>("sceneHierarchy");
#else
                return HierarchyWindowInstance;
#endif
            }
        }

        public static object TreeView { get { return SceneHierarchyOrWindow.GetPropertyValue<object>("treeView"); } }

        public static object TreeViewGUI { get { return TreeView.GetPropertyValue<object>("gui"); } }

        public static bool IconWidthSupported {
            get {
#if UNITY_2018_3_OR_NEWER
                return TreeView != null && TreeViewGUI != null && TreeViewGUI.HasField("k_IconWidth");
#else 
                return false;
#endif
            }
        }

        // Icon to the left side of obj name, introducted on Unity 2018.3
        public static float IconWidth {
            get {
                if(!IconWidthSupported)
                    return 0;
                return TreeViewGUI.GetFieldValue<float>("k_IconWidth");
            }
            set { TreeViewGUI.SetFieldValue("k_IconWidth", value); }
        }

        public static class HierarchyArea {

            static HierarchyArea() {
                if(Preferences.DebugEnabled && !Supported)
                    Debug.LogWarning("HierarchyArea not supported!");
            }

            public static bool Supported {
                get {
                    try {
                        return HierarchyWindowInstance != null && TreeView != null && TreeViewGUI != null;
                    }
                    catch {
                        return false;
                    }
                }
            }

            public static float IndentWidth {
                get { return TreeViewGUI.GetFieldValue<float>("k_IndentWidth"); }
                set { TreeViewGUI.SetFieldValue("k_IndentWidth", value); }
            }

            //public static float foldoutYOffset {
            //    get { return TreeViewGUI.GetFieldValue<float>("foldoutYOffset"); }
            //    set { TreeViewGUI.SetFieldValue("foldoutYOffset", value); }
            //}

            public static float BaseIndent {
                get { return TreeViewGUI.GetFieldValue<float>("k_BaseIndent"); }
                set { TreeViewGUI.SetFieldValue("k_BaseIndent", value); }
            }

            public static float BottomRowMargin {
                get { return TreeViewGUI.GetFieldValue<float>("k_BottomRowMargin"); }
                set { TreeViewGUI.SetFieldValue("k_BottomRowMargin", value); }
            }

            public static float TopRowMargin {
                get { return TreeViewGUI.GetFieldValue<float>("k_TopRowMargin"); }
                set { TreeViewGUI.SetFieldValue("k_TopRowMargin", value); }
            }

            public static float HalfDropBetweenHeight {
                get { return TreeViewGUI.GetFieldValue<float>("k_HalfDropBetweenHeight"); }
                set { TreeViewGUI.SetFieldValue("k_HalfDropBetweenHeight", value); }
            }

            public static float IconWidth {
                get { return TreeViewGUI.GetFieldValue<float>("k_IconWidth"); }
                set { TreeViewGUI.SetFieldValue("k_IconWidth", value); }
            }

            public static float LineHeight {
                get { return TreeViewGUI.GetFieldValue<float>("k_LineHeight"); }
                set { TreeViewGUI.SetFieldValue("k_LineHeight", value); }
            }

            public static float SpaceBetweenIconAndText {
                get { return TreeViewGUI.GetFieldValue<float>("k_SpaceBetweenIconAndText"); }
                set { TreeViewGUI.SetFieldValue("k_SpaceBetweenIconAndText", value); }
            }

            public static float IconLeftPadding {
                get { return TreeViewGUI.GetPropertyValue<float>("iconLeftPadding"); }
                set { TreeViewGUI.SetPropertyValue("iconLeftPadding", value); }
            }

            public static float IconRightPadding { //Same as iconLeftPadding
                get { return TreeViewGUI.GetPropertyValue<float>("iconRightPadding"); }
                set { TreeViewGUI.SetPropertyValue("iconRightPadding", value); }
            }
        }

        private static Type GameObjectTreeViewStylesType {
            get {
                if(!gameObjectStylesTypeLoaded) {
                    gameObjectStylesTypeLoaded = true;
                    gameObjectTreeViewStylesType = TreeViewGUI.GetType().GetNestedType("GameObjectStyles", FULL_BINDING);
                }

                return gameObjectTreeViewStylesType;
            }
        }

        public static bool NativeHierarchyHoverTintSupported {
            get {
                return GameObjectTreeViewStylesType != null && GameObjectTreeViewStylesType.FindField("hoveredBackgroundColor") != null;
            }
        }

        // I implement the hover tint and then a few weeks later
        // unity implements it as a native feature
        public static Color NativeHierarchyHoverTint {
            get {
                if(Preferences.DebugEnabled && !NativeHierarchyHoverTintSupported) {
                    Debug.LogWarning("Native hover tint not supported!");
                    return Color.clear;
                }

                return GameObjectTreeViewStylesType.GetFieldValue<Color>("hoveredBackgroundColor");
            }
            set {
                if(Preferences.DebugEnabled && !NativeHierarchyHoverTintSupported) {
                    Debug.LogWarning("Native hover tint not supported!");
                    return;
                }

                GameObjectTreeViewStylesType.SetFieldValue("hoveredBackgroundColor", value);
            }
        }

        #region Extension methods 
        private static readonly Dictionary<string, FieldInfo> cachedFields = new Dictionary<string, FieldInfo>(100);
        private static readonly Dictionary<string, PropertyInfo> cachedProps = new Dictionary<string, PropertyInfo>(100);
        private static readonly Dictionary<string, MethodInfo> cachedMethods = new Dictionary<string, MethodInfo>(100);

        public static Type FindClass(string name) {
            var type = FindClass(name, editorAssembly);

            if(type != null)
                return type;
            else {
                if(assemblies == null)
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();

                for(var i = 0; i < assemblies.Length; i++) {
                    type = FindClass(name, assemblies[i]);

                    if(type != null)
                        return type;
                }
            }

            return null;
        }

        public static Type FindClass(string name, Assembly assembly) {
            if(assembly == null)
                return null;

            using(ProfilerSample.Get(name))
                return assembly.GetType(name, false, true);
        }

        public static FieldInfo FindField(this Type type, string fieldName) {
            using(ProfilerSample.Get(fieldName))
                try { return cachedFields[fieldName]; }
                catch { return cachedFields[fieldName] = type.GetField(fieldName, FULL_BINDING); }
        }

        public static PropertyInfo FindProperty(this Type type, string propertyName) {
            using(ProfilerSample.Get(propertyName))
                try { return cachedProps[propertyName]; }
                catch { return cachedProps[propertyName] = type.GetProperty(propertyName, FULL_BINDING); }
        }

        public static MethodInfo FindMethod(this Type type, string methodName, Type[] parameters = null) {
            using(ProfilerSample.Get(methodName))
                try { return cachedMethods[methodName]; }
                catch {
                    if(parameters == null)
                        return cachedMethods[methodName] = type.GetMethod(methodName, FULL_BINDING);
                    else
                        return cachedMethods[methodName] = type.GetMethod(methodName, FULL_BINDING, null, parameters, null);
                }
        }

        public static bool HasField(this object obj, string fieldName) {
            return obj.GetType().FindField(fieldName) != null;
        }

        public static bool HasProperty(this object obj, string propertyName) {
            return obj.GetType().FindProperty(propertyName) != null;
        }

        public static bool HasMethod(this object obj, string methodName, Type[] parameters = null) {
            return obj.GetType().FindMethod(methodName, parameters) != null;
        }

        public static T GetFieldValue<T>(this Type type, string fieldName) {
            using(ProfilerSample.Get(fieldName))
                return (T)type.FindField(fieldName).GetValue(null);
        }

        public static T GetFieldValue<T>(this object obj, string fieldName) {
            using(ProfilerSample.Get(fieldName))
                return (T)obj.GetType().FindField(fieldName).GetValue(obj);
        }

        public static void SetFieldValue(this Type type, string fieldName, object value) {
            using(ProfilerSample.Get(fieldName))
                type.FindField(fieldName).SetValue(null, value);
        }

        public static void SetFieldValue(this object obj, string fieldName, object value) {
            using(ProfilerSample.Get(fieldName))
                obj.GetType().FindField(fieldName).SetValue(obj, value);
        }

        public static T GetPropertyValue<T>(this Type type, string propertyName) {
            using(ProfilerSample.Get(propertyName))
                return (T)type.FindProperty(propertyName).GetValue(null, null);
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName) {
            using(ProfilerSample.Get(propertyName))
                return (T)obj.GetType().FindProperty(propertyName).GetValue(obj, null);
        }

        public static void SetPropertyValue(this Type type, string propertyName, object value) {
            using(ProfilerSample.Get(propertyName))
                type.FindProperty(propertyName).SetValue(null, value, null);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value) {
            using(ProfilerSample.Get(propertyName))
                obj.GetType().FindProperty(propertyName).SetValue(obj, value, null);
        }

        public static T InvokeMethod<T>(this Type type, string methodName, params object[] args) {
            return InvokeMethod<T>(type, methodName, null, args);
        }

        public static T InvokeMethod<T>(this object obj, string methodName, params object[] args) {
            return InvokeMethod<T>(obj, methodName, null, args);
        }

        public static T InvokeMethod<T>(this Type type, string methodName, Type[] parameters, params object[] args) {
            return (T)type.FindMethod(methodName, parameters).Invoke(null, args);
        }

        public static T InvokeMethod<T>(this object obj, string methodName, Type[] parameters, params object[] args) {
            return (T)obj.GetType().FindMethod(methodName, parameters).Invoke(obj, args);
        }

        public static void InvokeMethod(this Type type, string methodName, params object[] args) {
            InvokeMethod(type, methodName, null, args);
        }

        public static void InvokeMethod(this object obj, string methodName, params object[] args) {
            InvokeMethod(obj, methodName, null, args);
        }

        public static void InvokeMethod(this Type type, string methodName, Type[] parameters, params object[] args) {
            using(ProfilerSample.Get(methodName))
                type.FindMethod(methodName, parameters).Invoke(null, args);
        }

        public static void InvokeMethod(this object obj, string methodName, Type[] parameters, params object[] args) {
            using(ProfilerSample.Get(methodName))
                obj.GetType().FindMethod(methodName, parameters).Invoke(obj, args);
        }
        #endregion

    }
}