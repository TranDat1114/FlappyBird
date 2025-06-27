using System;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.AI
{
    /// <summary>
    /// AI điều khiển chim trong God mode
    /// </summary>
    public static class GodModeAI
    {
        private static readonly Random Random = new Random();
        private static AIImprovementData? cachedImprovementData = null;
        private static DateTime lastAnalysisTime = DateTime.MinValue;
        
        /// <summary>
        /// Điều khiển chim tự động trong God mode với AI được cải thiện
        /// </summary>
        public static void AutoControlBird(GameState gameState)
        {
            // Lấy dữ liệu cải thiện AI (cache 10 giây để tối ưu performance)
            if (cachedImprovementData == null || DateTime.Now - lastAnalysisTime > TimeSpan.FromSeconds(10))
            {
                cachedImprovementData = GameLogger.AnalyzeFailures();
                lastAnalysisTime = DateTime.Now;
            }
            
            // Điều chỉnh AI dựa trên dữ liệu học được
            float conservativeness = cachedImprovementData.GetRecommendedConservativeness();
            float godJumpStrength = -0.6f * (1f + conservativeness * 0.3f); // Điều chỉnh jump strength dựa trên conservativeness
            
            // Tìm ống gần nhất phía trước chim
            Pipe? nearestPipe = null;
            foreach (var pipe in gameState.Pipes)
            {
                if (pipe.X > GameState.BirdX - 15) // Giảm phạm vi phản ứng để chính xác hơn
                {
                    nearestPipe = pipe;
                    break;
                }
            }
            
            // Kiểm tra AI learning - tránh các vị trí nguy hiểm đã va chạm trước đó
            if (nearestPipe != null && GameLogger.IsHazardousPosition(gameState.BirdY, nearestPipe.X, nearestPipe.TopHeight, nearestPipe.BottomHeight))
            {
                // Nếu vị trí hiện tại nguy hiểm, nhảy ngay để tránh
                float safeJumpResult = gameState.BirdY + godJumpStrength;
                if (safeJumpResult >= 3) // Đảm bảo không nhảy vào viền trên
                {
                    gameState.BirdVelocity = godJumpStrength;
                    return; // Ưu tiên thoát khỏi vị trí nguy hiểm
                }
            }
            
            // Emergency protection - bảo vệ với viền (điều chỉnh dựa trên border collision rate)
            int topBuffer = 6 + (int)(cachedImprovementData.BorderCollisionRate * 4); // Tăng buffer nếu va chạm biên nhiều
            int bottomBuffer = 6 + (int)(cachedImprovementData.BorderCollisionRate * 4);
            
            if (gameState.BirdY <= topBuffer)
            {
                return; // TUYỆT ĐỐI không nhảy khi gần viền trên
            }
            
            if (gameState.BirdY >= GameState.GameHeight - bottomBuffer)
            {
                gameState.BirdVelocity = godJumpStrength; // Nhảy nhẹ thoát viền dưới
                return;
            }
            
            if (nearestPipe != null)
            {
                HandlePipeNavigation(gameState, nearestPipe, godJumpStrength, conservativeness);
            }
            else
            {
                // Không có ống - giữ vị trí an toàn dưới trung tâm
                int centerY = GameState.GameHeight / 2 + 6; // Tăng buffer thêm 3 ô (từ 3 lên 6) - dưới trung tâm
                if (gameState.BirdY > centerY + 5) // Tăng threshold thêm 3 ô (từ 2 lên 5)
                {
                    gameState.BirdVelocity = godJumpStrength;
                }
            }
        }
        
        /// <summary>
        /// Xử lý điều hướng qua ống với AI được cải thiện
        /// </summary>
        private static void HandlePipeNavigation(GameState gameState, Pipe pipe, float godJumpStrength, float conservativeness)
        {
            // Thông số an toàn tối đa điều chỉnh theo conservativeness
            int safetyBuffer = 7 + (int)(conservativeness * 3); // Tăng buffer dựa trên mức độ conservative
            int reactionDistance = Math.Max(18, 25 - gameState.DifficultyLevel / 2); // Giảm để phản ứng chính xác
            
            // Tính toán gap an toàn
            int gapTop = pipe.TopHeight + safetyBuffer;
            int gapBottom = GameState.GameHeight - pipe.BottomHeight - safetyBuffer;
            int safeGapCenter = (gapTop + gapBottom) / 2;
            int distanceToPipe = pipe.X - GameState.BirdX;
            
            // Dự đoán vị trí chim chính xác
            float timeToReach = (float)distanceToPipe / gameState.PipeSpeed;
            float predictedY = gameState.BirdY + gameState.BirdVelocity * timeToReach + 0.5f * GameState.Gravity * timeToReach * timeToReach;
            
            if (distanceToPipe <= reactionDistance)
            {
                HandleCloseRangePipe(gameState, pipe, gapTop, gapBottom, predictedY, godJumpStrength, conservativeness);
            }
            else // Xa ống - chuẩn bị vị trí conservative
            {
                HandleLongRangePipe(gameState, pipe, safeGapCenter, gapTop, godJumpStrength, conservativeness);
            }
        }
        
        /// <summary>
        /// Xử lý khi gần ống với AI được cải thiện
        /// </summary>
        private static void HandleCloseRangePipe(GameState gameState, Pipe pipe, int gapTop, int gapBottom, float predictedY, float godJumpStrength, float conservativeness)
        {
            // Zone 1: Tránh ống trên (ưu tiên tuyệt đối) - điều chỉnh margin theo conservativeness
            int topMargin = 6 + (int)(conservativeness * 3);
            if (gameState.BirdY <= gapTop + topMargin || predictedY <= gapTop + topMargin)
            {
                return; // KHÔNG BAO GIỜ nhảy khi có rủi ro va ống trên
            }
            
            // Zone 2: Tránh ống dưới với kiểm tra kích thước
            int bottomMargin = 5 + (int)(conservativeness * 2);
            if (gameState.BirdY >= gapBottom - bottomMargin || predictedY >= gapBottom - bottomMargin)
            {
                // Kiểm tra jump sẽ không làm va vào ống trên
                float jumpResult = gameState.BirdY + godJumpStrength;
                int jumpSafetyMargin = 7 + (int)(conservativeness * 3);
                if (jumpResult >= gapTop + jumpSafetyMargin)
                {
                    gameState.BirdVelocity = godJumpStrength;
                }
                return;
            }
            
            // Zone 3: Điều chỉnh vị trí trong gap
            int safeGapCenter = (gapTop + gapBottom) / 2;
            int centerMargin = 5 + (int)(conservativeness * 2);
            if (gameState.BirdY > safeGapCenter + centerMargin)
            {
                float jumpResult = gameState.BirdY + godJumpStrength;
                int jumpSafetyMargin = 7 + (int)(conservativeness * 3);
                if (jumpResult >= gapTop + jumpSafetyMargin)
                {
                    gameState.BirdVelocity = godJumpStrength;
                }
            }
        }
        
        /// <summary>
        /// Xử lý khi xa ống với AI được cải thiện
        /// </summary>
        private static void HandleLongRangePipe(GameState gameState, Pipe pipe, int safeGapCenter, int gapTop, float godJumpStrength, float conservativeness)
        {
            // Vị trí lý tưởng: dưới trung tâm một chút để tránh bay cao
            int idealOffset = Math.Max(2, 4 - (gameState.DifficultyLevel / 4));
            int idealY = safeGapCenter + idealOffset; // Luôn dưới trung tâm
            
            // Chỉ nhảy khi quá thấp và chắc chắn an toàn - điều chỉnh theo conservativeness
            int lowThreshold = 6 + (int)(conservativeness * 3);
            if (gameState.BirdY > idealY + lowThreshold)
            {
                // Kiểm tra kĩ lưỡng trước khi nhảy
                float jumpResult = gameState.BirdY + godJumpStrength;
                float futureY = jumpResult + GameState.Gravity * 3; // Dự đoán 3 frame sau jump
                
                int futureSafetyMargin = 8 + (int)(conservativeness * 4);
                if (futureY >= gapTop + futureSafetyMargin)
                {
                    gameState.BirdVelocity = godJumpStrength;
                }
            }
            
            // Xử lý đặc biệt cho gap nhỏ - ULTRA CONSERVATIVE
            if (pipe.GapSize <= 7)
            {
                int distanceToPipe = pipe.X - GameState.BirdX;
                int smallGapThreshold = 4 + (int)(conservativeness * 2);
                if (distanceToPipe <= 25 && gameState.BirdY > idealY + smallGapThreshold)
                {
                    float conservativeJump = gameState.BirdY + godJumpStrength + GameState.Gravity * 2;
                    int smallGapSafetyMargin = 8 + (int)(conservativeness * 4);
                    if (conservativeJump >= gapTop + smallGapSafetyMargin)
                    {
                        gameState.BirdVelocity = godJumpStrength;
                    }
                }
            }
        }
    }
}
