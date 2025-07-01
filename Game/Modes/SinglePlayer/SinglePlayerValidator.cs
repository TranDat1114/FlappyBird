using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    /// <summary>
    /// Validator cho Single Player Mode - Đảm bảo consistency với menu design
    /// </summary>
    public class SinglePlayerValidator
    {
        // === MENU CONSISTENCY CONSTANTS ===
        private const int MENU_BORDER_WIDTH = 66;  // Khớp chính xác với menu border
        private const int GAME_DISPLAY_HEIGHT = 22; // Chiều cao vùng game
        private const int TOTAL_DISPLAY_HEIGHT = 30; // Tổng chiều cao (game + header + footer)

        /// <summary>
        /// Validate rằng game dimensions khớp hoàn toàn với menu
        /// </summary>
        public void ValidateMenuConsistency()
        {
            if (GameState.GameWidth != MENU_BORDER_WIDTH)
            {
                throw new InvalidOperationException($"Game width ({GameState.GameWidth}) không khớp với menu border width ({MENU_BORDER_WIDTH})");
            }

            if (GameState.GameHeight != GAME_DISPLAY_HEIGHT)
            {
                throw new InvalidOperationException($"Game height ({GameState.GameHeight}) không phù hợp với display height ({GAME_DISPLAY_HEIGHT})");
            }
        }
    }
}
