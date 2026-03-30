using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private void ShowTogglePopup(
            string headline,
            Vector2 pos,
            List<string> labels,
            string curLabel,
            Action<string> toggleCallback
        )
        {
            VisualElement popupMask = new VisualElement()
            {
                name = "TogglePopupMask",
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                }
            };
            rootVisualElement.Add(popupMask);

            const int borderRadius = 8;
            PopupWindow popupWindow = new PopupWindow()
            {
                name = "TogglePopup",
                text = headline,
                style =
                {
                    position = Position.Absolute,
                    borderTopLeftRadius = borderRadius,
                    borderTopRightRadius = borderRadius,
                    borderBottomLeftRadius = borderRadius,
                    borderBottomRightRadius = borderRadius,
                }
            };
            rootVisualElement.Add(popupWindow);

            void UpdateTransform()
            {
                Rect rootBound = rootVisualElement.worldBound;
                const float minWidth = 150;
                const float maxWidth = 200;
                const float marginRight = 15;
                const float marginBottom = 15;
                float top = pos.y;
                float left = pos.x;
                float width = popupWindow.worldBound.width;
                if (left + width >= rootBound.width - marginRight)
                {
                    left = (rootBound.width - width - marginRight);
                }
                float maxHeight = (rootBound.height - top - marginBottom);
                popupWindow.style.minWidth = minWidth;
                popupWindow.style.maxWidth = maxWidth;
                popupWindow.style.maxHeight = maxHeight;
                popupWindow.style.top = top;
                popupWindow.style.left = left;
            }

            rootVisualElement.RegisterCallback<GeometryChangedEvent>((evt) => UpdateTransform());
            popupWindow.RegisterCallback<GeometryChangedEvent>((evt) => UpdateTransform());

            UpdateTransform();

            void CloseTogglePopup()
            {
                popupWindow.RemoveFromHierarchy();
                popupMask.RemoveFromHierarchy();
            }

            popupMask.RegisterCallback<ClickEvent>((evt) => CloseTogglePopup());

            popupWindow.RegisterCallback<GeometryChangedEvent>((evt) => popupWindow.BringToFront());

            void OnToggleValueChanged(ChangeEvent<bool> evt)
            {
                Toggle toggle = (Toggle)evt.target;
                string value = (string)toggle.userData;
                toggleCallback(value);
                CloseTogglePopup();
            }

            Toggle GenToggle(string label, bool isOn)
            {
                Toggle toggle = new Toggle()
                {
                    label = label,
                    value = isOn,
                    userData = label,
                    style =
                    {
                        minHeight = 16,
                        marginLeft = 0,
                        marginRight = 0,
                        paddingLeft = 1,
                        paddingRight = 1,
                        flexGrow = 1,
                    }
                };
                toggle.AddToClassList("Toggle");
                {
                    Label labelElement = toggle.labelElement;
                    labelElement.style.flexGrow = 1;
                    labelElement.style.flexShrink = 1;
                    labelElement.style.minWidth = StyleKeyword.Auto;
                    labelElement.style.whiteSpace = WhiteSpace.Normal;
                }
                {
                    VisualElement checkmarkElement = toggle
                        .Q<VisualElement>("unity-checkmark")
                        .parent;
                    checkmarkElement.style.flexGrow = 0;
                    checkmarkElement.style.marginLeft = 5;
                }
                toggle.RegisterValueChangedCallback(OnToggleValueChanged);
                return toggle;
            }

            if (labels.Count > 0)
            {
                ScrollView scrollView = new ScrollView();
                popupWindow.Add(scrollView);
                foreach (string label in labels)
                {
                    bool isOn = label.Equals(curLabel, StringComparison.OrdinalIgnoreCase);
                    Toggle toggle = GenToggle(label, isOn);
                    scrollView.Add(toggle);
                }
            }
            else
            {
                popupWindow.Add(
                    new Label()
                    {
                        name = "Placeholder",
                        text = "Empty...",
                        style =
                        {
                            color = new Color(128 / 255f, 128 / 255f, 128 / 255f, 1f),
                            unityTextAlign = TextAnchor.MiddleCenter,
                        }
                    }
                );
            }
        }
    }
}
