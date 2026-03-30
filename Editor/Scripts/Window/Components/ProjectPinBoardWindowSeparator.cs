using UnityEngine.UIElements;

namespace ChenPipi.ProjectPinBoard.Editor
{
    public partial class ProjectPinBoardWindow
    {
        private VisualElement GenHorizontalSeparator(float margin = 5, string name = "Separator")
        {
            return new VisualElement()
            {
                name = name,
                style =
                {
                    height = 1,
                    borderBottomWidth = 1,
                    borderBottomColor = separatorColor,
                    marginTop = margin,
                    marginBottom = margin,
                    flexShrink = 0,
                },
            };
        }
    }
}
