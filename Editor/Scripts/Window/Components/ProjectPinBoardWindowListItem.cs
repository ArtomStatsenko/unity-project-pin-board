using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private class ListItem : VisualElement
        {
            public int index = -1;

            public new ItemInfo userData = null;

            public readonly Image topImage = null;

            public readonly Image iconImage = null;

            public readonly Label nameLabel = null;

            public readonly TextField nameTextField = null;

            public readonly ButtonWithIcon floatButton = null;

            public bool enableFloatButton = false;

            public event Action<ListItem> floatButtonClicked;

            public Func<ListItem, string, bool> renameCallback;

            public event Action<ListItem> dragged;

            public ListItem()
            {
                this.style.paddingTop = 0;
                this.style.paddingBottom = 0;
                this.style.paddingLeft = 0;
                this.style.paddingRight = 0;
                this.style.flexDirection = FlexDirection.Row;
                this.style.alignItems = Align.Center;

                VisualElement one = new VisualElement()
                {
                    name = "1",
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = 15,
                        height = Length.Percent(100),
                        marginLeft = 0,
                        flexShrink = 0,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                    },
                };
                this.Add(one);
                {
                    topImage = new Image
                    {
                        name = "Top",
                        image = PipiUtility.GetIcon("Download-Available"),
                        pickingMode = PickingMode.Ignore,
                        scaleMode = ScaleMode.ScaleToFit,
                        style = { display = DisplayStyle.None, width = 12, },
                    };
#if UNITY_2021_1_OR_NEWER
                    topImage.transform.rotation = Quaternion.Euler(0, 0, 180);
                    one.Add(topImage);
#else
                    VisualElement pivot = new VisualElement
                    {
                        name = "Pivot",
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            width = 0,
                            height = 0,
                            flexGrow = 1,
                            top = Length.Percent(100),
                            left = Length.Percent(0),
                            alignItems = Align.Center,
                            justifyContent = Justify.Center,
                        },
                        transform = { rotation = Quaternion.Euler(0, 0, 180) }
                    };
                    one.Add(pivot);
                    pivot.Add(topImage);
#endif
                }

                VisualElement two = new VisualElement()
                {
                    name = "2",
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        width = 17,
                        height = Length.Percent(100),
                        marginLeft = 0,
                        flexShrink = 0,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                    },
                };
                this.Add(two);
                {
                    iconImage = new Image()
                    {
                        name = "Icon",
                        pickingMode = PickingMode.Ignore,
                        scaleMode = ScaleMode.ScaleToFit,
                        style = { },
                    };
                    two.Add(iconImage);
                }

                VisualElement three = new VisualElement()
                {
                    name = "3",
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        height = Length.Percent(100),
                        marginLeft = 1,
                        flexGrow = 1,
                        alignItems = Align.Center,
                    },
                };
                this.Add(three);
                {
                    nameLabel = new Label()
                    {
                        name = "Name",
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            display = DisplayStyle.Flex,
                            flexGrow = 1,
                            height = 18,
                            unityTextAlign = TextAnchor.MiddleLeft,
                        },
                    };
                    three.Add(nameLabel);
                    floatButton = new ButtonWithIcon()
                    {
                        name = "FloatButton",
                        focusable = false,
                        style =
                        {
                            position = Position.Absolute,
                            right = 2,
                            width = 16,
                            height = 16,
                            display = DisplayStyle.None,
                        },
                    };
                    floatButton.clicked += OnFloatButtonClick;
                    three.Add(floatButton);
                    nameTextField = new TextField()
                    {
                        name = "NameTextField",
                        style =
                        {
                            display = DisplayStyle.None,
                            paddingLeft = 0,
                            marginTop = 1,
                            marginBottom = 1,
                            marginLeft = 0,
                            marginRight = 1,
                            minWidth = 100,
                        },
                    };
                    three.Add(nameTextField);
                }

                this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
                this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

                this.RegisterCallback<MouseDownEvent>(OnMouseDown);
                this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                this.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            #region Interface

            public void SetText(string text)
            {
                this.nameLabel.text = text;
            }

            public void SetTextFontStyle(StyleEnum<FontStyle> fontStyle)
            {
                this.nameLabel.style.unityFontStyleAndWeight = fontStyle;
            }

            public void SetIcon(Texture image)
            {
                this.iconImage.image = image;
            }

            public void SetTop(bool top)
            {
                this.topImage.style.display = top ? DisplayStyle.Flex : DisplayStyle.None;
            }

            #endregion

            #region Name TextField

            private void OnNameTextFieldFocusOut(FocusOutEvent evt)
            {
                ApplyNameInput();
            }

            private void OnNameTextFieldKeyDown(KeyDownEvent evt)
            {
                bool stopEvent = true;

                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    ApplyNameInput();
                }
                else if (evt.keyCode == KeyCode.Escape || evt.keyCode == KeyCode.F2)
                {
                    HideNameTextField();
                }
                else
                {
                    stopEvent = false;
                }

                if (stopEvent)
                {
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                }
            }

            public bool isShowingNameTextField =>
                (nameTextField.style.display == DisplayStyle.Flex);

            public void ShowNameTextField()
            {
                if (isShowingNameTextField)
                    return;
                nameLabel.style.display = DisplayStyle.None;
                nameTextField.style.display = DisplayStyle.Flex;
                nameTextField.value = userData.Name;
                nameTextField.tooltip = userData.Name;
                nameTextField.Focus();
                nameTextField.SelectAll();
                EditorApplication.delayCall += () =>
                {
                    nameTextField.RegisterCallback<KeyDownEvent>(OnNameTextFieldKeyDown);
                    nameTextField.RegisterCallback<FocusOutEvent>(OnNameTextFieldFocusOut);
                };
            }

            public void HideNameTextField()
            {
                if (!isShowingNameTextField)
                    return;
                nameTextField.UnregisterCallback<KeyDownEvent>(OnNameTextFieldKeyDown);
                nameTextField.UnregisterCallback<FocusOutEvent>(OnNameTextFieldFocusOut);
                nameLabel.style.display = DisplayStyle.Flex;
                nameTextField.style.display = DisplayStyle.None;
                nameTextField.value = string.Empty;
            }

            private void ApplyNameInput()
            {
                if (renameCallback == null || renameCallback.Invoke(this, nameTextField.value))
                {
                    HideNameTextField();
                }
            }

            #endregion

            #region Float Button

            private void OnFloatButtonClick()
            {
                floatButtonClicked?.Invoke(this);
            }

            private void OnMouseEnter(MouseEnterEvent evt)
            {
                if (!enableFloatButton || isShowingNameTextField)
                {
                    floatButton.style.display = DisplayStyle.None;
                }
                else
                {
                    floatButton.style.display = DisplayStyle.Flex;
                }
            }

            private void OnMouseLeave(MouseLeaveEvent evt)
            {
                floatButton.style.display = DisplayStyle.None;
            }

            #endregion

            #region Dragging

            private bool m_GotMouseDown = false;

            private void OnMouseDown(MouseDownEvent evt)
            {
                if (evt.target == this && evt.button == 0)
                {
                    m_GotMouseDown = true;
                }
            }

            private void OnMouseUp(MouseUpEvent evt)
            {
                if (m_GotMouseDown && evt.button == 0)
                {
                    m_GotMouseDown = false;
                }
            }

            private void OnMouseMove(MouseMoveEvent evt)
            {
                if (m_GotMouseDown && evt.pressedButtons == 1)
                {
                    m_GotMouseDown = false;
                    dragged?.Invoke(this);
                }
            }

            #endregion
        }
    }
}
