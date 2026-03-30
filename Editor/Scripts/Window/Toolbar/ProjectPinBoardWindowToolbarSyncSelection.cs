using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarToggle m_ToolbarSyncSelectionToggle = null;

        private void InitToolbarSyncSelectionToggle()
        {
            m_ToolbarSyncSelectionToggle = new ToolbarToggle()
            {
                name = "SyncSelection",
                tooltip = "Sync selection to Project Browser",
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 25,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginLeft = -1,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Toolbar.Add(m_ToolbarSyncSelectionToggle);
            {
                VisualElement input = m_ToolbarSyncSelectionToggle.Q<VisualElement>(
                    "",
                    "unity-toggle__input"
                );
                input.style.flexGrow = 0;
            }
            m_ToolbarSyncSelectionToggle.Add(
                new Image()
                {
                    image = PipiUtility.GetIcon("Grid.Default"),
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 16, }
                }
            );
            m_ToolbarSyncSelectionToggle.RegisterValueChangedCallback(OnSyncSelectionValueChanged);
        }

        private void OnSyncSelectionValueChanged(ChangeEvent<bool> evt)
        {
            ProjectPinBoardSettings.syncSelection = evt.newValue;
        }
    }
}
