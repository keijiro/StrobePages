using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StrobePages {

[CustomEditor(typeof(StrobePagesController)), CanEditMultipleObjects]
sealed class StrobePagesControllerEditor : Editor
{
    [SerializeField] VisualTreeAsset _uxml = null;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var ui = _uxml.CloneTree();
        ui.Q<Button>("page-turn-button").clicked += OnPageTurnButton;
        root.Add(ui);
        return root;
    }

    void OnPageTurnButton()
    {
        foreach (StrobePagesController controller in targets)
            controller.StartPageTurn();
    }
}

} // namespace StrobePages
