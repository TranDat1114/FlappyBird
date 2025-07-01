using System;
using FlappyBird.Enum;
using FlappyBird.Game;
using FlappyBird.UI;

namespace FlappyBird;

class FlappyBirdGame
{
    static void Main()
    {
        Console.CursorVisible = false;

        // Chỉ thiết lập kích thước cửa sổ trên Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(100, 40);
                Console.SetBufferSize(100, 40);
            }
            catch
            {
                // Ignore if we can't set window size
            }
        }

        // Main menu loop
        while (true)
        {
            var menuAction = SimpleMenuSystem.ShowMenu();

            switch (menuAction)
            {
                case MenuAction.SinglePlayer:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.TwoPlayer:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.DualAI:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.SplitScreenAI:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.AITournament:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.Exit:
                    // Console.Clear();
                    // Console.SetCursorPosition(0, 10);
                    // Console.ForegroundColor = ConsoleColor.Yellow;
                    // Console.WriteLine("*** Cam on ban da choi Flappy Bird Game! ***");
                    // Console.WriteLine("*** Hen gap lai! ***");
                    Console.ResetColor();
                    return;
            }
        }
    }
}
