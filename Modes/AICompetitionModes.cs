using System;
using System.Threading;
using System.Collections.Generic;
using FlappyBird.Models;
using FlappyBird.Game;
using FlappyBird.AI;

namespace FlappyBird.Modes
{
    /// <summary>
    /// Chế độ so sánh 2 AI khác nhau
    /// </summary>
    public static class DualAIMode
    {
        public static void StartDualAI()
        {
            Console.Clear();
            Console.CursorVisible = false;

            var ai1Wins = 0;
            var ai2Wins = 0;
            var round = 1;
            const int totalRounds = 5;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚔️ DUAL AI COMPARISON ⚔️");
            Console.WriteLine("Conservative AI vs Aggressive AI");
            Console.WriteLine($"Best of {totalRounds} rounds\n");
            Console.ResetColor();

            while (round <= totalRounds)
            {
                Console.WriteLine($"🔥 ROUND {round}/{totalRounds} 🔥");
                Console.WriteLine($"Conservative AI: {ai1Wins} wins | Aggressive AI: {ai2Wins} wins\n");

                // Run both AIs
                var ai1Result = RunAI("Conservative AI", ConservativeAILogic, ConsoleColor.Green);
                var ai2Result = RunAI("Aggressive AI", AggressiveAILogic, ConsoleColor.Red);

                // Determine winner
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (ai1Result.Score > ai2Result.Score)
                {
                    ai1Wins++;
                    Console.WriteLine($"🎉 Round {round} Winner: Conservative AI (Score: {ai1Result.Score} vs {ai2Result.Score})");
                }
                else if (ai2Result.Score > ai1Result.Score)
                {
                    ai2Wins++;
                    Console.WriteLine($"🎉 Round {round} Winner: Aggressive AI (Score: {ai2Result.Score} vs {ai1Result.Score})");
                }
                else
                {
                    Console.WriteLine($"🤝 Round {round}: TIE! Both scored {ai1Result.Score}");
                }
                Console.ResetColor();

                round++;
                
                if (round <= totalRounds)
                {
                    Console.WriteLine("\nNext round in 3 seconds... (ESC to stop)");
                    for (int i = 0; i < 30; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape) return;
                        }
                        Thread.Sleep(100);
                    }
                    Console.Clear();
                }
            }

            // Final results
            Console.WriteLine("\n" + new string('═', 50));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 FINAL TOURNAMENT RESULTS 🏆");
            
