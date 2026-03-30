using System.Text.RegularExpressions;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private ToolbarSearchField m_ToolbarSearchField = null;

        private void InitToolbarSearchField()
        {
            m_ToolbarSearchField = new ToolbarSearchField()
            {
                name = "Search",
                value = m_SearchText,
                tooltip = "Search [Ctrl+F]",
                style =
                {
                    width = StyleKeyword.Auto,
                    marginLeft = 4,
                    marginRight = 4,
                    flexShrink = 1,
                }
            };
            m_Toolbar.Add(m_ToolbarSearchField);
            m_ToolbarSearchField.RegisterValueChangedCallback(OnSearchFieldValueChanged);
            m_ToolbarSearchField.RegisterCallback<KeyDownEvent>(
                (evt) =>
                {
                    if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
                    {
                        FocusToAssetList();
                    }
                }
            );
        }

        private void OnSearchFieldValueChanged(ChangeEvent<string> evt)
        {
            SetSearchText(evt.newValue);
        }

        #region SearchField

        private string m_SearchText = string.Empty;

        private void SetSearchText(string value)
        {
            m_SearchText = value;
            m_ToolbarSearchField.SetValueWithoutNotify(value);
            UpdateContent();
        }

        private void FocusToSearchField()
        {
            m_ToolbarSearchField.Focus();
        }

        #endregion

        #region Searching

        private string GetSearchContent()
        {
            string text = m_SearchText;
            if (text.Contains("type:"))
            {
                Match match = Regex.Match(text, k_TypeFilterPattern, RegexOptions.IgnoreCase);
                text = match.Groups[3].Value;
            }
            if (text.Contains("tag:"))
            {
                Match match = Regex.Match(text, k_TagFilterPattern, RegexOptions.IgnoreCase);
                text = match.Groups[3].Value;
            }
            return text;
        }

        #endregion
    }
}
