using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        #region Initialization

        private VisualElement m_PreviewPane = null;

        private VisualElement m_PreviewPlaceholder = null;

        private ScrollView m_PreviewScrollView = null;

        private Image m_PreviewIcon = null;

        private Label m_PreviewName = null;

        private Label m_PreviewDisplayName = null;

        private VisualElement m_PreviewSeparator = null;

        private PreviewItem m_PreviewGuidItem = null;

        private PreviewItem m_PreviewTypeItem = null;

        private PreviewItem m_PreviewAssetBundleItem = null;

        private PreviewItem m_PreviewPathItem = null;

        private VisualElement m_PreviewTagContainer = null;

        private VisualElement m_PreviewFloatingButtonContainer = null;

        private void InitContentPreview()
        {
            m_PreviewPane = new VisualElement()
            {
                name = "ContentPreview",
                style = { flexBasis = Length.Percent(100), minWidth = 100, }
            };
            m_ContentSplitView.Add(m_PreviewPane);

            m_PreviewPlaceholder = new VisualElement()
            {
                name = "Placeholder",
                style =
                {
                    flexBasis = Length.Percent(100),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_PreviewPane.Add(m_PreviewPlaceholder);
            m_PreviewPlaceholder.Add(
                new Label()
                {
                    text = "Preview",
                    style =
                    {
                        fontSize = 22,
                        color = new Color(128 / 255f, 128 / 255f, 128 / 255f, 0.8f),
                        unityFontStyleAndWeight = FontStyle.Bold,
                    }
                }
            );

            m_PreviewScrollView = new ScrollView()
            {
                name = "ScrollView",
                style =
                {
                    display = DisplayStyle.None,
                    flexBasis = Length.Percent(100),
                    paddingTop = 10,
                    paddingBottom = 0,
                    minWidth = 100,
                    flexGrow = 1,
                }
            };
            m_PreviewPane.Add(m_PreviewScrollView);
            m_PreviewScrollView.contentContainer.style.flexGrow = 1;

            m_PreviewIcon = new Image
            {
                name = "Icon",
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    alignSelf = Align.Center,
                    width = Length.Percent(100),
                    height = 100,
                    paddingLeft = 5,
                    paddingRight = 5,
                },
            };
            m_PreviewScrollView.Add(m_PreviewIcon);
            m_PreviewName = new Label()
            {
                name = "Name",
                style =
                {
                    minHeight = 30,
                    paddingLeft = 5,
                    paddingRight = 5,
                    marginTop = 5,
                    fontSize = 15,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            m_PreviewScrollView.Add(m_PreviewName);
            m_PreviewName.AddManipulator(new ContextualMenuManipulator(CopyNameMenuBuilder));
            m_PreviewDisplayName = new Label()
            {
                name = "DisplayName",
                style =
                {
                    paddingLeft = 5,
                    paddingRight = 5,
                    marginTop = 0,
                    marginBottom = 5,
                    fontSize = 12,
                    color = new Color(128 / 255f, 128 / 255f, 128 / 255f, 1f),
                    unityFontStyleAndWeight = FontStyle.Italic,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            m_PreviewScrollView.Add(m_PreviewDisplayName);

            m_PreviewSeparator = GenHorizontalSeparator();
            m_PreviewScrollView.Add(m_PreviewSeparator);

            m_PreviewGuidItem = new PreviewItem() { name = "GUID", };
            m_PreviewScrollView.Add(m_PreviewGuidItem);
            m_PreviewGuidItem.AddManipulator(new ContextualMenuManipulator(CopyGuidMenuBuilder));

            m_PreviewTypeItem = new PreviewItem() { name = "Type", };
            m_PreviewScrollView.Add(m_PreviewTypeItem);
            m_PreviewTypeItem.AddManipulator(new ContextualMenuManipulator(CopyTypeMenuBuilder));

            m_PreviewAssetBundleItem = new PreviewItem() { name = "AssetBundle", };
            m_PreviewScrollView.Add(m_PreviewAssetBundleItem);
            m_PreviewAssetBundleItem.AddManipulator(
                new ContextualMenuManipulator(CopyAssetBundleMenuBuilder)
            );

            m_PreviewPathItem = new PreviewItem() { name = "Path", };
            m_PreviewScrollView.Add(m_PreviewPathItem);
            m_PreviewPathItem.AddManipulator(new ContextualMenuManipulator(CopyPathMenuBuilder));

            m_PreviewTagContainer = new VisualElement()
            {
                name = "Tags",
                style =
                {
                    minHeight = 25,
                    paddingLeft = 0,
                    paddingRight = 5,
                    marginTop = 5,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                },
            };
            m_PreviewScrollView.Add(m_PreviewTagContainer);

            m_PreviewFloatingButtonContainer = new VisualElement()
            {
                name = "FloatingButtons",
                style =
                {
                    display = DisplayStyle.None,
                    flexShrink = 0,
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5,
                    borderTopWidth = 1,
                    backgroundColor = PipiUtility.EditorBackgroundColor,
                    borderTopColor = separatorColor,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,
                },
            };
            m_PreviewPane.Add(m_PreviewFloatingButtonContainer);
            {
                Button selectButton = new Button()
                {
                    name = "SelectButton",
                    text = "Select",
                    style = { height = 20, }
                };
                m_PreviewFloatingButtonContainer.Add(selectButton);
                selectButton.clicked += OnPreviewSelectButtonClick;
                Button openButton = new Button()
                {
                    name = "OpenButton",
                    text = "Open",
                    style = { height = 20, }
                };
                m_PreviewFloatingButtonContainer.Add(openButton);
                openButton.clicked += OnPreviewOpenButtonClick;
                Button showInExplorerButton = new Button()
                {
                    name = "ShowInExplorerButton",
                    text = "Show In Explorer",
                    style = { height = 20, }
                };
                m_PreviewFloatingButtonContainer.Add(showInExplorerButton);
                showInExplorerButton.clicked += OnPreviewShowInExplorerButtonClick;
            }
        }

        private void CopyGuidMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (!(m_PreviewPane.userData is ItemInfo itemInfo))
                return;
            evt.menu.AppendAction(
                "Copy GUID",
                CopyTextAction,
                DropdownMenuAction.AlwaysEnabled,
                itemInfo.guid
            );
        }

        private void CopyTypeMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (!(m_PreviewPane.userData is ItemInfo itemInfo))
                return;
            evt.menu.AppendAction(
                "Copy Type",
                CopyTextAction,
                DropdownMenuAction.AlwaysEnabled,
                itemInfo.Type
            );
        }

        private void CopyNameMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (!(m_PreviewPane.userData is ItemInfo itemInfo))
                return;
            evt.menu.AppendAction(
                "Copy Name",
                CopyTextAction,
                DropdownMenuAction.AlwaysEnabled,
                itemInfo.Name
            );
        }

        private void CopyAssetBundleMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (!(m_PreviewPane.userData is ItemInfo itemInfo))
                return;
            evt.menu.AppendAction(
                "Copy AssetBundle",
                CopyTextAction,
                DropdownMenuAction.AlwaysEnabled,
                itemInfo.AssetBundle
            );
        }

        private void CopyPathMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (!(m_PreviewPane.userData is ItemInfo itemInfo))
                return;
            evt.menu.AppendAction(
                "Copy Path",
                CopyTextAction,
                DropdownMenuAction.AlwaysEnabled,
                itemInfo.Path
            );
        }

        private void CopyTextAction(DropdownMenuAction action)
        {
            PipiUtility.SaveToClipboard((string)action.userData);
        }

        private void OnPreviewSelectButtonClick()
        {
            if (m_PreviewPane.userData == null)
                return;
            ItemInfo itemInfo = (ItemInfo)m_PreviewPane.userData;
            PipiUtility.FocusOnAsset(itemInfo.guid);
        }

        private void OnPreviewOpenButtonClick()
        {
            if (m_PreviewPane.userData == null)
                return;
            ItemInfo itemInfo = (ItemInfo)m_PreviewPane.userData;
            PipiUtility.OpenAsset(itemInfo.guid);
        }

        private void OnPreviewShowInExplorerButtonClick()
        {
            if (m_PreviewPane.userData == null)
                return;
            ItemInfo itemInfo = (ItemInfo)m_PreviewPane.userData;
            PipiUtility.ShowInExplorer(itemInfo.guid);
        }

        #endregion

        #region Interface

        public void TogglePreview(bool enable)
        {
            if (enable)
            {
                float rootWidth = rootVisualElement.worldBound.width;
                float previewMinWidth = m_PreviewPane.style.minWidth.value.value;
                if (ProjectPinBoardSettings.dragLinePos > rootWidth - previewMinWidth)
                {
                    m_ContentSplitView.fixedPaneInitialDimension = rootWidth - previewMinWidth;
                }
                else
                {
                    m_ContentSplitView.fixedPaneInitialDimension =
                        ProjectPinBoardSettings.dragLinePos;
                }
                m_ContentSplitView.UnCollapse();
            }
            else
            {
                m_ContentSplitView.CollapseChild(1);
            }
        }

        public void ClearPreview()
        {
            m_PreviewPane.userData = null;

            m_PreviewPlaceholder.style.display = DisplayStyle.Flex;
            m_PreviewScrollView.style.display = DisplayStyle.None;

            m_PreviewIcon.image = null;
            m_PreviewName.text = string.Empty;
            m_PreviewDisplayName.text = string.Empty;
            m_PreviewDisplayName.style.display = DisplayStyle.None;

            m_PreviewGuidItem.titleLabel.text = string.Empty;
            m_PreviewGuidItem.contentLabel.text = string.Empty;

            m_PreviewTypeItem.titleLabel.text = string.Empty;
            m_PreviewTypeItem.contentLabel.text = string.Empty;

            m_PreviewAssetBundleItem.titleLabel.text = string.Empty;
            m_PreviewAssetBundleItem.contentLabel.text = string.Empty;

            m_PreviewPathItem.titleLabel.text = string.Empty;
            m_PreviewPathItem.contentLabel.text = string.Empty;

            m_PreviewTagContainer.Clear();

            m_PreviewFloatingButtonContainer.style.display = DisplayStyle.None;
        }

        private void SetPreview(ItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                ClearPreview();
                return;
            }

            m_PreviewPane.userData = itemInfo;
            m_PreviewPlaceholder.style.display = DisplayStyle.None;
            m_PreviewScrollView.style.display = DisplayStyle.Flex;

            string path = itemInfo.Path;
            Object asset = itemInfo.Asset;
            if (asset is Texture texture)
            {
                m_PreviewIcon.image = texture;
            }
            else
            {
                m_PreviewIcon.image = (
                    asset ? AssetDatabase.GetCachedIcon(path) : PipiUtility.GetAssetIcon(path)
                );
            }
            m_PreviewName.text = (asset ? asset.name : "<Missing Asset>");
            string displayName = itemInfo.displayName;
            m_PreviewDisplayName.style.display = (
                string.IsNullOrWhiteSpace(displayName) ? DisplayStyle.None : DisplayStyle.Flex
            );
            if (!string.IsNullOrWhiteSpace(displayName))
                m_PreviewDisplayName.text = displayName;

            m_PreviewGuidItem.titleLabel.text = "GUID:";
            m_PreviewGuidItem.contentLabel.text = itemInfo.guid;
            m_PreviewTypeItem.titleLabel.text = "Type:";
            m_PreviewTypeItem.contentLabel.text = itemInfo.Type;
            m_PreviewAssetBundleItem.titleLabel.text = "AssetBundle:";
            m_PreviewAssetBundleItem.contentLabel.text = (
                asset
                    ? (
                        !string.IsNullOrEmpty(itemInfo.AssetBundle)
                            ? itemInfo.AssetBundle
                            : "<None>"
                    )
                    : string.Empty
            );
            m_PreviewPathItem.titleLabel.text = "Path:";
            m_PreviewPathItem.contentLabel.text = path;
            m_PreviewTagContainer.Clear();
            foreach (string tag in itemInfo.tags)
            {
                m_PreviewTagContainer.Add(GenTagLabel(tag));
            }

            m_PreviewFloatingButtonContainer.style.display = DisplayStyle.Flex;
        }

        #endregion

        #region PreviewItem

        private class PreviewItem : VisualElement
        {
            public readonly Label titleLabel = null;

            public readonly Label contentLabel = null;

            public PreviewItem(string title = "", string content = "")
            {
                this.style.paddingLeft = 5;
                this.style.paddingRight = 5;
                this.style.marginTop = 5;
                this.style.flexDirection = FlexDirection.Row;
                titleLabel = new Label()
                {
                    text = title,
                    style =
                    {
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginRight = 5,
                        fontSize = 11,
                        unityFontStyleAndWeight = FontStyle.Normal,
                        unityTextAlign = TextAnchor.UpperLeft,
                        whiteSpace = WhiteSpace.Normal,
                    },
                };
                this.Add(titleLabel);
                contentLabel = new Label()
                {
                    text = content,
                    style =
                    {
                        paddingLeft = 0,
                        paddingRight = 0,
                        flexShrink = 1,
                        fontSize = 11,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        unityTextAlign = TextAnchor.UpperLeft,
                        whiteSpace = WhiteSpace.Normal,
                    },
                };
                this.Add(contentLabel);
            }
        }

        #endregion
    }
}
