using System.Linq;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private VisualElement m_Toolbar = null;

        private void InitToolbar()
        {
            m_Toolbar = rootVisualElement.Q<VisualElement>("Toolbar");
            {
                m_Toolbar.style.height = 20;
                m_Toolbar.style.flexShrink = 0;
                m_Toolbar.style.flexDirection = FlexDirection.Row;
            }
            m_Toolbar.RegisterCallback<GeometryChangedEvent>(OnToolbarGeometryChangedEventChanged);

            VisualElement separator = rootVisualElement.Q<VisualElement>("Separator");
            {
                separator.style.borderBottomColor = separatorColor;
            }

            InitToolbarSearchField();
            InitToolbarFilterButton();
            InitToolbarTopFolderToggle();
            InitToolbarPreviewToggle();
            InitToolbarSyncSelectionToggle();
            InitToolbarSortingMenu();

            VisualElement[] elements = m_Toolbar.Children().ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
                if (i == elements.Length - 1)
                {
                    VisualElement element = elements[i];
                    element.style.marginRight = -1;
                }
            }
        }

        private void OnToolbarGeometryChangedEventChanged(GeometryChangedEvent evt)
        {
            m_ToolbarPreviewToggle.style.display = (
                (m_Toolbar.localBound.width <= 250) ? DisplayStyle.None : DisplayStyle.Flex
            );
            m_ToolbarSyncSelectionToggle.style.display = (
                (m_Toolbar.localBound.width <= 200) ? DisplayStyle.None : DisplayStyle.Flex
            );
        }
    }
}
