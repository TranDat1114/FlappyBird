using System;
using FlappyBird.AI;
using FlappyBird.Models;
using FlappyBird.Utils;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ chơi đơn - Tối ưu hóa với border design nhất quán
    /// </summary>
    public class SinglePlayerGameMode : GameModeBase
    {
        private GameState gameState = new GameState();
        
        // === MENU CONSISTENCY CONSTANTS ===
        private const int MENU_BORDER_WIDTH = 66;  // Khớp chính xác với menu border
        private const int GAME_DISPLAY_HEIGHT = 22; // Chiều cao vùng game
        private const int TOTAL_DISPLAY_HEIGHT = 30; // Tổng chiều cao (game + header + footer)
        
        // === GAME OVER MENU STATE ===
        private bool showGameOverMenu = false;
        private int gameOverSelectedIndex = 0; // 0: Chơi lại, 1: Thoát
        private readonly string[] gameOverOptions = { "Choi lai", "Ve menu chinh" };
        
        public override void Initialize()
        {
            // Đảm bảo dimensions khớp với menu design
            ValidateMenuConsistency();
            
            gameState.Reset();
            
            // Initialize game với border dimensions chính xác
            InitializeGameWithMenuConsistentBorders();
            
            // Initialize screen buffer với kích thước tối ưu
            GameRenderer.InitializeScreen(gameState);
            
            // Render lần đầu với thiết kế 100% nhất quán với menu
            GameRenderer.RenderWithConsistentDesign(gameState);
        }
        
        /// <summary>
        /// Validate rằng game dimensions khớp hoàn toàn với menu
        /// </summary>
        private void ValidateMenuConsistency()
        {
            if (GameState.GameWidth != MENU_BORDER_WIDTH)
            {
                throw new InvalidOperationException($"Game width ({GameState.GameWidth}) không khớp với menu border width ({MENU_BORDER_WIDTH})");
            }
            
            if (GameState.GameHeight != GAME_DISPLAY_HEIGHT)
            {
                throw new InvalidOperationException($"Game height ({GameState.GameHeight}) không phù hợp với display height ({GAME_DISPLAY_HEIGHT})");
            }
        }
        
        /// <summary>
        /// Khởi tạo game với border hoàn toàn nhất quán với menu design
        /// </summary>
        private void InitializeGameWithMenuConsistentBorders()
        {
            gameState.Pipes.Clear();
            
            // Tạo pipe đầu tiên với spacing phù hợp với border width
            int initialPipeX = GameState.GameWidth - 1;
            gameState.Pipes.Add(new Pipe(initialPipeX, GameState.BaseGapSize, GameState.GameHeight, Random));
            
            // Set last pipe position để spacing đều đặn
            gameState.LastPipeX = initialPipeX;
        }
        
        public override void Update()
        {
            gameState.FrameCounter++;
            
            // Nếu đang hiển thị game over menu, không update game logic
            if (showGameOverMenu)
            {
                return;
            }
            
            if (!gameState.GameStarted)
            {
                return;
            }
            
            gameState.UpdateDifficulty();
            
            UpdateBirdPhysics(gameState);
            UpdatePipes(gameState);
            CheckCollision(gameState);
            
            // Kiểm tra game over và hiển thị menu thay vì thoát ngay
            if (gameState.GameOver && !showGameOverMenu)
            {
                showGameOverMenu = true;
                gameOverSelectedIndex = 0; // Reset về "Chơi lại"
            }
        }
        
        public override void Render()
        {
            // Nếu đang hiển thị game over menu
            if (showGameOverMenu)
            {
                RenderGameOverMenu();
                return;
            }
            
            // Sử dụng renderer mới với thiết kế nhất quán nếu cần full redraw
            if (gameState.ForceFullRedraw)
            {
                GameRenderer.RenderWithConsistentDesign(gameState);
            }
            else
            {
                GameRenderer.Draw(gameState);
            }
        }
        
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            // Xử lý input cho game over menu
            if (showGameOverMenu)
            {
                HandleGameOverMenuInput(keyInfo);
                return;
            }
            
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    Jump(gameState);
                    break;
                    
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }
        
        public override bool IsGameOver()
        {
            return gameState.GameOver || shouldExit;
        }
        
        public GameState GetGameState()
        {
            return gameState;
        }
        
        protected new void CheckCollision(GameState gameState)
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
        
        protected new void Jump(GameState gameState)
        {
            if (!gameState.GameStarted)
            {
                gameState.GameStarted = true;
            }
            
            gameState.BirdVelocity = GameState.JumpStrength;
        }
        
        /// <summary>
        /// Render game over menu với thiết kế đẹp
        /// </summary>
        private void RenderGameOverMenu()
        {
            // Vẽ game hiện tại để người chơi thấy kết quả
            GameRenderer.Draw(gameState);
            
            // Overlay game over menu
            Console.SetCursorPosition(0, GameState.GameHeight + 4);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         GAME OVER!                            ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║                      Score: {gameState.Score,3} diem                        ║");
            Console.WriteLine($"║                      Level: {gameState.DifficultyLevel,3}                             ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            
            // Menu options
            for (int i = 0; i < gameOverOptions.Length; i++)
            {
                if (i == gameOverSelectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"║  > {gameOverOptions[i],-58} ║");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"║    {gameOverOptions[i],-58} ║");
                }
            }
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║    Up/Down: Chon    Enter: Xac nhan    Space: Choi lai         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Xử lý input cho game over menu
        /// </summary>
        private void HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex > 0 ? gameOverSelectedIndex - 1 : gameOverOptions.Length - 1;
                    break;
                    
                case ConsoleKey.DownArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex < gameOverOptions.Length - 1 ? gameOverSelectedIndex + 1 : 0;
                    break;
                    
                case ConsoleKey.Enter:
                    if (gameOverSelectedIndex == 0)
                    {
                        // Chọn "Chơi lại"
                        RestartGame();
                    }
                    else
                    {
                        // Chọn "Thoát"
                        shouldExit = true;
                    }
                    break;
                    
                case ConsoleKey.Spacebar:
                    // Shortcut để restart nhanh
                    RestartGame();
                    break;
                    
                case ConsoleKey.Escape:
                    // Thoát
                    shouldExit = true;
                    break;
            }
        }
        
        /// <summary>
        /// Restart game với cùng cài đặt
        /// </summary>
        private void RestartGame()
        {
            showGameOverMenu = false;
            gameState.Reset();
            
            // Khởi tạo lại game với border dimensions chính xác
            InitializeGameWithMenuConsistentBorders();
            
            // Force full redraw
            gameState.ForceFullRedraw = true;
        }
    }
}
