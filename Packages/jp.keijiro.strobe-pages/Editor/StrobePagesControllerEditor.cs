using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StrobePages {

[CustomEditor(typeof(StrobePagesController)), CanEditMultipleObjects]
sealed class StrobePagesControllerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        AddProperty(root, nameof(StrobePagesController.PageInterval));
        AddProperty(root, nameof(StrobePagesController.PageStiffness));
        AddProperty(root, nameof(StrobePagesController.MotionBlur));
        AddProperty(root, nameof(StrobePagesController.SampleCount));
        AddProperty(root, nameof(StrobePagesController.ShadeWidth));
        AddProperty(root, nameof(StrobePagesController.ShadeStrength));
        AddProperty(root, nameof(StrobePagesController.Opacity));
        return root;
    }

    void AddProperty(VisualElement root, string name)
      => root.Add(new PropertyField(FindBackedProperty(name)));

    SerializedProperty FindBackedProperty(string name)
        => serializedObject.FindProperty($"<{name}>k__BackingField");
}

} // namespace StrobePages
