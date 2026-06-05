using UnityEngine;

public static class GradeColorUtility
{
    public static Color GetColor(UpgradeGrade grade)
    {
        switch (grade)
        {
            case UpgradeGrade.Common:
                return new Color32(70, 55, 40, 255);      // 진한 갈색/검정 계열

            case UpgradeGrade.Uncommon:
                return new Color32(67, 145, 73, 255);     // 초록

            case UpgradeGrade.Rare:
                return new Color32(70, 125, 210, 255);    // 파랑

            case UpgradeGrade.Epic:
                return new Color32(155, 90, 210, 255);    // 보라

            case UpgradeGrade.Legendary:
                return new Color32(220, 145, 45, 255);    // 주황/금색

            default:
                return Color.white;
        }
    }

    public static string GetDisplayName(UpgradeGrade grade)
    {
        switch (grade)
        {
            case UpgradeGrade.Common:
                return "일반";

            case UpgradeGrade.Uncommon:
                return "고급";

            case UpgradeGrade.Rare:
                return "희귀";

            case UpgradeGrade.Epic:
                return "영웅";

            case UpgradeGrade.Legendary:
                return "전설";

            default:
                return "-";
        }
    }
    public static string GetHexColor(UpgradeGrade grade)
{
    Color color = GetColor(grade);
    return ColorUtility.ToHtmlStringRGB(color);
}
}