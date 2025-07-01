using System;
using FlappyBird.AI;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ xem 2 AI chơi cùng lúc trên màn hình chia đôi
    /// </summary>
    public class SplitScreenAIGameMode : GameModeBase
    {
        private readonly GameState ai1State = new();
        private readonly GameState ai2State = new();
        private bool gameStarted = false;
        
        public override void Initialize()
        {
            ai1State.Reset();
            ai2State.Reset();
            
            // Both use God mode
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
            
            // Update AI 1 (Conservative)
            if (!ai1State.GameOver)
            {
                ai1State.FrameCounter++;
                ai1State.UpdateDifficulty();
                GodModeAI.AutoControlBird(ai1State);
                UpdateBirdPhysics(ai1State);
                UpdatePipes(ai1State);
                CheckCollision(ai1State);
            }
            
            // Update AI 2 (Aggressive)
            if (!ai2State.GameOver)
            {
                ai2State.FrameCounter++;
                ai2State.UpdateDifficulty();
                GodModeAI.AutoControlBirdAggressive(ai2State);
                UpdateBirdPhysics(ai2State);
                UpdatePipes(ai2State);
                CheckCollision(ai2State);
            }
        }
        
        public override void Render()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🚧 SPLIT SCREEN AI MODE - ĐANG PHÁT TRIỂN");
            Console.WriteLine();
            Console.WriteLine($"Conservative AI: {ai1State.Score} điểm {(ai1State.GameOver ? "(Game Over)" : "")}");
            Console.WriteLine($"Aggressive AI  : {ai2State.Score} điểm {(ai2State.GameOver ? "(Game Over)" : "")}");
            Console.WriteLine();
            
            if (!gameStarted)
            {
                Console.WriteLine("SPACE: Bắt đầu | ESC: Menu");
            }
            else if (ai1State.GameOver && ai2State.GameOver)
            {
                var winner = ai1State.Score > ai2State.Score ? "Conservative AI" :
                           ai2State.Score > ai1State.Score ? "Aggressive AI" : "TIE";
                Console.WriteLine($"*** Winner: {winner} ***");
                Console.WriteLine("R: Restart | ESC: Menu");
            }
            
            Console.ResetColor();
        }
        
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted)
                    {
                        gameStarted = true;
                        ai1State.GameStarted = true;
                        ai2State.GameStarted = true;
                    }
                    break;
                    
                case ConsoleKey.R:
                    if (ai1State.GameOver && ai2State.GameOver)
                    {
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
