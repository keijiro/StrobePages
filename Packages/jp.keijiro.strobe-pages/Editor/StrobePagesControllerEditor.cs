using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StrobePages {

[CustomEditor(typeof(StrobePagesController)), CanEditMultipleObjects]
sealed class StrobePagesControllerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        AddProperty(root, nameof(StrobePagesController.AutoPageTurn));
        AddProperty(root, nameof(StrobePagesController.PageInterval));
        AddProperty(root, nameof(StrobePagesController.PageStiffness));
        AddProperty(root, nameof(StrobePagesController.MotionBlur));
        AddProperty(root, nameof(StrobePagesController.SampleCount));
        AddProperty(root, nameof(StrobePagesController.ShadeWidth));
        AddProperty(root, nameof(StrobePagesController.ShadeStrength));
        AddProperty(root, nameof(StrobePagesController.Opacity));
        var button = new Button(StartPageTurn) { text = "Start Page Turn" };
        root.Add(button);
        return root;
    }

    void StartPageTurn()
    {
        foreach (var targetObject in targets)
            if (targetObject is StrobePagesController controller)
            {
                controller.StartPageTurn();
                if (!Application.isPlaying) EditorUtility.SetDirty(controller);
            }
    }

    void AddProperty(VisualElement root, string name)
      => root.Add(new PropertyField(FindBackedProperty(name)));

    SerializedProperty FindBackedProperty(string name)
        => serializedObject.FindProperty($"<{name}>k__BackingField");
}

} // namespace StrobePages
