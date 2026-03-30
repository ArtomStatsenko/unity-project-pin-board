using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private const string k_DragAndDropGenericType = "ProjectPinBoard-ListItem";

        #region Dragging

        private void TriggerDragging()
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(k_DragAndDropGenericType, this);
            DragAndDrop.StartDrag("Assets");

            List<Object> assets = new List<Object>();
            object[] itemInfos = m_AssetList.selectedItems.ToArray();
            foreach (ItemInfo itemInfo in itemInfos)
            {
                Object asset = itemInfo.Asset;
                if (asset)
                    assets.Add(asset);
            }
            DragAndDrop.objectReferences = assets.ToArray();
        }

        #endregion

        #region Drop Area

        private VisualElement m_DropArea = null;

        private VisualElement m_DropTip = null;

        private void InitDropArea()
        {
            m_DropArea = new VisualElement()
            {
                name = "DropArea",
                style =
                {
                    display = DisplayStyle.None,
                    position = Position.Absolute,
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    backgroundColor = dropTipBgColor,
                }
            };
            rootVisualElement.Add(m_DropArea);

            const int dropTipBorderWidth = 2;
            float dropTipMarginTop = m_Toolbar.style.height.value.value;
            m_DropTip = new VisualElement()
            {
                name = "DropTip",
                style =
                {
                    display = DisplayStyle.None,
                    flexBasis = Length.Percent(100),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    borderTopWidth = dropTipBorderWidth,
                    borderBottomWidth = dropTipBorderWidth,
                    borderLeftWidth = dropTipBorderWidth,
                    borderRightWidth = dropTipBorderWidth,
                    borderTopColor = dropTipBorderColor,
                    borderBottomColor = dropTipBorderColor,
                    borderLeftColor = dropTipBorderColor,
                    borderRightColor = dropTipBorderColor,
                    marginTop = dropTipMarginTop,
                }
            };
            m_DropArea.Add(m_DropTip);
            Label label = new Label()
            {
                name = "Label",
                text = "Drop to Pin",
                style =
                {
                    paddingLeft = 10,
                    paddingRight = 10,
                    fontSize = 40,
                    color = dropTipTextColor,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal,
#if UNITY_2021_1_OR_NEWER
                    unityTextOutlineColor = new Color(0f, 0f, 0f, 1f),
                    unityTextOutlineWidth = 1,
#endif
                }
            };
            m_DropTip.Add(label);

            m_DropArea.RegisterCallback<DragEnterEvent>(OnDragEnter);
            m_DropArea.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            m_DropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            m_DropArea.RegisterCallback<DragPerformEvent>(OnDragPerform);
            m_DropArea.RegisterCallback<DragExitedEvent>(OnDragExited);
            {
                rootVisualElement.panel?.visualTree.RegisterCallback<DragExitedEvent>(OnDragExited);

                m_DropArea.RegisterCallback<AttachToPanelEvent>(
                    (evt) =>
                    {
                        evt.destinationPanel
                            .visualTree
                            .RegisterCallback<DragExitedEvent>(OnDragExited);
                    }
                );
                m_DropArea.RegisterCallback<DetachFromPanelEvent>(
                    (evt) =>
                    {
                        evt.originPanel
                            .visualTree
                            .UnregisterCallback<DragExitedEvent>(OnDragExited);
                    }
                );
            }

            rootVisualElement.RegisterCallback<DragEnterEvent>((evt) => EnableDropArea());
            rootVisualElement.RegisterCallback<DragLeaveEvent>((evt) => DisableDropArea());
            rootVisualElement.RegisterCallback<DragExitedEvent>((evt) => DisableDropArea());
        }

        private void EnableDropArea()
        {
            m_DropArea.style.display = DisplayStyle.Flex;
        }

        private void DisableDropArea()
        {
            m_DropArea.style.display = DisplayStyle.None;
        }

        private void ShowDropTip()
        {
            m_DropTip.style.display = DisplayStyle.Flex;
        }

        private void HideDropTip()
        {
            m_DropTip.style.display = DisplayStyle.None;
        }

        private const string k_InvalidGUID = "00000000000000000000000000000000";

        private bool CanDrop()
        {
            object genericData = DragAndDrop.GetGenericData(k_DragAndDropGenericType);
            if (genericData != null && (ProjectPinBoardWindow)genericData == this)
            {
                return false;
            }
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    obj,
                    out string guid,
                    out long localId
                );
                if (string.IsNullOrEmpty(guid) || guid.Equals(k_InvalidGUID) || localId == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            if (!CanDrop())
                return;

            ShowDropTip();
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            if (!CanDrop())
                return;

            HideDropTip();
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            if (!CanDrop())
                return;

            HideDropTip();
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (CanDrop())
            {
                ShowDropTip();
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            if (!CanDrop())
                return;

            foreach (Object obj in DragAndDrop.objectReferences)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    obj,
                    out string guid,
                    out long localId
                );
                if (guid != null && localId != 0)
                {
                    ProjectPinBoardManager.Pin(guid);
                }
            }
        }

        #endregion
    }
}
