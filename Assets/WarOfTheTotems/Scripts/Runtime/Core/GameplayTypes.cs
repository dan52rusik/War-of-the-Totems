namespace WarOfTheTotems.Core
{
    /// <summary>
    /// Сторона команды (игрок / враг).
    /// Общий тип, используемый во всех слоях.
    /// </summary>
    public enum TeamSide { Player, Enemy }

    /// <summary>
    /// Тип тотема юнита.
    /// </summary>
    public enum TotemType { None, Stone, Beast, Shadow }

    /// <summary>
    /// Идентификатор текущего экрана.
    /// </summary>
    public enum ScreenId { Intro, Hub, Levels, Units, Battle }
}
