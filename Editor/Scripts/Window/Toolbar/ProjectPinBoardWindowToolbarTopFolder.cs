using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarToggle m_ToolbarTopFolderToggle = null;

        private void InitToolbarTopFolderToggle()
        {
            m_ToolbarTopFolderToggle = new ToolbarToggle()
            {
                name = "FolderToggle",
                tooltip = "Keep folders on top",
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
            m_Toolbar.Add(m_ToolbarTopFolderToggle);
            {
                VisualElement input = m_ToolbarTopFolderToggle.Q<VisualElement>(
                    "",
                    "unity-toggle__input"
                );
                input.style.flexGrow = 0;
            }
            m_ToolbarTopFolderToggle.Add(
                new Image()
                {
                    image = PipiUtility.GetIcon("Folder Icon"),
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 16, }
                }
            );
            m_ToolbarTopFolderToggle.RegisterValueChangedCallback(OnFolderToggleValueChanged);
        }

        private void OnFolderToggleValueChanged(ChangeEvent<bool> evt)
        {
            ProjectPinBoardSettings.topFolder = evt.newValue;
            UpdateContent();
        }
    }
}
