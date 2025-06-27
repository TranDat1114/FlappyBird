using System;
using System.Threading;
using FlappyBird.AI;
using FlappyBird.Game;
using FlappyBird.Models;
using FlappyBird.Utils;

// Simple Menu Enums
public enum MenuAction
{
    None,
    SinglePlayer,
    TwoPlayer,
    DualAI,
    SplitScreenAI,
    AITournament,
    Exit
}

// Simple Menu System
public static class SimpleMenuSystem
{
    private static int selectedIndex = 0;
    private static readonly string[] menuItems = {
        "   NGƯỜI CHƠI",
        "       Chơi đơn",
        "       Chơi đôi",
        "",
        "   LUYỆN AI",
        "       Dual AI Comparison",
        "       Split Screen Real-time",
        "       AI Tournament",
        "",
        "   Thoát"
    };

    private static readonly MenuAction[] menuActions = {
        MenuAction.None,        // Header
        MenuAction.SinglePlayer,
        MenuAction.TwoPlayer,
        MenuAction.None,        // Spacer
        MenuAction.None,        // Header
        MenuAction.DualAI,
        MenuAction.SplitScreenAI,
        MenuAction.AITournament,
        MenuAction.None,        // Spacer
        MenuAction.Exit
    };

    private static readonly bool[] selectableItems = {
        false,  // Header
        true,   // Chơi đơn
        true,   // Chơi đôi
        false,  // Spacer
        false,  // Header
        true,   // Dual AI
        true,   // Split Screen
        true,   // Tournament
        false,  // Spacer
        true    // Thoát
    };

