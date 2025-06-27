using System;
using System.Threading;
using FlappyBird.AI;
using FlappyBird.Game;
using FlappyBird.Models;
using FlappyBird.Utils;

class FlappyBirdGame
{
    private static GameState gameState = new GameState();
    private static bool shouldExit = false;
    
    static void Main()
    {
        Console.CursorVisible = false;
        
        // Chỉ thiết lập kích thước cửa sổ trên Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(GameState.GameWidth, GameState.GameHeight + 5);
                Console.SetBufferSize(GameState.GameWidth, GameState.GameHeight + 5);
            }
            catch
            {
                // Bỏ qua nếu không thể thiết lập kích thước
            }
        }
        
        // Vòng lặp chính để cho phép restart game
        bool exitProgram = false;
        
        // Tải thống kê God mode từ file khi khởi động
        GameLogger.LoadGodModeStats(gameState);
        
        while (!exitProgram)
        {
            // Reset flag thoát cho game mới
            shouldExit = false;
            // Reset game state trước khi bắt đầu
            gameState.Reset();
            
            // Khởi tạo màn hình trống để tránh nhấp nháy ban đầu
            GameRenderer.InitializeScreen(gameState);
            
            // Khởi tạo game với ống đầu tiên
            GameEngine.InitializeGame(gameState);
            
            Thread gameThread = new Thread(GameLoop);
            Thread inputThread = new Thread(HandleInput);
            
            gameThread.Start();
            inputThread.Start();
            
            // Nếu God mode auto-restart được bật, tự động bắt đầu game
            if (gameState.GodMode && gameState.GodModeAutoRestart)
            {
                gameState.GameStarted = true;
            }
            
            // Đợi game kết thúc
            gameThread.Join();
            
            // Nếu shouldExit được set, thoát chương trình
            if (shouldExit)
            {
                exitProgram = true;
            }
            
            // Nếu game over và không phải do ESC, cho phép restart
            if (!exitProgram)
            {
                // Cập nhật thống kê God mode
                if (gameState.GodMode)
                {
                    gameState.GodModeAttempts++;
                    if (gameState.Score > gameState.GodModeBestScore)
                    {
                        gameState.GodModeBestScore = gameState.Score;
                    }
                    // Lưu thống kê sau mỗi lần chơi
                    GameLogger.SaveGodModeStats(gameState);
                }
                
                // God mode auto-restart
                if (gameState.GodMode && gameState.GodModeAutoRestart)
                {
                    // Hiển thị thông tin ngắn gọn và tự động restart
                    Console.SetCursorPosition(0, GameState.GameHeight + 2);
                    var improvement = GameLogger.AnalyzeFailures();
                    Console.WriteLine($"God Mode - Lần thử #{gameState.GodModeAttempts} | Điểm: {gameState.Score} | Best: {gameState.GodModeBestScore} | Failures: {improvement.TotalFailures}");
                    Console.WriteLine($"Learning: Top{improvement.TopCollisionRate:P0} Bottom{improvement.BottomCollisionRate:P0} Border{improvement.BorderCollisionRate:P0} | ESC: Dừng");
                    
                    // Đợi 1 giây hoặc ESC để dừng
                    for (int i = 0; i < 10; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                gameState.GodModeAutoRestart = false;
                                exitProgram = true;
                                break;
                            }
                        }
                        Thread.Sleep(100);
                    }
                    
                    // Nếu không bị dừng, tự động restart
                    if (!exitProgram)
                    {
                        continue; // Restart ngay lập tức
                    }
                }
                else
                {
                    // Chế độ thường hoặc God mode không auto-restart
                    Console.SetCursorPosition(0, GameState.GameHeight + 2);
                    if (gameState.GodMode)
                    {
                        var improvement = GameLogger.AnalyzeFailures();
                        Console.WriteLine($"GOD MODE - Lần thử #{gameState.GodModeAttempts} | Điểm: {gameState.Score} | Best: {gameState.GodModeBestScore}");
                        Console.WriteLine($"AI Learning: {improvement.TotalFailures} failures | Conservative: {improvement.GetRecommendedConservativeness():P0}");
                        Console.WriteLine("R: Thử lại | A: Auto-restart | C: Xóa data | ESC: Thoát");
                    }
                    else
                    {
                        Console.WriteLine($"GAME OVER! Điểm số cuối cùng: {gameState.Score}");
                        Console.WriteLine("R: Chơi lại | ESC: Thoát");
                    }
                    
                    // Đợi input sau game over
                    while (true)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.R)
                        {
                            break; // Restart game
                        }
                        else if (keyInfo.Key == ConsoleKey.A && gameState.GodMode)
                        {
                            gameState.GodModeAutoRestart = true;
                            break; // Restart với auto mode
                        }
                        else if (keyInfo.Key == ConsoleKey.C && gameState.GodMode)
                        {
                            // Xóa dữ liệu học và reset
                            GameLogger.ClearFailureData();
                            GameLogger.ClearGodModeStats();
                            gameState.GodModeAttempts = 0;
                            gameState.GodModeBestScore = 0;
                            Console.SetCursorPosition(0, GameState.GameHeight + 4);
                            Console.WriteLine("Đã xóa dữ liệu học! Nhấn R để bắt đầu lại...");
                        }
                        else if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            exitProgram = true;
                            break; // Thoát
                        }
                    }
                }
            }
        }
        
        Console.SetCursorPosition(0, GameState.GameHeight + 3);
        Console.WriteLine("Cảm ơn bạn đã chơi!");
    }
    
    static void GameLoop()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long lastFrameTime = 0;
        const long targetFrameTime = 16; // 60 FPS = ~16.67ms per frame
        
        while (!gameState.GameOver)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            
            // Chỉ update nếu đủ thời gian đã trôi qua
            if (currentTime - lastFrameTime >= targetFrameTime)
            {
                GameEngine.Update(gameState);
                GameRenderer.Draw(gameState);
                lastFrameTime = currentTime;
            }
            
            // Sleep ngắn để không làm CPU quá tải
            Thread.Sleep(1);
        }
        
        // Vẽ frame cuối cùng khi game over
        GameRenderer.Draw(gameState);
    }
    
    static void HandleInput()
    {
        while (!gameState.GameOver && !shouldExit)
        {
            try
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    GameEngine.Jump(gameState);
                }
                else if (keyInfo.Key == ConsoleKey.G && !gameState.GameStarted)
                {
                    // Chỉ cho phép chuyển đổi God mode khi chưa bắt đầu game
                    gameState.GodMode = !gameState.GodMode;
                    gameState.ForceFullRedraw = true; // Buộc vẽ lại toàn bộ để cập nhật UI
                }
                else if (keyInfo.Key == ConsoleKey.A && !gameState.GameStarted && gameState.GodMode)
                {
                    // Chỉ cho phép bật auto-restart khi đang trong God mode và chưa bắt đầu game
                    gameState.GodModeAutoRestart = !gameState.GodModeAutoRestart;
                    gameState.ForceFullRedraw = true;
                    
                    // Nếu vừa bật auto-restart, tự động bắt đầu game
                    if (gameState.GodModeAutoRestart)
                    {
                        gameState.GameStarted = true;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    gameState.GameOver = true;
                    shouldExit = true; // Thoát hoàn toàn
                }
            }
            catch (InvalidOperationException)
            {
                // Có thể xảy ra khi console input bị interrupt, bỏ qua
                Thread.Sleep(10);
            }
        }
    }
}
