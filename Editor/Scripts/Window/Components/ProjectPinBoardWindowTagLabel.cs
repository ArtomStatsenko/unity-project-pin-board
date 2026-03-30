using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private const int k_TagBorderRadius = 9;

        private Label GenTagLabel(string text, Action clickCallback = null)
        {
            Label label = new Label()
            {
                name = $"Tag:{text}",
                text = text,
                style =
                {
                    alignSelf = Align.Center,
                    flexShrink = 1,
                    minWidth = 20,
                    minHeight = 20,
                    borderTopLeftRadius = k_TagBorderRadius,
                    borderTopRightRadius = k_TagBorderRadius,
                    borderBottomLeftRadius = k_TagBorderRadius,
                    borderBottomRightRadius = k_TagBorderRadius,
                    paddingLeft = 5,
                    paddingRight = 5,
                    marginLeft = 5,
                    marginTop = 5,
                    color = tagTextColor,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            label.AddToClassList("Tag");
            label.AddToClassList(Theme.isDarkTheme ? "Dark" : "Light");
            if (clickCallback != null)
            {
                label.RegisterCallback<MouseDownEvent>(_ => clickCallback());
            }
            return label;
        }
    }
}
