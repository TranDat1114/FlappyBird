using System;
using System.Threading;
using FlappyBird.Models;
using FlappyBird.Game;

namespace FlappyBird.Modes
{
    /// <summary>
    /// Chế độ chơi 2 người
    /// </summary>
    public static class TwoPlayerMode
    {
        private static GameState player1State = new GameState();
        private static GameState player2State = new GameState();
        private static bool shouldExit = false;

        public static void StartTwoPlayerGame()
        {
            Console.Clear();
            Console.CursorVisible = false;
            
            // Reset states
            player1State.Reset();
            player2State.Reset();
            shouldExit = false;

            // Start both games
            player1State.GameStarted = true;
            player2State.GameStarted = true;

            // Display instructions
            ShowInstructions();

            Thread gameThread = new Thread(GameLoop);
            Thread inputThread = new Thread(HandleInput);

            gameThread.Start();
            inputThread.Start();

            gameThread.Join();
            inputThread.Join();

            // Show results
            ShowResults();
        }

        private static void ShowInstructions()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎮 CHẾ ĐỘ HAI NGƯỜI CHƠI 🎮");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Người chơi 1 (Trái): Phím W để nhảy");
            Console.WriteLine("Người chọi 2 (Phải): Phím ↑ để nhảy");
            Console.WriteLine("ESC: Thoát về menu");
            Console.ResetColor();
            Console.WriteLine(new string('═', GameState.GameWidth));
        }

        private static void GameLoop()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long lastFrameTime = 0;
            const long targetFrameTime = 50; // Slower for 2 players

            while (!shouldExit && (!player1State.GameOver || !player2State.GameOver))
            {
                long currentTime = stopwatch.ElapsedMilliseconds;

                if (currentTime - lastFrameTime >= targetFrameTime)
                {
                    // Update both games
                    if (!player1State.GameOver)
                    {
                        GameEngine.Update(player1State);
                    }
                    
                    if (!player2State.GameOver)
                    {
                        GameEngine.Update(player2State);
                    }

                    DrawSplitScreen();
                    lastFrameTime = currentTime;
                }

                Thread.Sleep(1);
            }
        }

        private static void HandleInput()
        {
            while (!shouldExit && (!player1State.GameOver || !player2State.GameOver))
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.W:
                                if (!player1State.GameOver)
                                    GameEngine.Jump(player1State);
                                break;
                                
                            case ConsoleKey.UpArrow:
                                if (!player2State.GameOver)
                                    GameEngine.Jump(player2State);
                                break;
                                
                            case ConsoleKey.Escape:
                                shouldExit = true;
                                break;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Thread.Sleep(10);
                }
                
                Thread.Sleep(10);
            }
        }

        private static void DrawSplitScreen()
        {
            // Clear game area only
            for (int y = 5; y < GameState.GameHeight + 5; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(new string(' ', GameState.GameWidth));
            }

            // Draw separator
            Console.SetCursorPosition(GameState.GameWidth / 2, 5);
            for (int y = 5; y < GameState.GameHeight + 5; y++)
            {
                Console.SetCursorPosition(GameState.GameWidth / 2, y);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
            }
            Console.ResetColor();

            // Draw Player 1 (Left side)
            DrawPlayer(player1State, 0, "P1", ConsoleColor.Green);
            
            // Draw Player 2 (Right side)
            DrawPlayer(player2State, GameState.GameWidth / 2 + 1, "P2", ConsoleColor.Blue);

            // Draw scores
            Console.SetCursorPosition(5, GameState.GameHeight + 6);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"P1: Score {player1State.Score} | Level {player1State.DifficultyLevel}");
            
            Console.SetCursorPosition(45, GameState.GameHeight + 6);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"P2: Score {player2State.Score} | Level {player2State.DifficultyLevel}");
            Console.ResetColor();
        }

        private static void DrawPlayer(GameState state, int offsetX, string playerName, ConsoleColor color)
        {
            int halfWidth = GameState.GameWidth / 2 - 1;

            // Draw bird
            Console.SetCursorPosition(offsetX + (int)GameState.BirdX / 2, (int)state.BirdY + 5);
            Console.ForegroundColor = color;
            Console.Write(state.GameOver ? "✗" : "♦");

            // Draw pipes
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var pipe in state.Pipes)
            {
                int pipeX = offsetX + (pipe.X / 2);
                if (pipeX >= offsetX && pipeX < offsetX + halfWidth)
                {
                    // Top pipe
                    for (int y = 0; y < pipe.TopHeight; y++)
                    {
                        Console.SetCursorPosition(pipeX, y + 5);
                        Console.Write("█");
                    }

                    // Bottom pipe
                    for (int y = pipe.TopHeight + pipe.GapSize; y < GameState.GameHeight; y++)
                    {
                        Console.SetCursorPosition(pipeX, y + 5);
                        Console.Write("█");
                    }
                }
            }

            // Player name and status
            Console.SetCursorPosition(offsetX + 2, 5);
            Console.ForegroundColor = color;
            Console.Write($"{playerName}{(state.GameOver ? " [THUA]" : "")}");
            
            Console.ResetColor();
        }

        private static void ShowResults()
        {
            Console.SetCursorPosition(0, GameState.GameHeight + 8);
            Console.WriteLine(new string('═', GameState.GameWidth));
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 KẾT QUẢ TRẬN ĐẤU:");

            string winner;
            if (player1State.Score > player2State.Score)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                winner = "NGƯỜI CHƠI 1 THẮNG!";
            }
            else if (player2State.Score > player1State.Score)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                winner = "NGƯỜI CHƠI 2 THẮNG!";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                winner = "HÒA!";
            }

            Console.WriteLine($"🎉 {winner}");
            Console.ResetColor();
            
            Console.WriteLine($"Người chơi 1: {player1State.Score} điểm");
            Console.WriteLine($"Người chơi 2: {player2State.Score} điểm");
            Console.WriteLine("\nNhấn phím bất kỳ để quay về menu...");
            
            Console.ReadKey(true);
        }
    }
}
