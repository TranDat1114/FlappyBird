using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Xử lý Game Over menu cho TwoPlayerGameMode
    /// </summary>
    public class TwoPlayerGameOverMenu(TwoPlayerBuffer buffer)
    {
        private bool showGameOverMenu = false;
        private int gameOverSelectedIndex = 0; // 0: Chơi lại, 1: Về menu chính
        private readonly string[] gameOverOptions = ["Choi lai", "Ve menu chinh"];
        private DateTime gameOverTime = DateTime.MinValue; // Thời gian bắt đầu game over

        private readonly TwoPlayerBuffer buffer = buffer;

        public bool ShowGameOverMenu => showGameOverMenu;
        public DateTime GameOverTime => gameOverTime;

        /// <summary>
        /// Bắt đầu hiển thị game over menu
        /// </summary>
        public void StartGameOverMenu()
        {
            showGameOverMenu = true;
            gameOverSelectedIndex = 0; // Reset về "Chơi lại"
            gameOverTime = DateTime.Now; // Ghi lại thời gian game over
        }

        /// <summary>
        /// Reset game over menu state
        /// </summary>
        public void ResetGameOverMenu()
        {
            showGameOverMenu = false;
            gameOverSelectedIndex = 0;
            gameOverTime = DateTime.MinValue;
        }

        /// <summary>
        /// Render game over menu vào buffer
        /// </summary>
        public void RenderGameOverMenuToBuffer(GameState player1State, GameState player2State)
        {
            // Clear area first
            for (int y = 5; y < 20; y++)
            {
                for (int x = 10; x < 56; x++)
                {
                    buffer.WriteToBuffer(x, y, ' ', ConsoleColor.White);
                }
            }

            // Menu border
            int menuStartX = 15;
            int menuStartY = 8;
            int menuWidth = 36;
            int menuHeight = 8;

            // Draw border
            buffer.WriteToBuffer(menuStartX, menuStartY, '╔', ConsoleColor.White);
            for (int i = 1; i < menuWidth - 1; i++)
            {
                buffer.WriteToBuffer(menuStartX + i, menuStartY, '═', ConsoleColor.White);
            }
            buffer.WriteToBuffer(menuStartX + menuWidth - 1, menuStartY, '╗', ConsoleColor.White);

            // Menu content
            string[] menuLines = [
                "",
                "           GAME OVER",
                "",
                $"    Player 1 Score: {player1State.Score}",
                $"    Player 2 Score: {player2State.Score}",
                "",
                "    Choi lai",
                "    Ve menu chinh"
            ];

            for (int line = 0; line < menuLines.Length && line < menuHeight - 2; line++)
            {
                buffer.WriteToBuffer(menuStartX, menuStartY + 1 + line, '║', ConsoleColor.White);

                string text = menuLines[line];
                if (line == 6 || line == 7) // Menu options
                {
                    bool isSelected = (line == 6 && gameOverSelectedIndex == 0) ||
                                     (line == 7 && gameOverSelectedIndex == 1);

                    // For simplicity in buffer, use different characters for selection
                    if (isSelected)
                    {
                        text = ">>> " + text.Trim() + " <<<";
                    }
                }

                for (int i = 0; i < text.Length && i < menuWidth - 2; i++)
                {
                    buffer.WriteToBuffer(menuStartX + 1 + i, menuStartY + 1 + line, text[i], ConsoleColor.White);
                }

                buffer.WriteToBuffer(menuStartX + menuWidth - 1, menuStartY + 1 + line, '║', ConsoleColor.White);
            }

            // Bottom border
            buffer.WriteToBuffer(menuStartX, menuStartY + menuHeight - 1, '╚', ConsoleColor.White);
            for (int i = 1; i < menuWidth - 1; i++)
            {
                buffer.WriteToBuffer(menuStartX + i, menuStartY + menuHeight - 1, '═', ConsoleColor.White);
            }
            buffer.WriteToBuffer(menuStartX + menuWidth - 1, menuStartY + menuHeight - 1, '╝', ConsoleColor.White);
        }

        /// <summary>
        /// Xử lý input cho game over menu - tương tự SinglePlayerGameMode
        /// </summary>
        public GameOverMenuAction HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex > 0 ? gameOverSelectedIndex - 1 : gameOverOptions.Length - 1;
                    return GameOverMenuAction.None;

                case ConsoleKey.DownArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex < gameOverOptions.Length - 1 ? gameOverSelectedIndex + 1 : 0;
                    return GameOverMenuAction.None;

                case ConsoleKey.Enter:
                    if (gameOverSelectedIndex == 0)
                    {
                        // Chọn "Chơi lại"
                        return GameOverMenuAction.Restart;
                    }
                    else
                    {
                        // Chọn "Thoát"
                        return GameOverMenuAction.Exit;
                    }

                case ConsoleKey.Spacebar:
                    // Shortcut để restart nhanh
                    return GameOverMenuAction.Restart;

                case ConsoleKey.Escape:
                    // Thoát
                    return GameOverMenuAction.Exit;

                default:
                    return GameOverMenuAction.None;
            }
        }

        /// <summary>
        /// Xác định người thắng
        /// </summary>
        public string GetWinner(GameState player1State, GameState player2State)
        {
            if (player1State.GameOver && player2State.GameOver)
            {
                if (player1State.Score > player2State.Score)
                    return "PLAYER 1";
                else if (player2State.Score > player1State.Score)
                    return "PLAYER 2";
                else
                    return "HOA";
            }
            else if (player1State.GameOver)
                return "PLAYER 2";
            else if (player2State.GameOver)
                return "PLAYER 1";

            return "DANG CHOI";
        }
    }

    /// <summary>
    /// Enum cho các hành động của game over menu
    /// </summary>
    public enum GameOverMenuAction
    {
        None,
        Restart,
        Exit
    }
}