            if (ai1Wins > ai2Wins)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🥇 CHAMPION: Conservative AI ({ai1Wins}-{ai2Wins})");
            }
            else if (ai2Wins > ai1Wins)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"🥇 CHAMPION: Aggressive AI ({ai2Wins}-{ai1Wins})");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"🤝 TOURNAMENT TIE! ({ai1Wins}-{ai2Wins})");
            }

            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }

        private static (int Score, int Level) RunAI(string aiName, Action<GameState> aiLogic, ConsoleColor color)
        {
            var gameState = new GameState();
            gameState.GameStarted = true;

            Console.ForegroundColor = color;
            Console.Write($"{aiName}: ");
            Console.ResetColor();
            Console.Write("Running");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int lastDots = 0;

            while (!gameState.GameOver)
            {
                GameEngine.Update(gameState);
                aiLogic(gameState);

                // Show progress dots
                int dots = (int)(stopwatch.ElapsedMilliseconds / 500) % 4;
                if (dots != lastDots)
                {
                    Console.Write(new string('.', dots));
                    if (dots == 0) Console.Write("   \b\b\b"); // Clear dots
                    lastDots = dots;
                }

                Thread.Sleep(20); // Fast simulation
            }

            Console.ForegroundColor = color;
            Console.WriteLine($" Finished! Score: {gameState.Score}, Level: {gameState.DifficultyLevel}");
            Console.ResetColor();

            return (gameState.Score, gameState.DifficultyLevel);
        }

        /// <summary>
        /// Conservative AI logic - plays safe
        /// </summary>
        private static void ConservativeAILogic(GameState gameState)
        {
            // Use existing GodModeAI but with conservative settings
            GodModeAI.AutoControlBird(gameState);
        }

        /// <summary>
        /// Aggressive AI logic - takes more risks
        /// </summary>
        private static void AggressiveAILogic(GameState gameState)
        {
            // Simplified aggressive logic - jumps more frequently
            if (gameState.BirdY >= GameState.GameHeight - 5 || 
                (gameState.BirdVelocity > 0.3f && gameState.BirdY > GameState.GameHeight / 2))
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
        }
    }

    /// <summary>
    /// Chế độ split screen real-time
    /// </summary>
    public static class SplitScreenAIMode
    {
        private static GameState ai1State = new GameState();
        private static GameState ai2State = new GameState();
        private static bool shouldExit = false;

        public static void StartSplitScreenAI()
        {
            Console.Clear();
            Console.CursorVisible = false;

            ai1State.Reset();
            ai2State.Reset();
            shouldExit = false;

            ai1State.GameStarted = true;
            ai2State.GameStarted = true;

            ShowHeader();

            Thread gameThread = new Thread(GameLoop);
            Thread inputThread = new Thread(HandleInput);

            gameThread.Start();
            inputThread.Start();

            gameThread.Join();
            inputThread.Join();

            ShowFinalResults();
        }

        private static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📺 SPLIT SCREEN AI REAL-TIME 📺");
            Console.ResetColor();
            Console.WriteLine("Conservative AI (Left) vs Learning AI (Right)");
            Console.WriteLine("ESC: Stop");
            Console.WriteLine(new string('═', GameState.GameWidth));
        }

        private static void GameLoop()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long lastFrameTime = 0;
            const long targetFrameTime = 100; // Slower to watch

            while (!shouldExit && (!ai1State.GameOver || !ai2State.GameOver))
            {
                long currentTime = stopwatch.ElapsedMilliseconds;

                if (currentTime - lastFrameTime >= targetFrameTime)
                {
                    // Update both AIs
                    if (!ai1State.GameOver)
                    {
                        GameEngine.Update(ai1State);
                        GodModeAI.AutoControlBird(ai1State); // Conservative
                    }

                    if (!ai2State.GameOver)
                    {
                        GameEngine.Update(ai2State);
                        LearningAILogic(ai2State); // Learning
                    }

                    DrawSplitScreen();
                    lastFrameTime = currentTime;
                }

                Thread.Sleep(1);
            }
        }

        private static void HandleInput()
        {
            while (!shouldExit && (!ai1State.GameOver || !ai2State.GameOver))
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                            shouldExit = true;
                    }
                }
                catch (InvalidOperationException)
                {
                    Thread.Sleep(10);
                }

                Thread.Sleep(50);
            }
        }

        private static void DrawSplitScreen()
        {
            // Clear game area
            for (int y = 4; y < GameState.GameHeight + 4; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(new string(' ', GameState.GameWidth));
            }

            // Draw separator
            for (int y = 4; y < GameState.GameHeight + 4; y++)
            {
                Console.SetCursorPosition(GameState.GameWidth / 2, y);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
            }

            // Draw AI 1 (Left)
            DrawAI(ai1State, 0, "Conservative AI", ConsoleColor.Green);

            // Draw AI 2 (Right)
            DrawAI(ai2State, GameState.GameWidth / 2 + 1, "Learning AI", ConsoleColor.Cyan);

            // Status
            Console.SetCursorPosition(0, GameState.GameHeight + 5);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Conservative: Score {ai1State.Score} | Level {ai1State.DifficultyLevel}");
            
            Console.SetCursorPosition(42, GameState.GameHeight + 5);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Learning: Score {ai2State.Score} | Level {ai2State.DifficultyLevel}");
            Console.ResetColor();
        }

        private static void DrawAI(GameState state, int offsetX, string aiName, ConsoleColor color)
        {
            int halfWidth = GameState.GameWidth / 2 - 1;

            // AI name
            Console.SetCursorPosition(offsetX + 1, 4);
            Console.ForegroundColor = color;
            Console.Write($"{aiName}{(state.GameOver ? " [STOPPED]" : "")}");

            // Bird
            Console.SetCursorPosition(offsetX + (int)GameState.BirdX / 2, (int)state.BirdY + 5);
            Console.Write(state.GameOver ? "✗" : "●");

            // Pipes
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var pipe in state.Pipes)
            {
                int pipeX = offsetX + (pipe.X / 2);
                if (pipeX >= offsetX && pipeX < offsetX + halfWidth)
                {
                    for (int y = 0; y < pipe.TopHeight; y++)
                    {
                        Console.SetCursorPosition(pipeX, y + 5);
                        Console.Write("█");
                    }

                    for (int y = pipe.TopHeight + pipe.GapSize; y < GameState.GameHeight; y++)
                    {
                        Console.SetCursorPosition(pipeX, y + 5);
                        Console.Write("█");
                    }
                }
            }

            Console.ResetColor();
        }

        private static void LearningAILogic(GameState gameState)
        {
            // Simple learning logic - more aggressive than conservative
            if (gameState.BirdY >= GameState.GameHeight - 6 || 
                (gameState.BirdVelocity > 0.4f && gameState.BirdY > GameState.GameHeight / 2 + 2))
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
        }

        private static void ShowFinalResults()
        {
            Console.SetCursorPosition(0, GameState.GameHeight + 7);
            Console.WriteLine(new string('═', GameState.GameWidth));
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏁 SPLIT SCREEN RESULTS:");

            if (ai1State.Score > ai2State.Score)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🏆 Conservative AI Wins!");
            }
            else if (ai2State.Score > ai1State.Score)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🏆 Learning AI Wins!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🤝 It's a Tie!");
            }

            Console.ResetColor();
            Console.WriteLine($"Conservative AI: {ai1State.Score} points");
            Console.WriteLine($"Learning AI: {ai2State.Score} points");
            Console.WriteLine("\nPress any key to return to menu...");
            
            Console.ReadKey(true);
        }
    }
}