    public static MenuAction ShowMenu()
    {
        selectedIndex = GetFirstSelectableIndex();
        ConsoleKeyInfo keyInfo;

        do
        {
            DrawMenu();
            keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    MoveToPreviousSelectableItem();
                    break;
                case ConsoleKey.DownArrow:
                    MoveToNextSelectableItem();
                    break;
                case ConsoleKey.Enter:
                    if (selectableItems[selectedIndex])
                    {
                        return menuActions[selectedIndex];
                    }
                    break;
                case ConsoleKey.Escape:
                    return MenuAction.Exit;
            }
        } while (true);
    }

    private static void DrawMenu()
    {
        Console.Clear();
        Console.CursorVisible = false;

        // ASCII Art Title
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║    ███████╗██╗      █████╗ ██████╗ ██████╗ ██╗   ██╗           ║");
        Console.WriteLine("║    ██╔════╝██║     ██╔══██╗██╔══██╗██╔══██╗╚██╗ ██╔╝           ║");
        Console.WriteLine("║    █████╗  ██║     ███████║██████╔╝██████╔╝ ╚████╔╝            ║");
        Console.WriteLine("║    ██╔══╝  ██║     ██╔══██║██╔═══╝ ██╔═══╝   ╚██╔╝             ║");
        Console.WriteLine("║    ██║     ███████╗██║  ██║██║     ██║        ██║              ║");
        Console.WriteLine("║    ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝        ╚═╝              ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║                 ██████╗ ██╗██████╗ ██████╗                     ║");
        Console.WriteLine("║                 ██╔══██╗██║██╔══██╗██╔══██╗                    ║");
        Console.WriteLine("║                 ██████╔╝██║██████╔╝██║  ██║                    ║");
        Console.WriteLine("║                 ██╔══██╗██║██╔══██╗██║  ██║                    ║");
        Console.WriteLine("║                 ██████╔╝██║██║  ██║██████╔╝                    ║");
        Console.WriteLine("║                 ╚═════╝ ╚═╝╚═╝  ╚═╝╚═════╝                     ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("              🎮 CHÀO MỪNG ĐẾN VỚI FLAPPY BIRD GAME! 🎮");
        Console.ResetColor();
        Console.WriteLine();

        // Menu box
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                           MENU CHÍNH                           ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

        // Menu items
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (string.IsNullOrEmpty(menuItems[i]))
            {
                Console.WriteLine("║                                                                ║");
                continue;
            }

            string prefix = "║  ";

            if (i == selectedIndex && selectableItems[i])
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write(prefix + "► " + menuItems[i]);
                Console.ResetColor();
                Console.WriteLine(new string(' ', 60 - menuItems[i].Length) + "║");
            }
            else
            {
                Console.ForegroundColor = GetMenuItemColor(i);
                Console.Write(prefix + "  " + menuItems[i]);
                Console.ResetColor();
                Console.WriteLine(new string(' ', 60 - menuItems[i].Length) + "║");
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        // Instructions
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("┌────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│    Điều khiển:                                                 │");
        Console.WriteLine("│    ↑ ↓  : Di chuyển lựa chọn                                   │");
        Console.WriteLine("│    ENTER: Chọn                                                 │");
        Console.WriteLine("│    ESC  : Thoát                                                │");
        Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
        Console.ResetColor();

        // Description
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📝 " + GetMenuDescription(selectedIndex));
        Console.ResetColor();
    }

    private static ConsoleColor GetMenuItemColor(int index)
    {
        if (!selectableItems[index])
        {
            return menuItems[index].StartsWith("🎮") || menuItems[index].StartsWith("🤖")
                ? ConsoleColor.Green
                : ConsoleColor.DarkGray;
        }

        return index switch
        {
            1 or 2 => ConsoleColor.Cyan,       // Player modes
            5 or 6 or 7 => ConsoleColor.Magenta, // AI modes
            9 => ConsoleColor.Red,             // Exit
            _ => ConsoleColor.White
        };
    }

    private static string GetMenuDescription(int index)
    {
        return index switch
        {
            0 => "Chế độ dành cho người chơi thật",
            1 => "Chơi một mình với AI tự động (God Mode) hoặc thủ công",
            2 => "Hai người chơi cùng lúc: W (Player 1) và ↑ (Player 2)",
            4 => "Các chế độ huấn luyện và thử nghiệm AI",
            5 => "So sánh hiệu suất giữa 2 AI khác nhau",
            6 => "Xem 2 AI chơi cùng lúc trên màn hình chia đôi",
            7 => "Giải đấu AI với nhiều thuật toán khác nhau",
            9 => "Thoát khỏi game",
            _ => "Sử dụng phím mũi tên để điều hướng"
        };
    }

    private static int GetFirstSelectableIndex()
    {
        for (int i = 0; i < selectableItems.Length; i++)
        {
            if (selectableItems[i])
                return i;
        }
        return 0;
    }

    private static void MoveToPreviousSelectableItem()
    {
        int current = selectedIndex;
        do
        {
            selectedIndex = selectedIndex > 0 ? selectedIndex - 1 : selectableItems.Length - 1;
        } while (!selectableItems[selectedIndex] && selectedIndex != current);
    }

    private static void MoveToNextSelectableItem()
    {
        int current = selectedIndex;
        do
        {
            selectedIndex = selectedIndex < selectableItems.Length - 1 ? selectedIndex + 1 : 0;
        } while (!selectableItems[selectedIndex] && selectedIndex != current);
    }
}

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

        // Main menu loop
        while (true)
        {
            var menuAction = SimpleMenuSystem.ShowMenu();

            switch (menuAction)
            {
                case MenuAction.SinglePlayer:
                    StartSinglePlayerGame();
                    break;

                case MenuAction.TwoPlayer:
                    Console.Clear();
                    Console.WriteLine("🚧 Chế độ chơi đôi đang được phát triển...");
                    Console.WriteLine("Nhấn phím bất kỳ để quay lại menu.");
                    Console.ReadKey();
                    break;

                case MenuAction.DualAI:
                    Console.Clear();
                    Console.WriteLine("🚧 Dual AI Comparison đang được phát triển...");
                    Console.WriteLine("Nhấn phím bất kỳ để quay lại menu.");
                    Console.ReadKey();
                    break;

                case MenuAction.SplitScreenAI:
                    Console.Clear();
                    Console.WriteLine("🚧 Split Screen AI đang được phát triển...");
                    Console.WriteLine("Nhấn phím bất kỳ để quay lại menu.");
                    Console.ReadKey();
                    break;

                case MenuAction.AITournament:
                    Console.Clear();
                    Console.WriteLine("🚧 AI Tournament đang được phát triển...");
                    Console.WriteLine("Nhấn phím bất kỳ để quay lại menu.");
                    Console.ReadKey();
                    break;

                case MenuAction.Exit:
                    Console.Clear();
                    Console.SetCursorPosition(0, 10);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("🎮 Cảm ơn bạn đã chơi Flappy Bird Game!");
                    Console.WriteLine("👋 Hẹn gặp lại!");
                    Console.ResetColor();
                    return;
            }
        }
    }

    static void StartSinglePlayerGame()
    {
        // Vòng lặp chính để cho phép restart game
        bool exitToMenu = false;

        // Tải thống kê God mode từ file khi khởi động
        GameLogger.LoadGodModeStats(gameState);

        while (!exitToMenu)
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

            // Nếu shouldExit được set, thoát về menu
            if (shouldExit)
            {
                exitToMenu = true;
            }

            // Nếu game over và không phải do ESC, cho phép restart
            if (!exitToMenu)
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
                    Console.WriteLine($"Learning: Top{improvement.TopCollisionRate:P0} Bottom{improvement.BottomCollisionRate:P0} Border{improvement.BorderCollisionRate:P0} | ESC: Menu");

                    // Đợi 1 giây hoặc ESC để dừng
                    for (int i = 0; i < 10; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                gameState.GodModeAutoRestart = false;
                                exitToMenu = true;
                                break;
                            }
                        }
                        Thread.Sleep(100);
                    }

                    // Nếu không bị dừng, tự động restart
                    if (!exitToMenu)
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
                        Console.WriteLine("R: Thử lại | A: Auto-restart | C: Xóa data | ESC: Menu");
                    }
                    else
                    {
                        Console.WriteLine($"GAME OVER! Điểm số cuối cùng: {gameState.Score}");
                        Console.WriteLine("R: Chơi lại | ESC: Menu");
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
                            exitToMenu = true;
                            break; // Thoát về menu
                        }
                    }
                }
            }
        }
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
                    shouldExit = true; // Thoát về menu
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
