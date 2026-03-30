using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarMenu m_ToolbarSortingMenu = null;

        private void InitToolbarSortingMenu()
        {
            m_ToolbarSortingMenu = new ToolbarMenu()
            {
                name = "SortingMenu",
                tooltip = "Asset list sorting",
                variant = ToolbarMenu.Variant.Popup,
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 40,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 4,
                    paddingRight = 4,
                    marginLeft = -1,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                }
            };
            m_Toolbar.Add(m_ToolbarSortingMenu);
            m_ToolbarSortingMenu.Insert(
                0,
                new Image()
                {
                    image = PipiUtility.GetIcon("AlphabeticalSorting"),
                    scaleMode = ScaleMode.ScaleToFit,
                }
            );
            {
                TextElement text = m_ToolbarSortingMenu.Q<TextElement>("", "unity-text-element");
                text.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            BuildSortingMenuItems();
        }

        #region Sorting Menu

        private Sorting m_Sorting = Sorting.NameUp;

        private static readonly Dictionary<string, Sorting> s_SortingMenuMap = new Dictionary<
            string,
            Sorting
        >()
        {
            { "Name ↑", Sorting.NameUp },
            { "Name ↓", Sorting.NameDown },
            { "Pin Time ↑", Sorting.TimeUp },
            { "Pin Time ↓", Sorting.TimeDown },
        };

        private void BuildSortingMenuItems()
        {
            DropdownMenu menu = m_ToolbarSortingMenu.menu;
            foreach (var item in s_SortingMenuMap)
            {
                menu.AppendAction(item.Key, OnSortingMenuAction, GetSortingMenuActionStatus);
            }
        }

        private void OnSortingMenuAction(DropdownMenuAction action)
        {
            if (s_SortingMenuMap.TryGetValue(action.name, out Sorting value))
            {
                SwitchSorting(value);
            }
        }

        private DropdownMenuAction.Status GetSortingMenuActionStatus(DropdownMenuAction action)
        {
            if (s_SortingMenuMap.TryGetValue(action.name, out Sorting value))
            {
                return (
                    m_Sorting == value
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal
                );
            }
            return DropdownMenuAction.Status.Disabled;
        }

        #endregion

        #region Sorting

        private enum Sorting
        {
            NameUp = 1,
            NameDown = 2,
            TimeUp = 3,
            TimeDown = 4,
        }

        private static class SortingPriority
        {
            public const int Directory = 20;
            public const int Top = 10;
            public const int Base = 0;
            public const int Invalid = -1;
        }

        private static readonly Comparison<ItemInfo> s_BaseSortingComparer = (a, b) =>
        {
            int ap = SortingPriority.Base;
            int bp = SortingPriority.Base;
            if (ProjectPinBoardSettings.topFolder)
            {
                if (a.IsDirectory())
                    ap += SortingPriority.Directory;
                if (b.IsDirectory())
                    bp += SortingPriority.Directory;
            }
            if (a.top)
                ap += SortingPriority.Top;
            if (b.top)
                bp += SortingPriority.Top;
            if (!a.IsValid())
                ap += SortingPriority.Invalid;
            if (!b.IsValid())
                bp += SortingPriority.Invalid;
            return bp - ap;
        };

        private static readonly Dictionary<Sorting, Comparison<ItemInfo>> s_SortingComparers =
            new Dictionary<Sorting, Comparison<ItemInfo>>()
            {
                {
                    Sorting.NameUp,
                    (a, b) =>
                    {
                        int baseSorting = s_BaseSortingComparer(a, b);
                        return baseSorting != 0
                            ? baseSorting
                            : string.Compare(
                                a.Name,
                                b.Name,
                                StringComparison.InvariantCultureIgnoreCase
                            );
                    }
                },
                {
                    Sorting.NameDown,
                    (a, b) =>
                    {
                        int baseSorting = s_BaseSortingComparer(a, b);
                        if (baseSorting != 0)
                            return baseSorting;
                        return (
                            -string.Compare(
                                a.Name,
                                b.Name,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        );
                    }
                },
                {
                    Sorting.TimeUp,
                    (a, b) =>
                    {
                        int baseSorting = s_BaseSortingComparer(a, b);
                        if (baseSorting != 0)
                            return baseSorting;
                        return a.time.CompareTo(b.time);
                    }
                },
                {
                    Sorting.TimeDown,
                    (a, b) =>
                    {
                        int baseSorting = s_BaseSortingComparer(a, b);
                        if (baseSorting != 0)
                            return baseSorting;
                        return (-a.time.CompareTo(b.time));
                    }
                },
            };

        private void Sort(ref List<ItemInfo> list)
        {
            list.Sort(s_SortingComparers[m_Sorting]);
        }

        private void SwitchSorting(Sorting sorting)
        {
            m_Sorting = sorting;
            UpdateContent();
        }

        #endregion
    }
}
