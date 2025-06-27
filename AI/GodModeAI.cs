using System;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.AI
{
    /// <summary>
    /// AI điều khiển chim trong God mode - FAIR PLAY với học tập đơn giản
    /// </summary>
    public static class GodModeAI
    {
        private static List<SimplePattern> learnedPatterns = new List<SimplePattern>();
        private static DateTime lastLearningUpdate = DateTime.MinValue;
        private static DateTime lastJumpTime = DateTime.MinValue;
        
        /// <summary>
        /// Điều khiển chim tự động trong God mode - CONSERVATIVE SAFE PLAY
        /// </summary>
        public static void AutoControlBird(GameState gameState)
        {
            // **FAIR AI**: Uses EXACT same jump strength as human player
            float jumpStrength = GameState.JumpStrength; // NO CHEATING
            
            // **SAFE ZONE STRATEGY**: Luôn cố gắng giữ chim ở vùng an toàn (lower-middle)
            int safeZoneTop = GameState.GameHeight / 2 - 2;    // Trên vùng an toàn
            int safeZoneBottom = GameState.GameHeight - 5;     // Dưới vùng an toàn  
            
            // **LEARNING INTEGRATION**: Sử dụng độ cao đã học
            int learnedOptimalHeight = GetLearnedOptimalHeight();
            int idealHeight = learnedOptimalHeight; // Sử dụng độ cao đã học thay vì cố định
            
            // **DEBUG INFO**: Hiển thị trạng thái AI và learning
            Console.SetCursorPosition(1, GameState.GameHeight + 1);
            Console.Write($"[AI] Y:{gameState.BirdY:F1} V:{gameState.BirdVelocity:F2} Target:{idealHeight} Patterns:{learnedPatterns.Count}".PadRight(70));
            
            // **SIMPLE LEARNING**: Cập nhật kiến thức mỗi 5 giây
            if (DateTime.Now - lastLearningUpdate > TimeSpan.FromSeconds(5))
            {
                UpdateLearning();
                lastLearningUpdate = DateTime.Now;
            }
            
            // **ENHANCED CEILING PROTECTION** - Tránh va chạm ống trên cụ thể
            bool isNearCeiling = gameState.BirdY <= 8;
            bool isEmergencyFloor = gameState.BirdY >= GameState.GameHeight - 3; // Y >= 17
            
            // **PIPE-AWARE CEILING CHECK**: Kiểm tra xem có ống trên gần không
            bool willHitTopPipe = false;
            if (gameState.Pipes.Count > 0)
            {
                var nearPipe = gameState.Pipes.FirstOrDefault(p => p.X > GameState.BirdX - 10 && p.X < GameState.BirdX + 20);
                if (nearPipe != null)
                {
                    // Dự đoán vị trí sau jump
                    float predictedYAfterJump = gameState.BirdY + jumpStrength;
                    willHitTopPipe = predictedYAfterJump <= nearPipe.TopHeight + 1; // +1 buffer
                }
            }
            
            if ((isNearCeiling || willHitTopPipe) && !isEmergencyFloor)
            {
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: CEILING_PROTECTION - No Jump (Y:{gameState.BirdY:F1}, WillHit:{willHitTopPipe})".PadRight(60));
                return; // Chỉ thoát khi gần trần và không ở emergency floor
            }
            
            // **FIND NEAREST PIPE**: Tìm ống gần nhất
            Pipe? nearestPipe = null;
            foreach (var pipe in gameState.Pipes)
            {
                if (pipe.X > GameState.BirdX - 5)
                {
                    nearestPipe = pipe;
                    break;
                }
            }
            
            bool shouldJump = false;
            string jumpReason = "";
            
            if (nearestPipe != null)
            {
                // **PIPE-AWARE STRATEGY**: Điều chỉnh dựa trên ống
                int gapTop = nearestPipe.TopHeight;
                int gapBottom = GameState.GameHeight - nearestPipe.BottomHeight;
                int gapCenter = (gapTop + gapBottom) / 2;
                int distanceToPipe = nearestPipe.X - GameState.BirdX;
                
                // **SAFE PIPE ZONES**: Vùng an toàn cho ống cụ thể với learning
                int learnedMargin = GetLearnedSafetyMargin(nearestPipe.GapSize);
                int pipeSafeTop = gapTop + Math.Max(2, learnedMargin);
                int pipeSafeBottom = gapBottom - Math.Max(2, learnedMargin);
                int pipeTarget = (pipeSafeTop + pipeSafeBottom) / 2;
                
                // **PREDICTION**: Dự đoán vị trí khi đến ống
                float timeToReach = distanceToPipe / 8f;
                float predictedY = gameState.BirdY + gameState.BirdVelocity * timeToReach;
                
                // **PIPE DECISIONS** với enhanced safety:
                
                // **CRITICAL CHECK**: Không nhảy nếu sẽ va chạm ống trên
                float jumpResultY = gameState.BirdY + jumpStrength;
                bool wouldHitTop = jumpResultY <= pipeSafeTop;
                
                // 0. **LEARNED DANGEROUS POSITION**: Tránh vị trí đã học là nguy hiểm
                if (IsLearnedDangerousPosition((int)gameState.BirdY, nearestPipe) && gameState.BirdY > 9 && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = "LEARNED_DANGER_AVOID";
                }
                // 1. **EMERGENCY FLOOR**: Tránh chạm đáy (ưu tiên cao nhất)
                else if (gameState.BirdY >= GameState.GameHeight - 3)
                {
                    shouldJump = true;
                    jumpReason = "EMERGENCY_FLOOR";
                }
                // 1.5. **DANGEROUS BOTTOM**: Tránh vùng nguy hiểm gần đáy
                else if (gameState.BirdY >= GameState.GameHeight - 5 && gameState.BirdVelocity > 0.2f)
                {
                    shouldJump = true;
                    jumpReason = "DANGEROUS_BOTTOM";
                }
                // 2. **PIPE BOTTOM COLLISION**: Tránh va chạm đáy ống (cân bằng + safe)
                else if (distanceToPipe <= 25 && predictedY > pipeSafeBottom && gameState.BirdY > 9 && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = "AVOID_PIPE_BOTTOM";
                }
                // 3. **PIPE ALIGNMENT**: Điều chỉnh về center của gap (cân bằng + safe)
                else if (distanceToPipe <= 35 && gameState.BirdY > pipeTarget + 2 && gameState.BirdY > 10 && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = "ALIGN_TO_PIPE";
                }
                // 4. **FALLING TOO FAST**: Kiểm soát tốc độ rơi (cân bằng + safe)
                else if (distanceToPipe <= 20 && gameState.BirdVelocity > 0.4f && gameState.BirdY > 9 && !wouldHitTop)
                {
                    shouldJump = true;
                    jumpReason = "CONTROL_FALL_SPEED";
                }
            }
            else
            {
                // **NO PIPE STRATEGY**: Giữ vùng an toàn khi không có ống
                
                // 1. **EMERGENCY FLOOR**: Tránh chạm đáy (ưu tiên cao nhất)
                if (gameState.BirdY >= GameState.GameHeight - 3)
                {
                    shouldJump = true;
                    jumpReason = "EMERGENCY_FLOOR";
                }
                // 1.5. **DANGEROUS BOTTOM**: Tránh vùng nguy hiểm gần đáy
                else if (gameState.BirdY >= GameState.GameHeight - 5 && gameState.BirdVelocity > 0.2f)
                {
                    shouldJump = true;
                    jumpReason = "DANGEROUS_BOTTOM";
                }
                // 2. **SAFE ZONE MAINTENANCE**: Về vùng an toàn (cân bằng)
                else if (gameState.BirdY > safeZoneBottom && gameState.BirdY > 10)
                {
                    shouldJump = true;
                    jumpReason = "BACK_TO_SAFE_ZONE";
                }
                // 3. **LEARNED OPTIMAL HEIGHT**: Điều chỉnh về độ cao đã học
                else if (Math.Abs(gameState.BirdY - idealHeight) > 3 && gameState.BirdY > idealHeight + 1 && gameState.BirdY > 11)
                {
                    shouldJump = true;
                    jumpReason = "LEARNED_HEIGHT_ADJUST";
                }
            }
            
            // **EXECUTE JUMP** với enhanced debug log
            if (shouldJump)
            {
                gameState.BirdVelocity = jumpStrength;
                lastJumpTime = DateTime.Now;
                
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: {jumpReason} (Y:{gameState.BirdY:F1}→{gameState.BirdY + jumpStrength:F1})".PadRight(60));
            }
            else
            {
                Console.SetCursorPosition(1, GameState.GameHeight + 2);
                Console.Write($"AI: COASTING (Y:{gameState.BirdY:F1}, V:{gameState.BirdVelocity:F2})".PadRight(60));
            }
        }
        
        /// <summary>
        /// Cập nhật học tập thông minh từ dữ liệu thất bại
        /// </summary>
        private static void UpdateLearning()
        {
            var failures = GameLogger.LoadGodModeFailures();
            if (failures.Count == 0) return;
            
            // **ENHANCED LEARNING**: Học nhiều pattern phức tạp hơn
            learnedPatterns.Clear();
            
            // Học về gap size và safety margin
            var gapGroups = failures.GroupBy(f => f.PipeGapSize);
            foreach (var group in gapGroups)
            {
                int gapSize = group.Key;
                var topCollisions = group.Count(f => f.CollisionType == "top");
                var bottomCollisions = group.Count(f => f.CollisionType == "bottom");
                var totalForGap = group.Count();
                
                if (totalForGap >= 2) // Học từ ít nhất 2 lần thất bại
                {
                    float topRate = (float)topCollisions / totalForGap;
                    float bottomRate = (float)bottomCollisions / totalForGap;
                    
                    // **SMART SAFETY MARGIN**: Tăng margin dựa trên loại va chạm chủ yếu
                    int safetyMargin = 1; // Base margin
                    
                    if (topRate > 0.4f) // Nếu va chạm ống trên nhiều
                    {
                        safetyMargin = Math.Min(4, (int)(topRate * 6)); // Tăng margin để tránh trần
                    }
                    else if (bottomRate > 0.5f) // Nếu va chạm ống dưới nhiều  
                    {
                        safetyMargin = Math.Max(1, (int)(bottomRate * 3)); // Margin vừa phải để không quá conservative
                    }
                    
                    learnedPatterns.Add(new SimplePattern
                    {
                        GapSize = gapSize,
                        SafetyMargin = safetyMargin,
                        Confidence = Math.Min(totalForGap / 5f, 1f),
                        TopCollisionRate = topRate,
                        BottomCollisionRate = bottomRate
                    });
                }
            }
        }
        
        /// <summary>
        /// Lấy safety margin đã học cho gap size cụ thể
        /// </summary>
        private static int GetLearnedSafetyMargin(int gapSize)
        {
            var pattern = learnedPatterns.FirstOrDefault(p => p.GapSize == gapSize);
            return pattern?.SafetyMargin ?? 1; // Default safety margin
        }
        
        /// <summary>
        /// Kiểm tra vị trí có nguy hiểm dựa trên kinh nghiệm đã học
        /// </summary>
        private static bool IsLearnedDangerousPosition(int birdY, Pipe pipe)
        {
            var failures = GameLogger.LoadGodModeFailures();
            if (failures.Count < 5) return false;
            
            // **SỬA LỖI**: Không coi vị trí gần trần (Y <= 3) là nguy hiểm
            if (birdY <= 3) return false;
            
            // Tìm các lần thất bại tương tự (cùng gap size và vị trí tương tự)
            int dangerousCount = failures.Count(f => 
                f.PipeGapSize == pipe.GapSize &&
                Math.Abs(f.BirdY - birdY) <= 2 &&
                f.CollisionType == "top");
            
            int totalSimilar = failures.Count(f => 
                f.PipeGapSize == pipe.GapSize &&
                Math.Abs(f.BirdY - birdY) <= 2);
            
            // Nếu > 40% lần ở vị trí này bị va chạm ống trên → nguy hiểm
            return totalSimilar >= 3 && (float)dangerousCount / totalSimilar > 0.4f;
        }
        
        /// <summary>
        /// Lấy độ cao tối ưu đã học - THỰC SỰ HỌC TỪ THẤT BẠI
        /// </summary>
        private static int GetLearnedOptimalHeight()
        {
            var failures = GameLogger.LoadGodModeFailures();
            
            // **CONSERVATIVE**: Luôn target ở giữa màn hình, tránh cả trần và đáy
            int defaultHeight = GameState.GameHeight / 2; // Chính giữa
            
            if (failures.Count < 3) return defaultHeight;
            
            // **SMART LEARNING**: Phân tích chi tiết các loại va chạm
            var topCollisions = failures.Where(f => f.CollisionType == "top").ToList();
            var bottomCollisions = failures.Where(f => f.CollisionType == "bottom").ToList();
            
            int targetHeight = defaultHeight;
            
            // Nếu va chạm ống trên nhiều hơn → bay thấp hơn
            if (topCollisions.Count > bottomCollisions.Count && topCollisions.Count >= 3)
            {
                int avgTopCollisionHeight = (int)topCollisions.Average(f => f.BirdY);
                targetHeight = Math.Max(avgTopCollisionHeight + 4, GameState.GameHeight / 2 + 2);
            }
            // Nếu va chạm ống dưới nhiều hơn → bay cao hơn một chút
            else if (bottomCollisions.Count > topCollisions.Count && bottomCollisions.Count >= 3)
            {
                int avgBottomCollisionHeight = (int)bottomCollisions.Average(f => f.BirdY);
                targetHeight = Math.Min(avgBottomCollisionHeight - 3, GameState.GameHeight / 2 - 1);
            }
            
            // **STRICT LIMITS**: Đảm bảo không bay quá cao hoặc quá thấp
            targetHeight = Math.Max(targetHeight, GameState.GameHeight / 3); // Không quá cao (tránh trần)
            targetHeight = Math.Min(targetHeight, GameState.GameHeight * 2 / 3); // Không quá thấp (tránh đáy)
            
            return targetHeight;
        }
    }
    
    /// <summary>
    /// Pattern thông minh AI học được
    /// </summary>
    public class SimplePattern
    {
        public int GapSize { get; set; }
        public int SafetyMargin { get; set; }
        public float Confidence { get; set; }
        public float TopCollisionRate { get; set; }
        public float BottomCollisionRate { get; set; }
    }
}
