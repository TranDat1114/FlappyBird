using System;
using FlappyBird.Models;

namespace FlappyBird.Game
{
    /// <summary>
    /// Base class cho các chế độ chơi
    /// </summary>
    public abstract class GameModeBase : IGameMode
    {
        protected bool shouldExit = false;
        protected readonly Random Random = new Random();
        
        public abstract void Initialize();
        public abstract void Update();
        public abstract void Render();
        public abstract void HandleInput(ConsoleKeyInfo keyInfo);
        public abstract bool IsGameOver();
        
        public virtual void Cleanup()
        {
            shouldExit = false;
        }
        
        /// <summary>
        /// Common physics update for bird
        /// </summary>
        protected void UpdateBirdPhysics(GameState gameState)
        {
            // Vật lý chim chuẩn
            gameState.BirdVelocity += GameState.Gravity;
            
            // Giới hạn tốc độ rơi tối đa
            if (gameState.BirdVelocity > GameState.MaxFallSpeed)
            {
                gameState.BirdVelocity = GameState.MaxFallSpeed;
            }
            
            gameState.BirdY += (int)Math.Round(gameState.BirdVelocity);
            
            // Kiểm tra biên
            if (gameState.BirdY < 1) 
            {
                gameState.BirdY = 1;
                gameState.BirdVelocity = 0;
            }
            
            if (gameState.BirdY >= GameState.GameHeight - 1)
            {
                gameState.GameOver = true;
                return;
            }
        }
        
        /// <summary>
        /// Common pipes update
        /// </summary>
        protected void UpdatePipes(GameState gameState)
        {
            if (gameState.FrameCounter % gameState.PipeSpeed == 0)
            {
                for (int i = gameState.Pipes.Count - 1; i >= 0; i--)
                {
                    gameState.Pipes[i].X--;
                    
                    if (gameState.Pipes[i].X < -2)
                    {
                        gameState.Pipes.RemoveAt(i);
                        gameState.Score++;
                    }
                }
                
                if (gameState.Pipes.Count == 0 || gameState.Pipes[gameState.Pipes.Count - 1].X < GameState.GameWidth - GameState.PipeSpacing)
                {
                    int currentGapSize = gameState.GetCurrentGapSize();
                    gameState.Pipes.Add(new Pipe(GameState.GameWidth - 1, currentGapSize, GameState.GameHeight, Random));
                }
            }
        }
        
        /// <summary>
        /// Common collision detection
        /// </summary>
        protected void CheckCollision(GameState gameState)
        {
            foreach (var pipe in gameState.Pipes)
            {
                if (GameState.BirdX >= pipe.X - 1 && GameState.BirdX <= pipe.X + 1)
                {
                    if (gameState.BirdY <= pipe.TopHeight || gameState.BirdY >= GameState.GameHeight - pipe.BottomHeight - 1)
                    {
                        gameState.GameOver = true;
                        return;
                    }
                }
            }
            
            if (gameState.BirdY <= 0 || gameState.BirdY >= GameState.GameHeight - 1)
            {
                gameState.GameOver = true;
            }
        }
        
        /// <summary>
        /// Common jump logic
        /// </summary>
        protected void Jump(GameState gameState)
        {
            if (!gameState.GameStarted)
            {
                gameState.GameStarted = true;
            }
            
            gameState.BirdVelocity = GameState.JumpStrength;
        }
    }
}
