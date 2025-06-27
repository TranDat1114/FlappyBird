using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using FlappyBird.Models;

namespace FlappyBird.Utils
{
    /// <summary>
    /// Utility class để ghi và đọc dữ liệu va chạm của God mode
    /// </summary>
    public static class GameLogger
    {
        private static readonly string GodModeFailuresFile = "godmode_failures.json";
        
        /// <summary>
        /// Ghi lại thông tin va chạm của God mode với pipe
        /// </summary>
        public static void RecordGodModeFailure(GameState gameState, Pipe pipe)
        {
            try
            {
                var failures = LoadGodModeFailures();
                
                var failure = new FailureData
                {
                    Frame = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                    BirdY = gameState.BirdY,
                    BirdVelocity = gameState.BirdVelocity,
                    PipeX = pipe.X,
                    PipeTopHeight = pipe.TopHeight,
                    PipeBottomHeight = pipe.BottomHeight,
                    PipeGapSize = pipe.GapSize,
                    Level = gameState.DifficultyLevel,
                    Score = gameState.Score,
                    CollisionType = gameState.BirdY <= pipe.TopHeight ? "top" : "bottom",
                    Timestamp = DateTime.Now
                };
                
                failures.Add(failure);
                SaveGodModeFailures(failures);
            }
            catch (Exception)
            {
                // Không hiển thị lỗi trong game, chỉ bỏ qua
            }
        }
        
        /// <summary>
        /// Ghi lại thông tin va chạm của God mode với biên
        /// </summary>
        public static void RecordGodModeBorderFailure(GameState gameState)
        {
            try
            {
                var failures = LoadGodModeFailures();
                
                var failure = new FailureData
                {
                    Frame = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                    BirdY = gameState.BirdY,
                    BirdVelocity = gameState.BirdVelocity,
                    PipeX = -1, // Không có pipe
                    PipeTopHeight = 0,
                    PipeBottomHeight = 0,
                    PipeGapSize = 0,
                    Level = gameState.DifficultyLevel,
                    Score = gameState.Score,
                    CollisionType = "border",
                    Timestamp = DateTime.Now
                };
                
                failures.Add(failure);
                SaveGodModeFailures(failures);
            }
            catch (Exception)
            {
                // Không hiển thị lỗi trong game, chỉ bỏ qua
            }
        }
        
