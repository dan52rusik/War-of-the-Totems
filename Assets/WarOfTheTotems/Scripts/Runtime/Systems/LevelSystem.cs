using System;
using UnityEngine;
using WarOfTheTotems.Core.Data;

namespace WarOfTheTotems.Systems
{
    /// <summary>
    /// Управляет выбором уровня и разблокировками.
    /// Не знает об UI — генерирует событие при изменении выбора.
    /// </summary>
    public sealed class LevelSystem
    {
        // ----------------------------------------------------------------
        // События
        // ----------------------------------------------------------------

        /// <summary>Выбранный уровень изменился.</summary>
        public event Action<int>? OnSelectionChanged; // (selectedIndex)

        // ----------------------------------------------------------------
        // Приватное состояние
        // ----------------------------------------------------------------

        private LevelDefinition[] levels  = Array.Empty<LevelDefinition>();
        private ProgressionSystem progression = null!;
        private int selectedIndex;

        // ----------------------------------------------------------------
        // Инициализация
        // ----------------------------------------------------------------

        public void Bind(LevelDefinition[] levelData, ProgressionSystem prog)
        {
            levels      = levelData;
            progression = prog;
            selectedIndex = 0;
        }

        // ----------------------------------------------------------------
        // Выбор уровня
        // ----------------------------------------------------------------

        public int SelectedIndex => selectedIndex;

        public LevelDefinition SelectedLevel
            => levels.Length == 0
                ? default
                : levels[Mathf.Clamp(selectedIndex, 0, levels.Length - 1)];

        public int TotalLevels => levels.Length;

        public bool CanGoNext => selectedIndex < levels.Length - 1;
        public bool CanGoPrev => selectedIndex > 0;

        public void SelectNext()
        {
            if (!CanGoNext) return;
            selectedIndex++;
            OnSelectionChanged?.Invoke(selectedIndex);
        }

        public void SelectPrevious()
        {
            if (!CanGoPrev) return;
            selectedIndex--;
            OnSelectionChanged?.Invoke(selectedIndex);
        }

        public void SelectIndex(int index)
        {
            if (levels.Length == 0) return;
            selectedIndex = Mathf.Clamp(index, 0, levels.Length - 1);
            OnSelectionChanged?.Invoke(selectedIndex);
        }

        // ----------------------------------------------------------------
        // Разблокировки
        // ----------------------------------------------------------------

        public bool IsSelectedUnlocked => progression.IsLevelUnlocked(selectedIndex);
        public bool IsUnlocked(int index) => progression.IsLevelUnlocked(index);

        /// <summary>
        /// Разблокировать следующий уровень после текущего выбранного
        /// (вызывается после победы в бою).
        /// </summary>
        public void UnlockNextAfterVictory()
        {
            if (levels.Length == 0) return;
            progression.UnlockLevel(selectedIndex + 1);
        }

        // ----------------------------------------------------------------
        // Tutorial
        // ----------------------------------------------------------------

        /// <summary>
        /// Возвращает обучающий уровень (не из массива levels).
        /// </summary>
        public static LevelDefinition GetTutorialLevel() => LevelDefinition.Tutorial();
    }
}
