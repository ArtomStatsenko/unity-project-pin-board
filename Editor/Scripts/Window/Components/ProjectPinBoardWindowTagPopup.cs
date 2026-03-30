using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private VisualElement m_TagPopupMask = null;

        private PopupWindow m_TagPopupWindow = null;

        private void CloseTagPopup()
        {
            m_TagPopupWindow.userData = null;

            Box allTagsContainer = m_TagPopupWindow.Q<Box>("AllTags");
            foreach (VisualElement element in allTagsContainer.Children().ToArray())
            {
                if (element is Label)
                    element.RemoveFromHierarchy();
            }

            Box itemTagsContainer = m_TagPopupWindow.Q<Box>("ItemTags");
            foreach (VisualElement element in itemTagsContainer.Children().ToArray())
            {
                if (element is Label)
                    element.RemoveFromHierarchy();
            }
            ((List<string>)itemTagsContainer.userData)?.Clear();

            m_TagPopupWindow.style.display = DisplayStyle.None;
            m_TagPopupMask.style.display = DisplayStyle.None;
        }

        private void ShowTagPopup(ItemInfo[] itemInfos)
        {
            if (m_TagPopupMask == null)
            {
                m_TagPopupMask = new VisualElement()
                {
                    name = "TagPopupMask",
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        bottom = 0,
                        left = 0,
                        right = 0,
                        backgroundColor = new Color(0f, 0f, 0f, 0.3f),
                    }
                };
                rootVisualElement.Add(m_TagPopupMask);
                m_TagPopupMask.RegisterCallback<ClickEvent>((evt) => CloseTagPopup());
            }
            else
            {
                m_TagPopupMask.style.display = DisplayStyle.Flex;
                m_TagPopupMask.BringToFront();
            }

            if (m_TagPopupWindow == null)
            {
                Label GenSubTitleLabel(string text)
                {
                    return new Label()
                    {
                        name = "SubTitle",
                        text = text,
                        style =
                        {
                            paddingLeft = 0,
                            paddingRight = 0,
                            marginTop = 0,
                            marginLeft = 5,
                            marginRight = 5,
                            unityFontStyleAndWeight = FontStyle.Normal,
                            whiteSpace = WhiteSpace.Normal,
                        }
                    };
                }

                Color tagContainerBorderColor = new Color(88 / 255f, 88 / 255f, 88 / 255f, 0.5f);
                const int tagContainerBorderWidth = 1;
                const int tagContainerBorderRadius = 5;

                Box GenTagContainer(string name)
                {
                    return new Box()
                    {
                        name = name,
                        style =
                        {
                            minHeight = 32,
                            paddingTop = 0,
                            paddingBottom = 5,
                            paddingLeft = 0,
                            paddingRight = 5,
                            marginTop = 2,
                            marginLeft = 3,
                            marginRight = 3,
                            borderTopWidth = tagContainerBorderWidth,
                            borderBottomWidth = tagContainerBorderWidth,
                            borderLeftWidth = tagContainerBorderWidth,
                            borderRightWidth = tagContainerBorderWidth,
                            borderTopColor = tagContainerBorderColor,
                            borderBottomColor = tagContainerBorderColor,
                            borderLeftColor = tagContainerBorderColor,
                            borderRightColor = tagContainerBorderColor,
                            borderTopLeftRadius = tagContainerBorderRadius,
                            borderTopRightRadius = tagContainerBorderRadius,
                            borderBottomLeftRadius = tagContainerBorderRadius,
                            borderBottomRightRadius = tagContainerBorderRadius,
                            flexDirection = FlexDirection.Row,
                            flexWrap = Wrap.Wrap,
                            flexGrow = 1,
                            flexShrink = 0,
                        },
                    };
                }

                m_TagPopupWindow = new PopupWindow()
                {
                    name = "TagPopup",
                    focusable = true,
                    style =
                    {
                        position = Position.Absolute,
                        paddingTop = 5,
                        paddingBottom = 10,
                        backgroundColor = popupBgColor,
                        borderTopColor = borderColor,
                        borderBottomColor = borderColor,
                        borderLeftColor = borderColor,
                        borderRightColor = borderColor,
                        borderTopLeftRadius = 8,
                        borderTopRightRadius = 8,
                        borderBottomLeftRadius = 8,
                        borderBottomRightRadius = 8,
                        flexDirection = FlexDirection.Column,
                    }
                };
                rootVisualElement.Add(m_TagPopupWindow);

                m_TagPopupWindow.contentContainer.style.paddingTop = 4;

                m_TagPopupWindow.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        OnConfirmButtonClick();
                    }
                    else if (evt.keyCode == KeyCode.Escape)
                    {
                        OnCancelButtonClick();
                    }
                });

                void UpdateTransform()
                {
                    Rect rootBound = rootVisualElement.worldBound;
                    const float width = 300;
                    const float top = 50f;
                    const float marginBottom = 20;
                    float left = (rootBound.width / 2f) - (width / 2f);
                    float maxHeight = (rootBound.height - top - marginBottom);
                    m_TagPopupWindow.style.width = width;
                    m_TagPopupWindow.style.maxHeight = maxHeight;
                    m_TagPopupWindow.style.top = top;
                    m_TagPopupWindow.style.left = left;
                }

                rootVisualElement.RegisterCallback<GeometryChangedEvent>(
                    (evt) => UpdateTransform()
                );

                UpdateTransform();

                m_TagPopupWindow.Add(
                    new Label()
                    {
                        name = "Title",
                        style =
                        {
                            marginBottom = 10,
                            unityTextAlign = TextAnchor.MiddleCenter,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            whiteSpace = WhiteSpace.Normal,
                        }
                    }
                );

                ScrollView scrollView = new ScrollView();
                m_TagPopupWindow.Add(scrollView);

                {
                    scrollView.Add(GenSubTitleLabel("Select Existing Tags (Click to add)"));
                    Box allTagsContainer = GenTagContainer("AllTags");
                    scrollView.Add(allTagsContainer);
                    allTagsContainer.Add(new VisualElement() { name = "Placeholder" });
                }

                scrollView.Add(GenHorizontalSeparator(8));

                {
                    scrollView.Add(GenSubTitleLabel("Current Tags (Click to remove)"));
                    Box itemTagsContainer = GenTagContainer("ItemTags");
                    itemTagsContainer.userData = new List<string>();
                    scrollView.Add(itemTagsContainer);
                    itemTagsContainer.Add(new VisualElement() { name = "Placeholder" });
                }

                scrollView.Add(GenHorizontalSeparator(8));

                {
                    scrollView.Add(GenSubTitleLabel("Add New Tag (Use ',' to separate)"));
                    VisualElement container = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            minHeight = 30,
                            maxHeight = 50,
                            marginTop = 2,
                        }
                    };
                    scrollView.Add(container);
                    TextField tagTextField = new TextField()
                    {
                        name = "TagTextField",
                        multiline = true,
                        style =
                        {
                            flexGrow = 1,
                            flexShrink = 1,
                            marginTop = 0,
                            marginBottom = 0,
                            unityTextAlign = TextAnchor.UpperLeft,
                            whiteSpace = WhiteSpace.Normal,
                        }
                    };
                    {
                        VisualElement textInput = tagTextField.Q<VisualElement>("unity-text-input");
                        textInput.style.unityTextAlign = TextAnchor.UpperLeft;
                    }
                    container.Add(tagTextField);
                    tagTextField.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            if (string.IsNullOrEmpty(tagTextField.value))
                            {
                                OnConfirmButtonClick();
                            }
                            else
                            {
                                OnAddTagButtonClick();
                            }
                            evt.StopPropagation();
                        }
                    });
                    Button addTagButton = new Button()
                    {
                        name = "AddTagButton",
                        text = "Add",
                        style =
                        {
                            width = 60,
                            marginTop = 0,
                            marginBottom = 0,
                            marginLeft = 0,
                        }
                    };
                    container.Add(addTagButton);
                    addTagButton.clicked += OnAddTagButtonClick;
                }

                scrollView.Add(GenHorizontalSeparator(8));

                {
                    VisualElement mainButtons = new VisualElement()
                    {
                        name = "MainButtons",
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            minHeight = 30,
                            flexShrink = 0,
                        }
                    };
                    scrollView.Add(mainButtons);
                    Button cancelButton = new Button()
                    {
                        name = "CancelButton",
                        text = "Cancel",
                        style =
                        {
                            height = 30,
                            flexGrow = 1,
                            unityFontStyleAndWeight = FontStyle.Bold,
                        }
                    };
                    mainButtons.Add(cancelButton);
                    cancelButton.clicked += OnCancelButtonClick;
                    Button confirmButton = new Button()
                    {
                        name = "ConfirmButton",
                        text = "Confirm",
                        style =
                        {
                            height = 30,
                            flexGrow = 1,
                            unityFontStyleAndWeight = FontStyle.Bold,
                        }
                    };
                    mainButtons.Add(confirmButton);
                    confirmButton.clicked += OnConfirmButtonClick;
                }
            }
            else
            {
                m_TagPopupWindow.style.display = DisplayStyle.Flex;
                m_TagPopupWindow.BringToFront();
            }

            ItemInfo[] GetEditingItemInfos()
            {
                return (ItemInfo[])m_TagPopupWindow.userData;
            }

            List<string> GetSelectedItemTags()
            {
                return (List<string>)m_TagPopupWindow.Q<Box>("ItemTags").userData;
            }

            void SetTitle(string text, string tooltip = "")
            {
                Label titleLabel = m_TagPopupWindow.Q<Label>("Title");
                titleLabel.text = text;
                titleLabel.tooltip = tooltip;
            }

            void RefreshAllTagsContainer(List<string> tags)
            {
                Box allTagsContainer = m_TagPopupWindow.Q<Box>("AllTags");

                foreach (VisualElement element in allTagsContainer.Children().ToArray())
                {
                    if (element is Label)
                        element.RemoveFromHierarchy();
                }

                foreach (string tag in tags)
                {
                    Label label = GenTagLabel(
                        tag,
                        () =>
                        {
                            AddTag(tag);
                            FocusToTagTextField();
                        }
                    );
                    label.AddToClassList("Addable");
                    allTagsContainer.Add(label);
                }
            }

            void RefreshItemTagsContainer(List<string> tags)
            {
                Box itemTagsContainer = m_TagPopupWindow.Q<Box>("ItemTags");

                List<string> itemTags = (List<string>)itemTagsContainer.userData;
                itemTags.Clear();
                tags.ForEach(v => itemTags.Add(v));

                foreach (VisualElement element in itemTagsContainer.Children().ToArray())
                {
                    if (element is Label)
                        element.RemoveFromHierarchy();
                }

                foreach (string tag in itemTags)
                {
                    Label label = GenTagLabel(tag, () => RemoveTag(tag));
                    label.AddToClassList("Removable");
                    itemTagsContainer.Add(label);
                }
            }

            void SetTagTextFieldValue(string text)
            {
                TextField tagTextField = m_TagPopupWindow.Q<TextField>("TagTextField");
                tagTextField.value = text;
            }

            void FocusToTagTextField()
            {
                TextField tagTextField = m_TagPopupWindow.Q<TextField>("TagTextField");
                if (m_TagPopupWindow.focusController.focusedElement == tagTextField)
                {
                    tagTextField.Blur();
                    EditorApplication.delayCall += () =>
                    {
                        tagTextField.Focus();
                        tagTextField.SelectAll();
                    };
                }
                else
                {
                    tagTextField.Focus();
                    tagTextField.SelectAll();
                }
            }

            void OnAddTagButtonClick()
            {
                TextField tagTextField = m_TagPopupWindow.Q<TextField>("TagTextField");
                string[] texts = tagTextField
                    .value
                    .Split(new[] { ",", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<string> fails = new List<string>();
                foreach (string text in texts)
                {
                    if (!AddTag(text))
                        fails.Add(text);
                }
                tagTextField.value = fails.Join(", ");
                FocusToTagTextField();
            }

            void OnConfirmButtonClick()
            {
                List<string> itemTags = GetSelectedItemTags();
                foreach (ItemInfo itemInfo in GetEditingItemInfos())
                {
                    ProjectPinBoardManager.SetTags(itemInfo.guid, itemTags);
                }
                CloseTagPopup();
            }

            void OnCancelButtonClick()
            {
                CloseTagPopup();
            }

            bool AddTag(string tag)
            {
                tag = Regex.Replace(tag, @"\s", "");

                if (string.IsNullOrWhiteSpace(tag))
                    return false;
                if (!ProjectPinBoardManager.IsValidTag(tag))
                    return false;

                Box itemTagsContainer = m_TagPopupWindow.Q<Box>("ItemTags");
                List<string> itemTags = (List<string>)itemTagsContainer.userData;
                if (itemTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                {
                    ShowNotification($"Tag \"{tag}\" already exists!");
                    return false;
                }

                itemTags.Add(tag);
                Label label = GenTagLabel(
                    tag,
                    () =>
                    {
                        RemoveTag(tag);
                        FocusToTagTextField();
                    }
                );
                label.AddToClassList("Removable");
                itemTagsContainer.Add(label);

                return true;
            }

            void RemoveTag(string tag)
            {
                Box itemTagsContainer = m_TagPopupWindow.Q<Box>("ItemTags");

                List<string> itemTags = (List<string>)itemTagsContainer.userData;
                itemTags.Remove(tag);

                foreach (VisualElement element in itemTagsContainer.Children())
                {
                    if (!(element is Label label))
                        continue;
                    if (label.text == tag)
                    {
                        label.RemoveFromHierarchy();
                        break;
                    }
                }
            }

            m_TagPopupWindow.userData = itemInfos;

            if (itemInfos.Length == 1)
            {
                SetTitle($"Set tag(s) of \"{itemInfos[0].AssetName}\"", itemInfos[0].AssetName);
            }
            else
            {
                string tooltip = itemInfos.Select(o => $"- {o.AssetName}").ToArray().Join("\n");
                SetTitle($"Set tag(s) of items...", tooltip);
            }

            RefreshAllTagsContainer(m_ItemTagList);

            RefreshItemTagsContainer(
                itemInfos.Length == 1 ? itemInfos[0].tags : new List<string>()
            );

            SetTagTextFieldValue(string.Empty);

            FocusToTagTextField();
        }
    }
}