        /// <summary>
        /// Lưu dữ liệu va chạm vào file
        /// </summary>
        private static void SaveGodModeFailures(List<FailureData> failures)
        {
            try
            {
                string json = JsonSerializer.Serialize(failures, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(GodModeFailuresFile, json);
            }
            catch (Exception)
            {
                // Không hiển thị lỗi trong game, chỉ bỏ qua
            }
        }
        
        /// <summary>
        /// Tải dữ liệu va chạm từ file
        /// </summary>
        public static List<FailureData> LoadGodModeFailures()
        {
            try
            {
                if (File.Exists(GodModeFailuresFile))
                {
                    string json = File.ReadAllText(GodModeFailuresFile);
                    var failures = JsonSerializer.Deserialize<List<FailureData>>(json);
                    return failures ?? new List<FailureData>();
                }
            }
            catch (Exception)
            {
                // Nếu có lỗi, tạo danh sách mới
            }
            
            return new List<FailureData>();
        }
        
        /// <summary>
        /// Kiểm tra xem vị trí hiện tại có nguy hiểm dựa trên dữ liệu học được
        /// </summary>
        public static bool IsHazardousPosition(int y, int pipeX, int pipeTopHeight, int pipeBottomHeight)
        {
            var failures = LoadGodModeFailures();
            if (failures.Count == 0) return false;
            
            // Tìm các va chạm tương tự trong lịch sử
            int hazardCount = 0;
            int totalSimilar = 0;
            
            foreach (var failure in failures)
            {
                // Kiểm tra vị trí tương tự
                if (Math.Abs(failure.BirdY - y) <= 2 && 
                    Math.Abs(failure.PipeTopHeight - pipeTopHeight) <= 1 &&
                    Math.Abs(failure.PipeBottomHeight - pipeBottomHeight) <= 1)
                {
                    totalSimilar++;
                    
                    // Nếu khoảng cách pipe tương tự
                    if (Math.Abs(failure.PipeX - pipeX) <= 5)
                    {
                        hazardCount++;
                    }
                }
            }
            
            // Nếu có quá nhiều va chạm ở vị trí tương tự, coi là nguy hiểm
            return totalSimilar > 0 && (float)hazardCount / totalSimilar > 0.3f;
        }
        
        /// <summary>
        /// Phân tích dữ liệu failures để cải thiện AI
        /// </summary>
        public static AIImprovementData AnalyzeFailures()
        {
            var failures = LoadGodModeFailures();
            var improvement = new AIImprovementData();
            
            if (failures.Count == 0) return improvement;
            
            // Phân tích các loại va chạm phổ biến
            int topCollisions = 0;
            int bottomCollisions = 0;
            int borderCollisions = 0;
            
            foreach (var failure in failures)
            {
                switch (failure.CollisionType)
                {
                    case "top": topCollisions++; break;
                    case "bottom": bottomCollisions++; break;
                    case "border": borderCollisions++; break;
                }
            }
            
            improvement.TotalFailures = failures.Count;
            improvement.TopCollisionRate = (float)topCollisions / failures.Count;
            improvement.BottomCollisionRate = (float)bottomCollisions / failures.Count;
            improvement.BorderCollisionRate = (float)borderCollisions / failures.Count;
            
            // Tính điểm trung bình khi thua
            improvement.AverageScoreAtFailure = failures.Count > 0 ? 
                failures.Sum(f => f.Score) / (float)failures.Count : 0;
            
            // Tìm level khó nhất
            improvement.HardestLevel = failures.Count > 0 ? 
                failures.Max(f => f.Level) : 1;
            
            return improvement;
        }
        
        /// <summary>
        /// Lấy điểm cao nhất từ dữ liệu
        /// </summary>
        public static int GetBestScore()
        {
            var failures = LoadGodModeFailures();
            return failures.Count > 0 ? failures.Max(f => f.Score) : 0;
        }
        
        /// <summary>
        /// Xóa dữ liệu cũ để reset learning
        /// </summary>
        public static void ClearFailureData()
        {
            try
            {
                if (File.Exists(GodModeFailuresFile))
                {
                    File.Delete(GodModeFailuresFile);
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi
            }
        }
        
        /// <summary>
        /// Lưu thống kê God mode vào file
        /// </summary>
        public static void SaveGodModeStats(GameState gameState)
        {
            try
            {
                var stats = new GodModeStats
                {
                    Attempts = gameState.GodModeAttempts,
                    BestScore = gameState.GodModeBestScore,
                    AutoRestart = gameState.GodModeAutoRestart,
                    LastUpdated = DateTime.Now
                };
                
                string json = JsonSerializer.Serialize(stats, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText("godmode_stats.json", json);
            }
            catch (Exception)
            {
                // Không hiển thị lỗi trong game, chỉ bỏ qua
            }
        }
        
        /// <summary>
        /// Tải thống kê God mode từ file
        /// </summary>
        public static void LoadGodModeStats(GameState gameState)
        {
            try
            {
                if (File.Exists("godmode_stats.json"))
                {
                    string json = File.ReadAllText("godmode_stats.json");
                    var stats = JsonSerializer.Deserialize<GodModeStats>(json);
                    if (stats != null)
                    {
                        gameState.GodModeAttempts = stats.Attempts;
                        gameState.GodModeBestScore = stats.BestScore;
                        gameState.GodModeAutoRestart = stats.AutoRestart;
                    }
                }
            }
            catch (Exception)
            {
                // Nếu có lỗi, giữ nguyên giá trị mặc định
            }
        }
        
        /// <summary>
        /// Xóa thống kê God mode
        /// </summary>
        public static void ClearGodModeStats()
        {
            try
            {
                if (File.Exists("godmode_stats.json"))
                {
                    File.Delete("godmode_stats.json");
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi
            }
        }
    }
    
    /// <summary>
    /// Dữ liệu phân tích để cải thiện AI
    /// </summary>
    public class AIImprovementData
    {
        public int TotalFailures { get; set; }
        public float TopCollisionRate { get; set; }
        public float BottomCollisionRate { get; set; }
        public float BorderCollisionRate { get; set; }
        public float AverageScoreAtFailure { get; set; }
        public int HardestLevel { get; set; }
        
        /// <summary>
        /// Tính toán độ conservative cần thiết dựa trên dữ liệu
        /// </summary>
        public float GetRecommendedConservativeness()
        {
            // Nếu va chạm ống trên nhiều -> tăng conservative
            float conservativeness = 0.5f; // Base level
            
            if (TopCollisionRate > 0.3f) conservativeness += 0.3f;
            if (BottomCollisionRate > 0.5f) conservativeness += 0.2f;
            if (BorderCollisionRate > 0.2f) conservativeness += 0.1f;
            
            return Math.Min(1.0f, conservativeness);
        }
    }
    
    /// <summary>
    /// Dữ liệu thống kê God mode
    /// </summary>
    public class GodModeStats
    {
        public int Attempts { get; set; }
        public int BestScore { get; set; }
        public bool AutoRestart { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
