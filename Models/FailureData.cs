using System;

namespace FlappyBird.Models
{
    /// <summary>
    /// Class để lưu thông tin va chạm của God mode
    /// </summary>
    public class FailureData
    {
        public int Frame { get; set; }
        public int BirdY { get; set; }
        public float BirdVelocity { get; set; }
        public int PipeX { get; set; }
        public int PipeTopHeight { get; set; }
        public int PipeBottomHeight { get; set; }
        public int PipeGapSize { get; set; }
        public int Level { get; set; }
        public int Score { get; set; }
        public string CollisionType { get; set; } = ""; // "top", "bottom", "border"
        public DateTime Timestamp { get; set; }
    }
}
