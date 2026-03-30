using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarToggle m_ToolbarPreviewToggle = null;

        private void InitToolbarPreviewToggle()
        {
            m_ToolbarPreviewToggle = new ToolbarToggle()
            {
                name = "PreviewToggle",
                tooltip = "Enable preview panel",
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
            m_Toolbar.Add(m_ToolbarPreviewToggle);
            {
                VisualElement input = m_ToolbarPreviewToggle.Q<VisualElement>(
                    "",
                    "unity-toggle__input"
                );
                input.style.flexGrow = 0;
            }
            m_ToolbarPreviewToggle.Add(
                new Image()
                {
                    image = PipiUtility.GetIcon("scenevis_visible_hover"),
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 16, }
                }
            );
            m_ToolbarPreviewToggle.RegisterValueChangedCallback(OnPreviewToggleValueChanged);
        }

        private void OnPreviewToggleValueChanged(ChangeEvent<bool> evt)
        {
            ProjectPinBoardSettings.enablePreview = evt.newValue;
            TogglePreview(evt.newValue);
            if (evt.newValue)
            {
                ApplySettings_DragLine();
            }
        }
    }
}
