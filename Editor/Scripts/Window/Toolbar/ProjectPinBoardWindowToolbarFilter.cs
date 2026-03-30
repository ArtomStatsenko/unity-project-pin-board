using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarButton m_ToolbarTypeFilterButton = null;

        private ToolbarButton m_ToolbarTagFilterButton = null;

        private void InitToolbarFilterButton()
        {
            m_ToolbarTypeFilterButton = new ToolbarButton()
            {
                name = "TypeFilterButton",
                tooltip = "Filter by Type",
                focusable = false,
                style =
                {
                    flexShrink = 0,
                    width = 25,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginLeft = 0,
                    marginRight = 0,
                    left = 0,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            m_Toolbar.Add(m_ToolbarTypeFilterButton);
            m_ToolbarTypeFilterButton.Add(
                new Image()
                {
                    image = PipiUtility.GetIcon("FilterByType"),
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 16, }
                }
            );
            m_ToolbarTypeFilterButton.clicked += OnTypeFilterButtonClicked;

            m_ToolbarTagFilterButton = new ToolbarButton()
            {
                name = "TagFilterButton",
                tooltip = "Filter by Tag",
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
            m_Toolbar.Add(m_ToolbarTagFilterButton);
            m_ToolbarTagFilterButton.Add(
                new Image()
                {
                    image = PipiUtility.GetIcon("FilterByLabel"),
                    scaleMode = ScaleMode.ScaleToFit,
                    style = { width = 16, }
                }
            );
            m_ToolbarTagFilterButton.clicked += OnTagFilterButtonClicked;
        }

        private void OnTypeFilterButtonClicked()
        {
            const string popupTitle = "Filter by Type";
            Vector2 popupPos = new Vector2(
                m_ToolbarTypeFilterButton.worldBound.x,
                m_ToolbarTypeFilterButton.worldBound.y + 4
            );
            ShowTogglePopup(
                popupTitle,
                popupPos,
                m_ItemTypeList,
                m_FilteringType,
                (v) =>
                {
                    SetTypeFilter(
                        m_FilteringType.Equals(v, StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : v
                    );
                }
            );
        }

        private void OnTagFilterButtonClicked()
        {
            const string popupTitle = "Filter by Tag";
            Vector2 popupPos = new Vector2(
                m_ToolbarTagFilterButton.worldBound.x,
                m_ToolbarTagFilterButton.worldBound.y + 4
            );
            ShowTogglePopup(
                popupTitle,
                popupPos,
                m_ItemTagList,
                m_FilteringTag,
                (v) =>
                {
                    SetTagFilter(
                        m_FilteringTag.Equals(v, StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : v
                    );
                }
            );
        }

        #region Filtering

        private const string k_TypeFilterPattern = @"(.*)\s*type:(\S+)?\s*(.*)";

        private const string k_TagFilterPattern = @"(.*)\s*tag:(\S+)?\s*(.*)";

        private string m_FilteringType = string.Empty;

        private string m_FilteringTag = string.Empty;

        private void SetTypeFilter(string type, bool updateSearch = true)
        {
            m_FilteringType = type;
            if (updateSearch)
            {
                string content = GetSearchContent();
                SetSearchText(string.IsNullOrWhiteSpace(type) ? content : $"type:{type} {content}");
            }
        }

        private void SetTagFilter(string tag, bool updateSearch = true)
        {
            m_FilteringTag = tag;
            if (updateSearch)
            {
                string content = GetSearchContent();
                SetSearchText(string.IsNullOrWhiteSpace(tag) ? content : $"tag:{tag} {content}");
            }
        }

        private void Filter(ref List<ItemInfo> list)
        {
            string text = m_SearchText.Trim();

            SetTypeFilter(string.Empty, false);
            if (text.Contains("type:"))
            {
                Match match = Regex.Match(text, k_TypeFilterPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    SetTypeFilter(match.Groups[2].Value, false);
                    if (!string.IsNullOrWhiteSpace(m_FilteringType))
                    {
                        list = list.FindAll(v => v.MatchType(m_FilteringType));
                    }
                    text = match.Groups[3].Value;
                }
            }

            SetTagFilter(string.Empty, false);
            if (text.Contains("tag:"))
            {
                Match match = Regex.Match(text, k_TagFilterPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    SetTagFilter(match.Groups[2].Value, false);
                    if (!string.IsNullOrWhiteSpace(m_FilteringTag))
                    {
                        list = list.FindAll(v => v.MatchTag(m_FilteringTag));
                    }
                    text = match.Groups[3].Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                string pattern = text.Trim().ToCharArray().Join(".*");
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                list = list.FindAll(
                    v => regex.Match(v.AssetName).Success || regex.Match(v.displayName).Success
                );
            }
        }

        #endregion
    }
}
