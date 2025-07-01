// Simple Menu System
using FlappyBird.Enum;

namespace FlappyBird.UI
{
    public static class SimpleMenuSystem
    {
        private static int selectedIndex = 0;
        private static readonly string[] menuItems = [
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
    ];

        private static readonly MenuAction[] menuActions = [
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
    ];

        private static readonly bool[] selectableItems = [
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
    ];

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
            Console.WriteLine("║       ███████╗██╗      █████╗ ██████╗ ██████╗ ██╗   ██╗        ║");
            Console.WriteLine("║       ██╔════╝██║     ██╔══██╗██╔══██╗██╔══██╗╚██╗ ██╔╝        ║");
            Console.WriteLine("║       █████╗  ██║     ███████║██████╔╝██████╔╝ ╚████╔╝         ║");
            Console.WriteLine("║       ██╔══╝  ██║     ██╔══██║██╔═══╝ ██╔═══╝   ╚██╔╝          ║");
            Console.WriteLine("║       ██║     ███████╗██║  ██║██║     ██║        ██║           ║");
            Console.WriteLine("║       ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝        ╚═╝           ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║                  ██████╗ ██╗██████╗ ██████╗                    ║");
            Console.WriteLine("║                  ██╔══██╗██║██╔══██╗██╔══██╗                   ║");
            Console.WriteLine("║                  ██████╔╝██║██████╔╝██║  ██║                   ║");
            Console.WriteLine("║                  ██╔══██╗██║██╔══██╗██║  ██║                   ║");
            Console.WriteLine("║                  ██████╔╝██║██║  ██║██████╔╝                   ║");
            Console.WriteLine("║                  ╚═════╝ ╚═╝╚═╝  ╚═╝╚═════╝                    ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            // Description
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("INFO: " + GetMenuDescription(selectedIndex));
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
            Console.WriteLine("│    Điều khiển  :                                               │");
            Console.WriteLine("│    ↑ ↓         : Di chuyển lựa chọn                            │");
            Console.WriteLine("│    ENTER       : Chọn                                          │");
            Console.WriteLine("│    ESC         : Thoát                                         │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static ConsoleColor GetMenuItemColor(int index)
        {
            if (!selectableItems[index])
            {
                return menuItems[index].StartsWith("   NGUOI") || menuItems[index].StartsWith("   LUYEN")
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
}