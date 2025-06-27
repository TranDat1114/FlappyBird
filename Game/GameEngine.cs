using System;
using System.Threading;
using FlappyBird.Enum;

namespace FlappyBird.Game
{
    /// <summary>
    /// Game Engine Manager - quản lý các chế độ chơi khác nhau
    /// </summary>
    public static class GameEngine
    {
        private static IGameMode? currentGameMode;
        private static Thread? gameThread;
        private static Thread? inputThread;
        private static volatile bool isRunning = false;

        /// <summary>
        /// Khởi động game với chế độ chơi được chỉ định
        /// </summary>
        public static void StartGame(GameMode gameMode)
        {
            //clear screen and reset console settings
            Console.Clear();
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);
            


            // Tạo game mode instance
            currentGameMode = GameModeFactory.CreateGameMode(gameMode);
            currentGameMode.Initialize();

            // Bắt đầu game loop
            isRunning = true;

            gameThread = new Thread(GameLoop);
            inputThread = new Thread(InputLoop);

            gameThread.Start();
            inputThread.Start();

            // Đợi threads kết thúc
            gameThread.Join();
            inputThread.Join();

            // Cleanup
            currentGameMode.Cleanup();
        }

        /// <summary>
        /// Dừng game hiện tại
        /// </summary>
        public static void StopGame()
        {
            isRunning = false;
        }

        /// <summary>
        /// Game loop chính
        /// </summary>
        private static void GameLoop()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long lastFrameTime = 0;
            const long targetFrameTime = 16; // 60 FPS

            while (isRunning && currentGameMode != null && !currentGameMode.IsGameOver())
            {
                long currentTime = stopwatch.ElapsedMilliseconds;

                if (currentTime - lastFrameTime >= targetFrameTime)
                {
                    currentGameMode.Update();
                    currentGameMode.Render();
                    lastFrameTime = currentTime;
                }

                Thread.Sleep(1);
            }

            isRunning = false;
        }

        /// <summary>
        /// Input handling loop
        /// </summary>
        private static void InputLoop()
        {
            while (isRunning && currentGameMode != null && !currentGameMode.IsGameOver())
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    currentGameMode.HandleInput(keyInfo);
                }

                Thread.Sleep(10);
            }
        }
    }
}
