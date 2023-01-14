namespace A320VAU.FWS
{
    public enum DisplayZone
    {
        Left = 0,
        Right = 1
    }

    public enum WarningColor
    {
        Danger = 0,
        Amber = 1,
        Green = 2,
        White = 3
    }

    public enum WarningType
    {
        SpecialLine = 0,
        ConfigMemo = 1,
        Memo = 2,
        Secondary = 3,
        Primary = 4
    }

    public enum WarningLevel
    {
        None = 0,
        Monitor = 1,
        Aware = 2,
        Immediate = 3
    }

    public enum SystemPage {
        None = 0,
        Engine = 1,
        Bleed = 2
    }
}