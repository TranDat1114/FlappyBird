using System;
using System.Collections.Generic;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.Game
{
    /// <summary>
    /// Class xử lý việc render game lên console
    /// </summary>
    public static class GameRenderer
    {
        // ASCII Art Characters for better UI design
        private static readonly char BirdChar = '♦';           // Diamond bird character
        private static readonly char PipeChar = '█';          // Solid block for pipes
        private static readonly char BorderHorizontal = '═';  // Double horizontal line
        private static readonly char BorderVertical = '║';    // Double vertical line
        private static readonly char BorderCornerTL = '╔';    // Top-left corner
        private static readonly char BorderCornerTR = '╗';    // Top-right corner
        private static readonly char BorderCornerBL = '╚';    // Bottom-left corner
        private static readonly char BorderCornerBR = '╝';    // Bottom-right corner
        private static readonly char BackgroundChar = '·';    // Light dot for background pattern
        
        /// <summary>
        /// Vẽ toàn bộ game
        /// </summary>
        public static void Draw(GameState gameState)
        {
            // Vẽ vào buffer thay vì trực tiếp ra console
            char[,] screen = new char[GameState.GameHeight, GameState.GameWidth];
            
            // Khởi tạo nền với pattern nhẹ
            InitializeBackground(screen);
            
            // Vẽ viền game
            DrawBorders(screen);
            
            // Vẽ ống
            DrawPipes(screen, gameState.Pipes);
            
            // Vẽ chim
            DrawBird(screen, gameState);
            
            // Chỉ cập nhật những ô thay đổi để tránh nhấp nháy
            RenderOptimized(screen, gameState);
            
            // Chỉ cập nhật UI text khi có thay đổi để giảm flickering
            bool uiChanged = (gameState.Score != gameState.LastScore) || 
                           (gameState.DifficultyLevel != gameState.LastDifficultyLevel) || 
                           (gameState.GameStarted != gameState.LastGameStarted) || 
                           (gameState.GodMode != gameState.LastGodMode) || 
                           gameState.ForceFullRedraw;
                           
            if (uiChanged)
            {
                RenderUI(gameState);
                gameState.LastScore = gameState.Score;
                gameState.LastDifficultyLevel = gameState.DifficultyLevel;
                gameState.LastGameStarted = gameState.GameStarted;
                gameState.LastGodMode = gameState.GodMode;
                gameState.ForceFullRedraw = false;
            }
        }
        
        /// <summary>
        /// Khởi tạo nền với pattern nhẹ
        /// </summary>
        private static void InitializeBackground(char[,] screen)
        {
            for (int y = 0; y < GameState.GameHeight; y++)
            {
                for (int x = 0; x < GameState.GameWidth; x++)
                {
                    // Tạo pattern nền với dots nhẹ
                    if ((x + y) % 8 == 0 && y > 0 && y < GameState.GameHeight - 1 && x > 0 && x < GameState.GameWidth - 1)
                        screen[y, x] = BackgroundChar;
                    else
                        screen[y, x] = ' ';
                }
            }
        }
        
        /// <summary>
        /// Vẽ viền game
        /// </summary>
        private static void DrawBorders(char[,] screen)
        {
            // Viền trên
            for (int x = 1; x < GameState.GameWidth - 1; x++)
            {
                screen[0, x] = BorderHorizontal;
            }
            // Viền dưới
            for (int x = 1; x < GameState.GameWidth - 1; x++)
            {
                screen[GameState.GameHeight - 1, x] = BorderHorizontal;
            }
            // Viền trái và phải
            for (int y = 1; y < GameState.GameHeight - 1; y++)
            {
                screen[y, 0] = BorderVertical;
                screen[y, GameState.GameWidth - 1] = BorderVertical;
            }
            // Góc viền
            screen[0, 0] = BorderCornerTL;
            screen[0, GameState.GameWidth - 1] = BorderCornerTR;
            screen[GameState.GameHeight - 1, 0] = BorderCornerBL;
            screen[GameState.GameHeight - 1, GameState.GameWidth - 1] = BorderCornerBR;
        }
        
        /// <summary>
        /// Vẽ tất cả các ống
        /// </summary>
        private static void DrawPipes(char[,] screen, List<Pipe> pipes)
        {
            foreach (var pipe in pipes)
            {
                if (pipe.X >= 1 && pipe.X < GameState.GameWidth - 1)
                {
                    DrawSinglePipe(screen, pipe);
                }
            }
        }
        
        /// <summary>
        /// Vẽ một ống
        /// </summary>
        private static void DrawSinglePipe(char[,] screen, Pipe pipe)
        {
            // Ống trên - vẽ với độ dày 3 pixel
            for (int y = 1; y <= pipe.TopHeight; y++)
            {
                if (y >= 1 && y < GameState.GameHeight - 1)
                {
                    // Vẽ ống chính
                    screen[y, pipe.X] = PipeChar;
                    
                    // Vẽ viền ống nếu có chỗ
                    if (pipe.X - 1 >= 1)
                        screen[y, pipe.X - 1] = PipeChar;
                    if (pipe.X + 1 < GameState.GameWidth - 1)
                        screen[y, pipe.X + 1] = PipeChar;
                }
            }
            
            // Vẽ mũ ống trên (cap)
            if (pipe.TopHeight + 1 < GameState.GameHeight - 1 && pipe.TopHeight >= 1)
            {
                for (int capX = pipe.X - 2; capX <= pipe.X + 2; capX++)
                {
                    if (capX >= 1 && capX < GameState.GameWidth - 1)
                        screen[pipe.TopHeight, capX] = '▀';
                }
            }
            
            // Ống dưới - vẽ với độ dày 3 pixel
            for (int y = GameState.GameHeight - pipe.BottomHeight - 1; y < GameState.GameHeight - 1; y++)
            {
                if (y >= 1 && y < GameState.GameHeight - 1)
                {
                    // Vẽ ống chính
                    screen[y, pipe.X] = PipeChar;
                    
                    // Vẽ viền ống nếu có chỗ
                    if (pipe.X - 1 >= 1)
                        screen[y, pipe.X - 1] = PipeChar;
                    if (pipe.X + 1 < GameState.GameWidth - 1)
                        screen[y, pipe.X + 1] = PipeChar;
                }
            }
            
            // Vẽ mũ ống dưới (cap)
            int bottomCapY = GameState.GameHeight - pipe.BottomHeight - 1;
            if (bottomCapY > 1 && bottomCapY < GameState.GameHeight - 1)
            {
                for (int capX = pipe.X - 2; capX <= pipe.X + 2; capX++)
                {
                    if (capX >= 1 && capX < GameState.GameWidth - 1)
                        screen[bottomCapY, capX] = '▄';
                }
            }
        }
        
        /// <summary>
        /// Vẽ chim với animation
        /// </summary>
        private static void DrawBird(char[,] screen, GameState gameState)
        {
            if (GameState.BirdX >= 1 && GameState.BirdX < GameState.GameWidth - 1 && 
                gameState.BirdY >= 1 && gameState.BirdY < GameState.GameHeight - 1)
            {
                // Animation chim responsive với vật lý
                char currentBirdChar;
                if (gameState.BirdVelocity < -0.12f) // Điều chỉnh để phù hợp với jumpStrength -0.8f
                {
                    currentBirdChar = '^';  // Bay lên
                }
                else if (gameState.BirdVelocity > 0.12f) // Giữ nguyên để phù hợp với gravity 0.06f
                {
                    currentBirdChar = 'v';  // Rơi xuống
                }
                else
                {
                    currentBirdChar = BirdChar;  // Bình thường
                }
                
                screen[gameState.BirdY, GameState.BirdX] = currentBirdChar;
                
                // Thêm hiệu ứng cánh chim nhanh hơn
                gameState.BirdAnimationFrame = (gameState.BirdAnimationFrame + 1) % 4; // Giảm từ 6 xuống 4 để nhanh hơn
                if (gameState.BirdAnimationFrame < 2) // Giảm từ 3 xuống 2
                {
                    // Cánh lên
                    if (GameState.BirdX - 1 >= 1) screen[gameState.BirdY, GameState.BirdX - 1] = '~';
                }
                else
                {
                    // Cánh xuống
                    if (GameState.BirdX - 1 >= 1) screen[gameState.BirdY, GameState.BirdX - 1] = '_';
                }
            }
        }
        
        /// <summary>
        /// Render tối ưu chỉ những pixel thay đổi
        /// </summary>
        private static void RenderOptimized(char[,] newScreen, GameState gameState)
        {
            // Batch các thay đổi để giảm số lần gọi SetCursorPosition
            var changes = new List<(int x, int y, char ch)>();
            
            // Thu thập tất cả các thay đổi
            for (int y = 0; y < GameState.GameHeight; y++)
            {
                for (int x = 0; x < GameState.GameWidth; x++)
                {
                    if (newScreen[y, x] != gameState.PreviousScreen[y, x])
                    {
                        changes.Add((x, y, newScreen[y, x]));
                        gameState.PreviousScreen[y, x] = newScreen[y, x];
                    }
                }
            }
            
            // Áp dụng các thay đổi theo batch để tối ưu performance
            if (changes.Count > 0)
            {
                // Sắp xếp theo y rồi x để tối ưu cursor movement
                changes.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
                
                int currentX = -1, currentY = -1;
                foreach (var (x, y, ch) in changes)
                {
                    // Chỉ di chuyển cursor khi cần thiết
                    if (x != currentX + 1 || y != currentY)
                    {
                        Console.SetCursorPosition(x, y);
                    }
                    Console.Write(ch);
                    currentX = x;
                    currentY = y;
                }
            }
        }
        
        /// <summary>
        /// Render UI text
        /// </summary>
        private static void RenderUI(GameState gameState)
        {
            // Hiển thị thông tin ở dưới màn hình game với thiết kế đẹp hơn
            Console.SetCursorPosition(0, GameState.GameHeight);
            if (!gameState.GameStarted)
            {
                string startMsg = "SPACE: Bắt đầu | G: God Mode | ESC: Thoát";
                if (gameState.GodMode)
                {
                    startMsg = "SPACE: Bắt đầu | A: Auto-restart | ESC: Thoát";
                }
                Console.Write(startMsg.PadRight(GameState.GameWidth));
                Console.SetCursorPosition(0, GameState.GameHeight + 1);
                
                string modeMsg;
                if (gameState.GodMode)
                {
                    var improvement = GameLogger.AnalyzeFailures();
                    if (gameState.GodModeAutoRestart)
                    {
                        modeMsg = $"GOD AUTO MODE - Attempts: {gameState.GodModeAttempts} | Best: {gameState.GodModeBestScore} | Failures: {improvement.TotalFailures}";
                    }
                    else
                    {
                        modeMsg = $"GOD MODE - Learning from {improvement.TotalFailures} failures | Conservative: {improvement.GetRecommendedConservativeness():P0}";
                    }
                    Console.ForegroundColor = gameState.GodModeAutoRestart ? ConsoleColor.Cyan : ConsoleColor.Magenta;
                }
                else
                {
                    modeMsg = "Chọn chế độ chơi của bạn";
                    Console.ForegroundColor = ConsoleColor.White;
                }
                
                Console.Write(modeMsg.PadRight(GameState.GameWidth));
                Console.ResetColor();
            }
            else
            {
                string statusMsg = $"Điểm: {gameState.Score} | Level: {gameState.DifficultyLevel}";
                if (gameState.GodMode)
                {
                    statusMsg += $" | GOD #{gameState.GodModeAttempts}";
                    if (gameState.GodModeAutoRestart) statusMsg += " AUTO";
                }
                Console.Write(statusMsg.PadRight(GameState.GameWidth));
                Console.SetCursorPosition(0, GameState.GameHeight + 1);
                string controlMsg = gameState.GodMode ? "ESC: Thoát (God Mode)" : "SPACE: Bay lên | ESC: Thoát";
                Console.Write(controlMsg.PadRight(GameState.GameWidth));
            }
        }
        
        /// <summary>
        /// Khởi tạo màn hình trống
        /// </summary>
        public static void InitializeScreen(GameState gameState)
        {
            // Khởi tạo màn hình trống
            for (int y = 0; y < GameState.GameHeight; y++)
            {
                for (int x = 0; x < GameState.GameWidth; x++)
                {
                    gameState.PreviousScreen[y, x] = ' ';
                }
            }
            
            // Xóa màn hình một lần duy nhất
            Console.Clear();
        }
    }
}
