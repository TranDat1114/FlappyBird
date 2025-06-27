using System;
using FlappyBird.AI;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ so sánh 2 AI
    /// </summary>
    public class DualAIGameMode : GameModeBase
    {
        private GameState ai1State = new GameState();
        private GameState ai2State = new GameState();
        private bool gameStarted = false;
        private int testRounds = 10;
        private int currentRound = 1;
        private int ai1Wins = 0;
        private int ai2Wins = 0;

        public override void Initialize()
        {
            ResetRound();
        }

        private void ResetRound()
        {
            ai1State.Reset();
            ai2State.Reset();

            // Both use God mode but different strategies
            ai1State.GodMode = true;
            ai2State.GodMode = true;

            // Initialize pipes
            var pipe1 = new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random);
            var pipe2 = new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random);

            ai1State.Pipes.Add(pipe1);
            ai2State.Pipes.Add(pipe2);

            gameStarted = false;
        }

        public override void Update()
        {
            if (!gameStarted) return;

            // Update both AIs
            UpdateAI(ai1State, "Conservative");
            UpdateAI(ai2State, "Aggressive");

            // Check if round is complete
            if (ai1State.GameOver && ai2State.GameOver)
            {
                CompleteRound();
            }
        }

        private void UpdateAI(GameState aiState, string strategy)
        {
            if (aiState.GameOver) return;

            aiState.FrameCounter++;
            aiState.UpdateDifficulty();

            // Different AI strategies
            if (strategy == "Conservative")
            {
                GodModeAI.AutoControlBird(aiState); // Standard AI
            }
            else
            {
                // More aggressive AI - jump more frequently
                GodModeAI.AutoControlBirdAggressive(aiState);
            }

            UpdateBirdPhysics(aiState);
            UpdatePipes(aiState);
            CheckCollision(aiState);
        }

        private void CompleteRound()
        {
            // Determine winner
            if (ai1State.Score > ai2State.Score)
            {
                ai1Wins++;
            }
            else if (ai2State.Score > ai1State.Score)
            {
                ai2Wins++;
            }

            currentRound++;

            if (currentRound <= testRounds)
            {
                // Start next round
                System.Threading.Thread.Sleep(1000); // Brief pause
                ResetRound();
                gameStarted = true;
                ai1State.GameStarted = true;
                ai2State.GameStarted = true;
            }
        }

        public override void Render()
        {
            Console.Clear();

            // Render comparison
            RenderAIComparison();

            // Render statistics
            RenderStatistics();
        }

        private void RenderAIComparison()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        AI COMPARISON                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  Round {currentRound}/{testRounds} - Conservative AI vs Aggressive AI                 ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

            // AI stats
            Console.WriteLine($"║  Conservative AI: {ai1State.Score,3} điểm | Status: {(ai1State.GameOver ? "THUA" : "CHƠI"),4}                      ║");
            Console.WriteLine($"║  Aggressive AI  : {ai2State.Score,3} điểm | Status: {(ai2State.GameOver ? "THUA" : "CHƠI"),4}                      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");

            Console.ResetColor();
        }

        private void RenderStatistics()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("THONG KE TONG HOP:");
            Console.WriteLine($"   Conservative AI Wins: {ai1Wins}");
            Console.WriteLine($"   Aggressive AI Wins  : {ai2Wins}");
            Console.WriteLine($"   Ties               : {currentRound - 1 - ai1Wins - ai2Wins}");

            if (currentRound > testRounds)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                string finalWinner = ai1Wins > ai2Wins ? "Conservative AI" :
                                   ai2Wins > ai1Wins ? "Aggressive AI" : "TIE";
                Console.WriteLine($"*** WINNER: {finalWinner} ***");
                Console.WriteLine("R: Run Again | ESC: Menu");
            }
            else if (!gameStarted)
            {
                Console.WriteLine();
                Console.WriteLine("SPACE: Start Test | ESC: Menu");
            }

            Console.ResetColor();
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted && currentRound <= testRounds)
                    {
                        gameStarted = true;
                        ai1State.GameStarted = true;
                        ai2State.GameStarted = true;
                    }
                    break;

                case ConsoleKey.R:
                    if (currentRound > testRounds)
                    {
                        // Reset tournament
                        currentRound = 1;
                        ai1Wins = 0;
                        ai2Wins = 0;
                        Initialize();
                    }
                    break;

                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }

        public override bool IsGameOver()
        {
            return shouldExit;
        }
    }
}
