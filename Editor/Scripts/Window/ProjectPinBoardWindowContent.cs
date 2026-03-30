using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2021_1_OR_NEWER
using System.Linq;
#endif


namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        #region Initialization

        private VisualElement m_Content = null;

        private VisualElement m_ContentPlaceholder = null;

        private TwoPaneSplitView m_ContentSplitView = null;

        private VisualElement m_ContentDragLine = null;

        private void InitContent()
        {
            m_Content = rootVisualElement.Q<VisualElement>("Content");
            {
                m_Content.style.flexBasis = Length.Percent(100);
                m_Content.style.marginTop = 0;
            }

            m_ContentPlaceholder = new VisualElement()
            {
                name = "Placeholder",
                style =
                {
                    flexBasis = Length.Percent(100),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Content.Add(m_ContentPlaceholder);
            {
                m_ContentPlaceholder.Add(
                    new Label()
                    {
                        text = "Empty...",
                        style =
                        {
                            fontSize = 22,
                            color = new Color(128 / 255f, 128 / 255f, 128 / 255f, 0.8f),
                            unityFontStyleAndWeight = FontStyle.Bold,
                        }
                    }
                );
            }
            m_Content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChangedEventChanged);

            m_ContentSplitView = new TwoPaneSplitView()
            {
                name = "ContentSplitView",
                fixedPaneIndex = 0,
                fixedPaneInitialDimension = ProjectPinBoardSettings.dragLinePos,
                orientation = TwoPaneSplitViewOrientation.Horizontal,
                style = { flexBasis = Length.Percent(100), }
            };
            m_Content.Add(m_ContentSplitView);

            {
                m_ContentDragLine = m_ContentSplitView.Q<VisualElement>("unity-dragline-anchor");
                IStyle dragLineStyle = m_ContentDragLine.style;
                Color color = dragLineColor;
                m_ContentDragLine.RegisterCallback<MouseEnterEvent>(
                    (evt) => dragLineStyle.backgroundColor = color
                );
                m_ContentDragLine.RegisterCallback<MouseUpEvent>(
                    (evt) =>
                    {
                        float rootWidth = rootVisualElement.worldBound.width;
                        float leftPaneMinWidth = m_AssetList.style.minWidth.value.value;
                        float rightPaneMinWidth = m_PreviewPane.style.minWidth.value.value;
                        float dragLinePos = dragLineStyle.left.value.value;
                        if (
                            dragLinePos < leftPaneMinWidth
                            || dragLinePos > rootWidth - rightPaneMinWidth
                        )
                        {
                            dragLinePos = leftPaneMinWidth;
                            dragLineStyle.left = dragLinePos;
                            m_ContentSplitView.fixedPaneInitialDimension = dragLinePos;
                        }
                        ProjectPinBoardSettings.dragLinePos = dragLinePos;
                    }
                );
            }

            InitListView();
            InitContentPreview();
        }

        private void OnContentGeometryChangedEventChanged(GeometryChangedEvent evt)
        {
            bool isNarrow = (m_Toolbar.localBound.width <= 250);
            TogglePreview(!isNarrow && ProjectPinBoardSettings.enablePreview);
        }

        private bool IsContentInited()
        {
            return (m_Content != null);
        }

        #endregion

        private void UpdateContent()
        {
            if (!IsContentInited())
                return;

            ClearAssetListSelection();

            UpdateAssetList();

            if (!string.IsNullOrEmpty(m_FirstSelectedItemGuid) && m_AssetListData.Count > 0)
            {
                SetAssetListSelection(m_FirstSelectedItemGuid);
                SetPreview(ProjectPinBoardData.GetItem(m_FirstSelectedItemGuid));
            }

            m_ContentPlaceholder.style.display = (
                m_AssetListData.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None
            );
            m_ContentSplitView.style.display = (
                m_AssetListData.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex
            );
        }
    }
}
