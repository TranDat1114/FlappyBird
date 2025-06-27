using System;
using FlappyBird.AI;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.Game
{
    /// <summary>
    /// Engine chính xử lý logic game
    /// </summary>
    public static class GameEngine
    {
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// Cập nhật logic game mỗi frame
        /// </summary>
        public static void Update(GameState gameState)
        {
            gameState.FrameCounter++;
            
            // Chỉ cập nhật game khi đã bắt đầu
            if (!gameState.GameStarted)
            {
                return; // Chim đứng yên cho đến khi nhấn space
            }
            
            // Cập nhật độ khó dựa trên điểm số
            gameState.UpdateDifficulty();
            
            // God mode - AI tự động điều khiển
            if (gameState.GodMode && gameState.GameStarted)
            {
                GodModeAI.AutoControlBird(gameState);
            }
            
            // Vật lý chim thích ứng God mode
            UpdateBirdPhysics(gameState);
            
            // Di chuyển ống
            UpdatePipes(gameState);
            
            // Kiểm tra va chạm
            CheckCollision(gameState);
        }
        
        /// <summary>
        /// Cập nhật vật lý chim
        /// </summary>
        private static void UpdateBirdPhysics(GameState gameState)
        {
            // **FAIR PLAY**: God mode uses EXACT same physics as human player
            float currentGravity = GameState.Gravity; // NO CHEATING - same gravity for everyone
            
            // Vật lý chim với gravity và velocity mượt mà hơn
            gameState.BirdVelocity += currentGravity;
            
            // Giới hạn tốc độ rơi tối đa để tránh rơi quá nhanh
            if (gameState.BirdVelocity > GameState.MaxFallSpeed)
            {
                gameState.BirdVelocity = GameState.MaxFallSpeed;
            }
            
            gameState.BirdY += (int)Math.Round(gameState.BirdVelocity);
            
            // **FAIR BOUNDARIES**: Same collision rules for everyone
            if (gameState.BirdY < 1) 
            {
                gameState.BirdY = 1;
                gameState.BirdVelocity = 0;
            }
            
            if (gameState.BirdY >= GameState.GameHeight - 1)
            {
                // **NO SPECIAL TREATMENT**: God mode dies just like human player
                gameState.GameOver = true;
                return;
            }
        }
        
        /// <summary>
        /// Cập nhật vị trí và tạo ống mới
        /// </summary>
        private static void UpdatePipes(GameState gameState)
        {
            // Di chuyển ống với tốc độ động theo độ khó
            if (gameState.FrameCounter % gameState.PipeSpeed == 0)
            {
                for (int i = gameState.Pipes.Count - 1; i >= 0; i--)
                {
                    gameState.Pipes[i].X--;
                    
                    // Xóa ống đã qua
                    if (gameState.Pipes[i].X < -2)
                    {
                        gameState.Pipes.RemoveAt(i);
                        gameState.Score++;
                    }
                }
                
                // Tạo ống mới với gap size động và spacing được tối ưu
                if (gameState.Pipes.Count == 0 || gameState.Pipes[gameState.Pipes.Count - 1].X < GameState.GameWidth - GameState.PipeSpacing)
                {
                    int currentGapSize = gameState.GetCurrentGapSize();
                    gameState.Pipes.Add(new Pipe(GameState.GameWidth - 1, currentGapSize, GameState.GameHeight, Random));
                }
            }
        }
        
        /// <summary>
        /// Kiểm tra va chạm với ống và biên
        /// </summary>
        private static void CheckCollision(GameState gameState)
        {
            foreach (var pipe in gameState.Pipes)
            {
                // Collision detection với một chút "forgiveness" để trải nghiệm tốt hơn
                // Giảm hitbox một chút để người chơi cảm thấy "may mắn" khi vượt qua
                if (GameState.BirdX >= pipe.X - 1 && GameState.BirdX <= pipe.X + 1) // Giảm từ 2 xuống 1
                {
                    if (gameState.BirdY <= pipe.TopHeight || gameState.BirdY >= GameState.GameHeight - pipe.BottomHeight - 1)
                    {
                        gameState.GameOver = true;
                        
                        // Nếu là God mode, ghi lại thông tin va chạm để học
                        if (gameState.GodMode)
                        {
                            GameLogger.RecordGodModeFailure(gameState, pipe);
                        }
                        return;
                    }
                }
            }
            
            // Kiểm tra va chạm với biên trên/dưới
            if (gameState.BirdY <= 0 || gameState.BirdY >= GameState.GameHeight - 1)
            {
                gameState.GameOver = true;
                
                // Nếu là God mode, ghi lại thông tin va chạm với biên
                if (gameState.GodMode)
                {
                    GameLogger.RecordGodModeBorderFailure(gameState);
                }
            }
        }
        
        /// <summary>
        /// Xử lý nhảy của chim
        /// </summary>
        public static void Jump(GameState gameState)
        {
            if (!gameState.GameStarted)
            {
                gameState.GameStarted = true; // Bắt đầu game khi nhấn space đầu tiên
            }
            
            // Trong God mode, SPACE bị vô hiệu hóa
            if (!gameState.GodMode)
            {
                gameState.BirdVelocity = GameState.JumpStrength; // Sử dụng jump strength gốc cho người chơi
            }
        }
        
        /// <summary>
        /// Khởi tạo game với ống đầu tiên
        /// </summary>
        public static void InitializeGame(GameState gameState)
        {
            // Tạo ống đầu tiên với gap size lớn nhất để khuyến khích người chơi mới
            gameState.Pipes.Clear();
            gameState.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random)); // Gap 9 cho level 1
        }
    }
}
