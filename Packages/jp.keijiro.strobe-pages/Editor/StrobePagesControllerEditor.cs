using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StrobePages {

[CustomEditor(typeof(StrobePagesController)), CanEditMultipleObjects]
sealed class StrobePagesControllerEditor : Editor
{
    [SerializeField] VisualTreeAsset _uxml = null;

    public override VisualElement CreateInspectorGUI()
    {
        var ui = _uxml.CloneTree();

        var button = ui.Q<Button>("page-turn-button");
        button.clicked += OnPageTurnButton;

        var flag = serializedObject.FindProperty("<AutoPageTurn>k__BackingField");
        UpdateButtonVisibility(button, flag);

        ui.TrackPropertyValue(flag, _ => UpdateButtonVisibility(button, flag));

        var root = new VisualElement();
        root.Add(ui);
        return root;
    }

    void UpdateButtonVisibility(Button button, SerializedProperty flag)
      => button.style.display =
          flag is { boolValue: false, hasMultipleDifferentValues: false } ?
            DisplayStyle.Flex : DisplayStyle.None;

    void OnPageTurnButton()
    {
        foreach (StrobePagesController controller in targets)
            controller.StartPageTurn();
    }
}

} // namespace StrobePages
