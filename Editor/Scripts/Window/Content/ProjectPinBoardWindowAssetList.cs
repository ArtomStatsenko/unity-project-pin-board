using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        #region Initialization

        private ListView m_AssetList = null;

        private string m_FirstSelectedItemGuid = null;

        private void InitListView()
        {
            m_AssetList = new ListView()
            {
                name = "ListView",
#if UNITY_2021_1_OR_NEWER
                fixedItemHeight = 18,
#else
                itemHeight = 18,
#endif
                selectionType = SelectionType.Multiple,
                makeItem = CreateListItem,
                bindItem = BindListItem,
                unbindItem = UnbindListItem,
                style = { flexBasis = Length.Percent(100), minWidth = 100, }
            };
            m_ContentSplitView.Add(m_AssetList);

            m_AssetList.onSelectionChange += OnAssetListSelectionChange;
            m_AssetList.onItemsChosen += OnAssetListItemsChosen;
        }

        private void OnAssetListSelectionChange(IEnumerable<object> objs)
        {
            object[] infos = objs as object[] ?? objs.ToArray();
            if (!infos.Any())
            {
                ClearPreview();
                return;
            }
            int index = infos
                .ToList()
                .FindIndex(o => ((ItemInfo)o).MatchGuid(m_FirstSelectedItemGuid));
            if (index >= 0)
            {
                return;
            }
            ItemInfo itemInfo = (ItemInfo)m_AssetList.selectedItems.First();
            m_FirstSelectedItemGuid = itemInfo.guid;
            SetPreview(itemInfo);
            if (ProjectPinBoardSettings.syncSelection)
            {
                PipiUtility.SelectAsset(itemInfo.guid);
                this.Focus();
            }
        }

        private void OnAssetListItemsChosen(IEnumerable<object> objs)
        {
            object[] infos = objs as object[] ?? objs.ToArray();
            if (!infos.Any())
            {
                return;
            }
            ItemInfo info = (ItemInfo)infos.First();
            string guid = info.guid;
            if (PipiUtility.CanOpenInEditor(guid) || PipiUtility.CanOpenInScriptEditor(guid))
            {
                PipiUtility.OpenAsset(guid);
            }
            else
            {
                PipiUtility.FocusOnAsset(guid);
            }
        }

        #endregion

        #region Interface

        private List<ItemInfo> m_AssetListData = new List<ItemInfo>();

        private void UpdateAssetList()
        {
            m_AssetListData.Clear();
            m_AssetListData.AddRange(ProjectPinBoardData.items);

            if (m_AssetListData.Count > 0)
            {
                Filter(ref m_AssetListData);
                Sort(ref m_AssetListData);
            }

            m_AssetList.itemsSource = m_AssetListData;
#if UNITY_2021_2_OR_NEWER
            m_AssetList.Rebuild();
#else
            m_AssetList.Refresh();
#endif
        }

        public void FocusToAssetList()
        {
            m_AssetList.Focus();
        }

        public void SetAssetListSelection(string guid, bool notify = true)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                ClearPreview();
                return;
            }
            int index = GetAssetListItemIndex(guid);
            if (index < 0)
            {
                ClearPreview();
                return;
            }
            m_AssetList.ScrollToItem(index);
            if (notify)
            {
                m_AssetList.SetSelection(new int[] { index });
            }
            else
            {
                m_AssetList.SetSelectionWithoutNotify(new int[] { index });
            }
        }

        private void ClearAssetListSelection(bool notify = true)
        {
            if (notify)
            {
                m_AssetList.ClearSelection();
            }
            else
            {
                m_AssetList.SetSelectionWithoutNotify(new int[] { });
            }
        }

        private ListItem GetAssetListItem(int index)
        {
#if UNITY_2021_1_OR_NEWER
            return (ListItem)m_AssetList.GetRootElementForIndex(index);
#else
            Type type = m_AssetList.GetType();
            FieldInfo fieldInfo = type.GetField(
                "m_ScrollView",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            if (fieldInfo == null)
            {
                return null;
            }
            ScrollView scrollView = (ScrollView)fieldInfo.GetValue(m_AssetList);
            if (scrollView == null)
            {
                return null;
            }
            VisualElement[] elements = scrollView.Children().ToArray();
            foreach (VisualElement element in elements)
            {
                ListItem listItem = (ListItem)element;
                if (listItem.index == index)
                {
                    return listItem;
                }
            }
            return null;
#endif
        }

        private ListItem GetAssetListItem(string guid)
        {
            int index = GetAssetListItemIndex(guid);
            return (index < 0 ? null : GetAssetListItem(index));
        }

        private ListItem GetSelectedAssetListItem()
        {
            return GetAssetListItem(m_AssetList.selectedIndex);
        }

        private int GetAssetListItemIndex(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return -1;
            }
            return m_AssetListData.FindIndex(v => v.MatchGuid(guid));
        }

        private ItemInfo[] GetSelectedItemInfos()
        {
            return m_AssetList.selectedItems.Select(o => (ItemInfo)o).ToArray();
        }

        private string[] GetSelectedItemGuids()
        {
            return m_AssetList.selectedItems.Select(o => ((ItemInfo)o).guid).ToArray();
        }

        #endregion

        #region ListItem

        private ListItem CreateListItem()
        {
            ListItem listItem = new ListItem();
            listItem.enableFloatButton = true;
            listItem.floatButton.SetIcon(PipiUtility.GetIcon("Record Off"));
            listItem.floatButton.tooltip = "Locate to asset";
            listItem.floatButtonClicked += OnListItemFloatButtonClicked;
            listItem.AddManipulator(new ContextualMenuManipulator(ItemMenuBuilder));
            return listItem;
        }

        private void OnListItemFloatButtonClicked(ListItem listItem)
        {
            if (listItem.userData == null)
                return;
            PipiUtility.SelectAsset(listItem.userData.guid);
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index >= m_AssetListData.Count)
            {
                element.RemoveFromHierarchy();
                return;
            }
            ItemInfo itemInfo = m_AssetListData[index];
            ListItem listItem = (ListItem)element;
            listItem.index = index;
            listItem.userData = itemInfo;
            string assetPath = AssetDatabase.GUIDToAssetPath(itemInfo.guid);
            if (itemInfo.IsValid())
            {
                const string displayNameSuffix = "^";
                string displayName = itemInfo.displayName;
                displayName = (
                    !string.IsNullOrWhiteSpace(displayName)
                        ? $"{displayName}{displayNameSuffix}"
                        : itemInfo.Name
                );
                listItem.SetText(displayName);
                listItem.SetIcon(AssetDatabase.GetCachedIcon(assetPath));
            }
            else
            {
                const string missingAssetName = "<Missing Asset>";
                listItem.SetText(missingAssetName);
                listItem.SetIcon(PipiUtility.GetAssetIcon(assetPath));
            }
            listItem.SetTop(itemInfo.top);
            listItem.renameCallback += OnListItemRenamed;
            listItem.dragged += OnListItemDragged;
            listItem.RegisterCallback<MouseDownEvent>(OnListItemMouseDown);
        }

        private bool OnListItemRenamed(ListItem listItem, string newName)
        {
            ItemInfo itemInfo = listItem.userData;
            if (string.IsNullOrWhiteSpace(newName) || newName.Equals(itemInfo.AssetName))
            {
                ProjectPinBoardManager.RemoveDisplayName(itemInfo.guid);
            }
            else
            {
                ProjectPinBoardManager.SetDisplayName(itemInfo.guid, newName);
            }
            m_AssetList.Focus();
            return true;
        }

        private void OnListItemDragged(ListItem listItem)
        {
            TriggerDragging();
        }

        private void UnbindListItem(VisualElement element, int index)
        {
            ListItem listItem = (ListItem)element;
            listItem.HideNameTextField();
            listItem.index = -1;
            listItem.userData = null;
            listItem.UnregisterCallback<MouseDownEvent>(OnListItemMouseDown);
        }

        private void OnListItemMouseDown(MouseDownEvent evt)
        {
            if (!(evt.target is ListItem listItem))
            {
                return;
            }
            if (evt.button == 1)
            {
                int[] selectedIndices = m_AssetList.selectedIndices.ToArray();
                m_AssetList.SetSelectionWithoutNotify(selectedIndices);
                if (!selectedIndices.Contains(listItem.index))
                {
                    m_AssetList.SetSelection(listItem.index);
                }
            }
        }

        #endregion

        #region ListItem Menu

        private static class ListItemMenuItemName
        {
            public const string Select = "Select";
            public const string Open = "Open";
            public const string ShowInExplorer = "Show In Explorer";
            public const string Top = "Top";
            public const string UnTop = "Un-top";
            public const string RePin = "Re-pin (Update pin time)";
            public const string Unpin = "Unpin";
            public const string SetDisplayName = "Set Display Name";
            public const string SetDisplayNameToPath = "Set Display Name to Path";
            public const string RemoveDisplayName = "Remove Display Name";
            public const string SetTags = "Set Tag(s)";
            public const string RemoveTags = "Remove Tag(s)";
        }

        private void ItemMenuBuilder(ContextualMenuPopulateEvent evt)
        {
            object listItem = evt.target;
            DropdownMenu menu = evt.menu;
            menu.AppendAction(
                ListItemMenuItemName.Select,
                OnItemMenuAction,
                EnabledOnSingleSelection,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.Open,
                OnItemMenuAction,
                EnabledOnSingleSelection,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.ShowInExplorer,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendSeparator();
            menu.AppendAction(
                ListItemMenuItemName.RePin,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.Unpin,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendSeparator();
            menu.AppendAction(ListItemMenuItemName.Top, OnItemMenuAction, AlwaysEnabled, listItem);
            menu.AppendAction(
                ListItemMenuItemName.UnTop,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendSeparator();
            menu.AppendAction(
                ListItemMenuItemName.SetDisplayName,
                OnItemMenuAction,
                EnabledOnSingleSelection,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.SetDisplayNameToPath,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.RemoveDisplayName,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendSeparator();
            menu.AppendAction(
                ListItemMenuItemName.SetTags,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendAction(
                ListItemMenuItemName.RemoveTags,
                OnItemMenuAction,
                AlwaysEnabled,
                listItem
            );
            menu.AppendSeparator();

            DropdownMenuAction.Status AlwaysEnabled(DropdownMenuAction a) =>
                DropdownMenuAction.AlwaysEnabled(a);

            DropdownMenuAction.Status EnabledOnSingleSelection(DropdownMenuAction a)
            {
                return (
                    m_AssetList.selectedItems.Count() == 1
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled
                );
            }
        }

        private void OnItemMenuAction(DropdownMenuAction action)
        {
            if (!(action.userData is ListItem item))
            {
                return;
            }
            switch (action.name)
            {
                case ListItemMenuItemName.Select:
                {
                    ItemInfo itemInfo = item.userData;
                    PipiUtility.FocusOnAsset(itemInfo.guid);
                    break;
                }
                case ListItemMenuItemName.Open:
                {
                    ItemInfo itemInfo = item.userData;
                    PipiUtility.OpenAsset(itemInfo.guid);
                    break;
                }
                case ListItemMenuItemName.ShowInExplorer:
                {
                    foreach (ItemInfo itemInfo in GetSelectedItemInfos())
                    {
                        PipiUtility.ShowInExplorer(itemInfo.guid);
                    }
                    break;
                }
                case ListItemMenuItemName.RePin:
                {
                    ProjectPinBoardManager.Pin(GetSelectedItemGuids());
                    break;
                }
                case ListItemMenuItemName.Unpin:
                {
                    ProjectPinBoardManager.Unpin(GetSelectedItemGuids());
                    break;
                }
                case ListItemMenuItemName.Top:
                {
                    ProjectPinBoardManager.Top(GetSelectedItemGuids(), true);
                    break;
                }
                case ListItemMenuItemName.UnTop:
                {
                    ProjectPinBoardManager.Top(GetSelectedItemGuids(), false);
                    break;
                }
                case ListItemMenuItemName.SetDisplayName:
                {
                    item.ShowNameTextField();
                    break;
                }
                case ListItemMenuItemName.SetDisplayNameToPath:
                {
                    foreach (ItemInfo itemInfo in GetSelectedItemInfos())
                    {
                        string path = itemInfo.Path.Replace("Assets/", "");
                        ProjectPinBoardManager.SetDisplayName(itemInfo.guid, path);
                    }
                    break;
                }
                case ListItemMenuItemName.RemoveDisplayName:
                {
                    foreach (ItemInfo itemInfo in GetSelectedItemInfos())
                    {
                        ProjectPinBoardManager.RemoveDisplayName(itemInfo.guid);
                    }
                    break;
                }
                case ListItemMenuItemName.SetTags:
                {
                    ShowTagPopup(GetSelectedItemInfos());
                    break;
                }
                case ListItemMenuItemName.RemoveTags:
                {
                    foreach (ItemInfo itemInfo in GetSelectedItemInfos())
                    {
                        ProjectPinBoardManager.RemoveTags(itemInfo.guid);
                    }
                    break;
                }
            }
        }

        #endregion
    }
}
