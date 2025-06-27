using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Models;
using FlappyBird.Game;
using FlappyBird.AI;

namespace FlappyBird.Modes
{
    /// <summary>
    /// Tournament AI với nhiều variants
    /// </summary>
    public static class AITournamentMode
    {
        private static readonly Dictionary<string, (Action<GameState> Logic, ConsoleColor Color)> AIVariants = new()
        {
            { "Conservative", (ConservativeAI, ConsoleColor.Green) },
            { "Aggressive", (AggressiveAI, ConsoleColor.Red) },
            { "Balanced", (BalancedAI, ConsoleColor.Yellow) },
            { "Learning", (LearningAI, ConsoleColor.Cyan) }
        };

        public static void StartAITournament()
        {
            Console.Clear();
            Console.CursorVisible = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 AI TOURNAMENT 🏆");
            Console.WriteLine("4 AI variants compete for supremacy!");
            Console.WriteLine($"Each AI plays {3} rounds, best total score wins\n");
            Console.ResetColor();

            var results = new Dictionary<string, List<int>>();
            
            // Initialize results
            foreach (var ai in AIVariants.Keys)
            {
                results[ai] = new List<int>();
            }

            // Run tournament rounds
            for (int round = 1; round <= 3; round++)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"🔥 ROUND {round}/3 🔥");
                Console.ResetColor();

                foreach (var kvp in AIVariants)
                {
                    string aiName = kvp.Key;
                    var (aiLogic, color) = kvp.Value;

                    var score = RunTournamentAI(aiName, aiLogic, color);
                    results[aiName].Add(score);
                }

                // Show round results
                Console.WriteLine($"\n📊 Round {round} Results:");
                var roundResults = results.Select(r => new { Name = r.Key, Score = r.Value.Last() })
                                        .OrderByDescending(r => r.Score);

                int position = 1;
                foreach (var result in roundResults)
                {
                    var color = AIVariants[result.Name].Color;
                    Console.ForegroundColor = color;
                    Console.WriteLine($"  {GetPositionIcon(position)} {result.Name}: {result.Score} points");
                    position++;
                }
                Console.ResetColor();

                if (round < 3)
                {
                    Console.WriteLine("\nNext round in 2 seconds...");
                    Thread.Sleep(2000);
                    Console.WriteLine();
                }
            }

            // Final tournament results
            ShowTournamentResults(results);
        }

        private static int RunTournamentAI(string aiName, Action<GameState> aiLogic, ConsoleColor color)
        {
            var gameState = new GameState();
            gameState.GameStarted = true;

            Console.ForegroundColor = color;
            Console.Write($"  {aiName} AI: ");
            Console.ResetColor();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int dotCount = 0;

            while (!gameState.GameOver)
            {
                GameEngine.Update(gameState);
                aiLogic(gameState);

                // Progress indicator
                if (stopwatch.ElapsedMilliseconds > dotCount * 200)
                {
                    Console.Write(".");
                    dotCount++;
                }

                Thread.Sleep(10); // Very fast simulation
            }

            Console.ForegroundColor = color;
            Console.WriteLine($" Score: {gameState.Score}");
            Console.ResetColor();

            return gameState.Score;
        }

        private static void ShowTournamentResults(Dictionary<string, List<int>> results)
        {
            Console.WriteLine("\n" + new string('═', 60));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 FINAL TOURNAMENT STANDINGS 🏆");
            Console.ResetColor();

            var finalStandings = results.Select(r => new
            {
                Name = r.Key,
                TotalScore = r.Value.Sum(),
                BestScore = r.Value.Max(),
                AverageScore = r.Value.Average(),
                Scores = r.Value,
                Color = AIVariants[r.Key].Color
            }).OrderByDescending(r => r.TotalScore);

            int position = 1;
            foreach (var ai in finalStandings)
            {
                Console.ForegroundColor = ai.Color;
                string medal = GetPositionIcon(position);
                Console.WriteLine($"{medal} {position}. {ai.Name} AI");
                Console.ResetColor();
                
                Console.WriteLine($"   Total: {ai.TotalScore} | Best: {ai.BestScore} | Avg: {ai.AverageScore:F1}");
                Console.Write($"   Rounds: ");
                
                for (int i = 0; i < ai.Scores.Count; i++)
                {
                    Console.ForegroundColor = ai.Color;
                    Console.Write($"{ai.Scores[i]}");
                    Console.ResetColor();
                    if (i < ai.Scores.Count - 1) Console.Write(", ");
                }
                Console.WriteLine();
                Console.WriteLine();
                
                position++;
            }

            // Champion announcement
            var champion = finalStandings.First();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎉 TOURNAMENT CHAMPION 🎉");
            Console.ForegroundColor = champion.Color;
            Console.WriteLine($"👑 {champion.Name} AI with {champion.TotalScore} total points!");
            
            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }

        private static string GetPositionIcon(int position)
        {
            return position switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => "🏅"
            };
        }

        #region AI Variants

        private static void ConservativeAI(GameState gameState)
        {
            // Very conservative - only jump when really necessary
            if (gameState.BirdY >= GameState.GameHeight - 4)
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
            else if (gameState.BirdVelocity > 0.6f && gameState.BirdY > GameState.GameHeight * 0.7f)
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
        }

        private static void AggressiveAI(GameState gameState)
        {
            // Aggressive - jumps frequently to stay high
            if (gameState.BirdY >= GameState.GameHeight - 7 || 
                (gameState.BirdVelocity > 0.2f && gameState.BirdY > GameState.GameHeight * 0.4f))
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
        }

        private static void BalancedAI(GameState gameState)
        {
            // Balanced approach - aims for middle area
            float targetY = GameState.GameHeight * 0.5f;
            
            if (gameState.BirdY >= GameState.GameHeight - 5 || 
                (gameState.BirdY > targetY + 3 && gameState.BirdVelocity > 0.3f))
            {
                gameState.BirdVelocity = GameState.JumpStrength;
            }
        }

        private static void LearningAI(GameState gameState)
        {
            // Simple pattern recognition - looks at pipes
            Pipe? nearestPipe = null;
            foreach (var pipe in gameState.Pipes)
            {
                if (pipe.X > GameState.BirdX - 5)
                {
                    nearestPipe = pipe;
                    break;
                }
            }

            if (nearestPipe != null)
            {
                int distanceToPipe = nearestPipe.X - GameState.BirdX;
                float optimalY = nearestPipe.TopHeight + (nearestPipe.GapSize / 2f);

                if (distanceToPipe <= 30 && gameState.BirdY > optimalY + 2)
                {
                    gameState.BirdVelocity = GameState.JumpStrength;
                }
            }
            else
            {
                // No pipe visible, maintain middle position
                if (gameState.BirdY >= GameState.GameHeight - 6)
                {
                    gameState.BirdVelocity = GameState.JumpStrength;
                }
            }
        }

        #endregion
    }
}
