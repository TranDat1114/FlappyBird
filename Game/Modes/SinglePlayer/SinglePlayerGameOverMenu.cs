using System;
using FlappyBird.Audio;
using FlappyBird.Audio.Enum;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    /// <summary>
    /// Quản lý Game Over Menu cho Single Player Mode
    /// </summary>
    public class SinglePlayerGameOverMenu(SinglePlayerRenderer renderer)
    {
        // === GAME OVER MENU STATE ===
        private bool showGameOverMenu = false;
        private int gameOverSelectedIndex = 0; // 0: Chơi lại, 1: Thoát
        private readonly string[] gameOverOptions = ["Choi lai", "Ve menu chinh"];
        private DateTime gameOverTime = DateTime.MinValue; // Thời gian bắt đầu game over

        private readonly SinglePlayerRenderer renderer = renderer;

        // === PROPERTIES ===
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
        /// Reset game over menu
        /// </summary>
        public void ResetGameOverMenu()
        {
            showGameOverMenu = false;
            gameOverSelectedIndex = 0;
            gameOverTime = DateTime.MinValue;
        }

        /// <summary>
        /// Kiểm tra xem có thể nhận input không (sau delay)
        /// </summary>
        public bool CanReceiveInput()
        {
            return showGameOverMenu && DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800);
        }

        /// <summary>
        /// Kiểm tra xem có thể hiển thị menu không (sau delay)
        /// </summary>
        public bool ShouldShowMenu()
        {
            return showGameOverMenu && DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800);
        }

        /// <summary>
        /// Render game over menu với thiết kế đẹp
        /// </summary>
        public void RenderGameOverMenu(GameState gameState)
        {
            // Vẽ game hiện tại để người chơi thấy kết quả
            renderer.Draw(gameState);

            // Overlay game over menu
            Console.SetCursorPosition(0, GameState.GameHeight + 4);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          GAME OVER!                            ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║                      Score: {gameState.Score,3} diem                           ║");
            Console.WriteLine($"║                      Level: {gameState.DifficultyLevel,3}                                ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

            // Menu options
            for (int i = 0; i < gameOverOptions.Length; i++)
            {
                if (i == gameOverSelectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"║  > {gameOverOptions[i],-58}  ║");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"║    {gameOverOptions[i],-58}  ║");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║    Up/Down: Chon    Enter: Xac nhan    Space: Choi lai         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Xử lý input cho game over menu
        /// </summary>
        /// <returns>Input result indicating what action to take</returns>
        public SinglePlayerGameOverMenuInputResult HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            var result = new SinglePlayerGameOverMenuInputResult();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex > 0 ? gameOverSelectedIndex - 1 : gameOverOptions.Length - 1;
                    break;

                case ConsoleKey.DownArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex < gameOverOptions.Length - 1 ? gameOverSelectedIndex + 1 : 0;
                    break;

                case ConsoleKey.Enter:
                    if (gameOverSelectedIndex == 0)
                    {
                        // Chọn "Chơi lại"
                        result.ShouldRestart = true;
                    }
                    else
                    {
                        // Chọn "Thoát"
                        result.ShouldExit = true;
                    }
                    break;

                case ConsoleKey.Spacebar:
                    // Shortcut để restart nhanh
                    result.ShouldRestart = true;
                    break;

                case ConsoleKey.Escape:
                    // Thoát
                    result.ShouldExit = true;
                    break;
            }

            return result;
        }
    }

    /// <summary>
    /// Kết quả xử lý input của Game Over Menu
    /// </summary>
    public class SinglePlayerGameOverMenuInputResult
    {
        public bool ShouldRestart { get; set; } = false;
        public bool ShouldExit { get; set; } = false;
    }
}
