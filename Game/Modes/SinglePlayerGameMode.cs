using System;
using FlappyBird.AI;
using FlappyBird.Models;
using FlappyBird.Utils;
using FlappyBird.Game.Modes.SinglePlayer;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ chơi đơn - Tối ưu hóa với border design nhất quán và tách thành components
    /// </summary>
    public class SinglePlayerGameMode : GameModeBase
    {
        private readonly GameState gameState = new();

        // === COMPONENTS ===
        private readonly SinglePlayerRenderer renderer;
        private readonly SinglePlayerGameOverMenu gameOverMenu;
        private readonly SinglePlayerInputHandler inputHandler;
        private readonly SinglePlayerInitializer initializer;

        // === CONSTRUCTOR ===
        public SinglePlayerGameMode()
        {
            renderer = new SinglePlayerRenderer();
            gameOverMenu = new SinglePlayerGameOverMenu(renderer);
            inputHandler = new SinglePlayerInputHandler(gameOverMenu);
            initializer = new SinglePlayerInitializer();
        }

        public override void Initialize()
        {
            gameState.Reset();

            // Initialize game với border dimensions chính xác
            initializer.InitializeGameWithMenuConsistentBorders(gameState, Random);

            // Initialize screen buffer với kích thước tối ưu
            renderer.InitializeScreen(gameState);

            // Render lần đầu với thiết kế 100% nhất quán với menu
            renderer.RenderWithConsistentDesign(gameState);
        }

        public override void Update()
        {
            gameState.FrameCounter++;

            // Nếu đang hiển thị game over menu, không update game logic
            if (gameOverMenu.ShowGameOverMenu)
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
            if (gameState.GameOver && !gameOverMenu.ShowGameOverMenu)
            {
                gameOverMenu.StartGameOverMenu();
            }
        }

        public override void Render()
        {
            // Nếu đang hiển thị game over menu
            if (gameOverMenu.ShowGameOverMenu)
            {
                // Cho phép người chơi nhìn thấy kết quả một chút trước khi hiển thị menu
                if (gameOverMenu.ShouldShowMenu())
                {
                    gameOverMenu.RenderGameOverMenu(gameState);
                }
                else
                {
                    // Hiển thị game state hiện tại với overlay "GAME OVER"
                    renderer.Draw(gameState);
                    renderer.RenderGameOverOverlay();
                }
                return;
            }

            // Sử dụng renderer mới với thiết kế nhất quán nếu cần full redraw
            if (gameState.ForceFullRedraw)
            {
                renderer.RenderWithConsistentDesign(gameState);
            }
            else
            {
                renderer.Draw(gameState);
            }
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            var inputResult = inputHandler.HandleInput(keyInfo, gameState);

            if (inputResult.ShouldExit)
            {
                shouldExit = true;
                return;
            }

            if (inputResult.ShouldRestart)
            {
                RestartGame();
                return;
            }

            if (inputResult.ShouldJump)
            {
                Jump(gameState);
            }
        }

        public override bool IsGameOver()
        {
            // Game chỉ kết thúc khi người chơi chọn thoát (shouldExit = true)
            // Không kết thúc khi gameState.GameOver = true vì lúc đó chúng ta đang hiển thị game over menu
            return shouldExit;
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
        /// Restart game với cùng cài đặt
        /// </summary>
        private void RestartGame()
        {
            gameOverMenu.ResetGameOverMenu();
            gameState.Reset();

            // Khởi tạo lại game với border dimensions chính xác
            initializer.InitializeGameWithMenuConsistentBorders(gameState, Random);

            // Force full redraw
            gameState.ForceFullRedraw = true;
        }
    }
}
