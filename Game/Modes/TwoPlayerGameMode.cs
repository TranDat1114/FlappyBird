using System;
using FlappyBird.Models;
using FlappyBird.Game;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ chơi 2 người - Dual Screen Layout với thiết kế nhất quán và tối ưu rendering
    /// </summary>
    public class TwoPlayerGameMode : GameModeBase
    {
        private GameState player1State = new GameState();
        private GameState player2State = new GameState();
        private bool gameStarted = false;
        
        // === MENU CONSISTENCY CONSTANTS ===
        private const int MENU_BORDER_WIDTH = 66;  // Khớp chính xác với menu border
        private const int GAME_DISPLAY_HEIGHT = 11; // Mỗi player 11 dòng (giống chia đôi SinglePlayer)
        private const int PLAYER_SCREEN_HEIGHT = 13; // Header (1) + Info (1) + Border (1) + Game (11) + Border (1) = 13
        private const int TOTAL_DISPLAY_HEIGHT = 30; // 2 player + footer (4 dòng)
        
        // === DOUBLE BUFFERING FOR ANTI-FLICKER ===
        private char[,] previousBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private char[,] currentBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] previousColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] currentColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private bool bufferInitialized = false;
        private bool firstRender = true; // Track first render to clear screen
        
        // === COUNTDOWN STATE ===
        private bool isCountingDown = false;
        private int countdownValue = 3;
        private DateTime countdownStartTime;
        
        // === CONSTRUCTOR ===
        public TwoPlayerGameMode()
        {
            InitializeBuffers();
        }
        
        /// <summary>
        /// Initialize double buffering arrays to reduce flicker
        /// </summary>
        private void InitializeBuffers()
        {
            previousBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            currentBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            previousColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            currentColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            
            // Initialize with spaces and default color
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < MENU_BORDER_WIDTH; x++)
                {
                    previousBuffer[y, x] = ' ';
                    currentBuffer[y, x] = ' ';
                    previousColorBuffer[y, x] = ConsoleColor.White;
                    currentColorBuffer[y, x] = ConsoleColor.White;
                }
            }
            
            bufferInitialized = true;
        }
        
        /// <summary>
        /// Clear current buffer for next frame
        /// </summary>
        private void ClearCurrentBuffer()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < MENU_BORDER_WIDTH; x++)
                {
                    currentBuffer[y, x] = ' ';
                    currentColorBuffer[y, x] = ConsoleColor.White;
                }
            }
        }
        
        /// <summary>
        /// Write character to current buffer
        /// </summary>
        private void WriteToBuffer(int x, int y, char ch, ConsoleColor color = ConsoleColor.White)
        {
            if (x >= 0 && x < MENU_BORDER_WIDTH && y >= 0 && y < TOTAL_DISPLAY_HEIGHT)
            {
                currentBuffer[y, x] = ch;
                currentColorBuffer[y, x] = color;
            }
        }
        
        /// <summary>
        /// Render buffer to console - only draw changed characters
        /// </summary>
        private void FlushBufferToConsole()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < MENU_BORDER_WIDTH; x++)
                {
                    if (currentBuffer[y, x] != previousBuffer[y, x] || 
                        currentColorBuffer[y, x] != previousColorBuffer[y, x])
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = currentColorBuffer[y, x];
                        Console.Write(currentBuffer[y, x]);
                        
                        // Update previous buffer
                        previousBuffer[y, x] = currentBuffer[y, x];
                        previousColorBuffer[y, x] = currentColorBuffer[y, x];
                    }
                }
            }
            Console.ResetColor();
        }

        // === GAME OVER MENU STATE ===
        private bool showGameOverMenu = false;
        private int gameOverSelectedIndex = 0; // 0: Chơi lại, 1: Về menu chính
        private readonly string[] gameOverOptions = ["Choi lai", "Ve menu chinh"];
        private DateTime gameOverTime = DateTime.MinValue; // Thời gian bắt đầu game over
        
        public override void Initialize()
        {
            // Validate menu consistency như SinglePlayerGameMode
            ValidateMenuConsistency();
            
            player1State.Reset();
            player2State.Reset();
            
            // Reset game over states
            showGameOverMenu = false;
            gameOverSelectedIndex = 0;
            gameOverTime = DateTime.MinValue;
            
            // Initialize pipes cho cả hai players như SinglePlayerGameMode
            InitializeGameForPlayer(player1State);
            InitializeGameForPlayer(player2State);
            
            gameStarted = false;
            firstRender = true; // Reset first render flag
        }
        
        /// <summary>
        /// Validate rằng game dimensions khớp hoàn toàn với menu - tương tự SinglePlayerGameMode
        /// </summary>
        private void ValidateMenuConsistency()
        {
            if (GameState.GameWidth != MENU_BORDER_WIDTH)
            {
                throw new InvalidOperationException($"Game width ({GameState.GameWidth}) không khớp với menu border width ({MENU_BORDER_WIDTH})");
            }
        }
        
        /// <summary>
        /// Initialize game cho một player - dùng logic từ SinglePlayerGameMode
        /// </summary>
        private void InitializeGameForPlayer(GameState playerState)
        {
            playerState.Pipes.Clear();
            
            // Tạo pipe đầu tiên với spacing phù hợp với border width
            int initialPipeX = GameState.GameWidth - 1;
            playerState.Pipes.Add(new Pipe(initialPipeX, GameState.BaseGapSize, GameState.GameHeight, Random));
            
            // Set last pipe position để spacing đều đặn
            playerState.LastPipeX = initialPipeX;
        }
        
        public override void Update()
        {
            // Xử lý countdown trước khi bắt đầu game
            if (isCountingDown)
            {
                int elapsed = (int)(DateTime.Now - countdownStartTime).TotalSeconds;
                int newCountdown = 3 - elapsed;
                if (newCountdown != countdownValue)
                {
                    countdownValue = newCountdown;
                }
                if (countdownValue <= 0)
                {
                    isCountingDown = false;
                    gameStarted = true;
                    player1State.GameStarted = true;
                    player2State.GameStarted = true;
                }
                return;
            }
            if (!gameStarted) return;
            
            // Update both players
            UpdatePlayer(player1State);
            UpdatePlayer(player2State);
            
            // Kiểm tra game over và hiển thị menu thay vì thoát ngay - như SinglePlayerGameMode
            if (AreBothPlayersGameOver() && !showGameOverMenu)
            {
                showGameOverMenu = true;
                gameOverSelectedIndex = 0; // Reset về "Chơi lại"
                gameOverTime = DateTime.Now; // Ghi lại thời gian game over
            }
        }
        
        private void UpdatePlayer(GameState playerState)
        {
            if (playerState.GameOver) return;
            
            playerState.FrameCounter++;
            playerState.UpdateDifficulty();
            
            UpdateBirdPhysics(playerState);
            UpdatePipes(playerState);
            CheckCollision(playerState);
        }
        
        public override void Render()
        {
            // Clear screen completely on first render to remove previous menu
            if (firstRender)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                firstRender = false;
            }
            // Initialize buffers if needed
            if (!bufferInitialized)
            {
                InitializeBuffers();
            }
            // Clear current buffer for new frame
            ClearCurrentBuffer();
            // Nếu đang hiển thị game over menu - tương tự SinglePlayerGameMode
            if (showGameOverMenu)
            {
                // Cho phép người chơi nhìn thấy kết quả một chút trước khi hiển thị menu
                if (DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800))
                {
                    RenderGameOverMenuToBuffer();
                }
                else
                {
                    // Hiển thị game state hiện tại với overlay "GAME OVER"
                    RenderDualStackedScreensToBuffer();
                    RenderGameOverOverlayToBuffer();
                }
            }
            else if (isCountingDown)
            {
                // Render hai màn hình như bình thường
                RenderDualStackedScreensToBuffer();
                // Hiển thị số countdown lớn ở giữa mỗi màn hình player
                RenderCountdownOverlayToBuffer();
                // Render footer
                RenderDualPlayerFooterToBuffer();
            }
            else
            {
                // Render dual stacked screens với thiết kế nhất quán
                RenderDualStackedScreensToBuffer();
                // Render footer với thông tin cả hai player
                RenderDualPlayerFooterToBuffer();
            }
            // Flush buffer to console - only changed characters
            FlushBufferToConsole();
        }
        
        /// <summary>
        /// Render hai màn hình game xếp chồng vào buffer - tối ưu cho anti-flicker
        /// </summary>
        private void RenderDualStackedScreensToBuffer()
        {
            // === PLAYER 1 SCREEN ===
            RenderPlayerScreenToBuffer(player1State, "PLAYER 1", 0);
            
            // === PLAYER 2 SCREEN ===  
            RenderPlayerScreenToBuffer(player2State, "PLAYER 2", PLAYER_SCREEN_HEIGHT);
        }
        
        /// <summary>
        /// Render một màn hình player vào buffer tại vị trí chỉ định (giống SinglePlayer)
        /// </summary>
        private void RenderPlayerScreenToBuffer(GameState playerState, string playerName, int startY)
        {
            // Header
            WriteToBuffer(0, startY, '╔', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                WriteToBuffer(i, startY, '═', ConsoleColor.Cyan);
            WriteToBuffer(MENU_BORDER_WIDTH - 1, startY, '╗', ConsoleColor.Cyan);
            // Info line
            string info = $" {playerName} | Score: {playerState.Score} {(playerState.GameOver ? "(GAME OVER)" : "")}";
            WriteToBuffer(0, startY + 1, '║', ConsoleColor.Cyan);
            for (int i = 0; i < info.Length && i < MENU_BORDER_WIDTH - 2; i++)
                WriteToBuffer(i + 1, startY + 1, info[i], ConsoleColor.White);
            for (int i = info.Length + 1; i < MENU_BORDER_WIDTH - 1; i++)
                WriteToBuffer(i, startY + 1, ' ', ConsoleColor.White);
            WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 1, '║', ConsoleColor.Cyan);
            // Border dưới info
            WriteToBuffer(0, startY + 2, '╠', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                WriteToBuffer(i, startY + 2, '═', ConsoleColor.Cyan);
            WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 2, '╣', ConsoleColor.Cyan);
            // Game area
            char[,] screenBuffer = new char[GAME_DISPLAY_HEIGHT, GameState.GameWidth - 2];
            for (int y = 0; y < GAME_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < GameState.GameWidth - 2; x++)
                    screenBuffer[y, x] = ((x + y) % 4 == 0) ? '·' : ' ';
            DrawPipesIntoBuffer(screenBuffer, playerState);
            DrawBirdIntoBuffer(screenBuffer, playerState);
            for (int y = 0; y < GAME_DISPLAY_HEIGHT; y++)
            {
                WriteToBuffer(0, startY + 3 + y, '║', ConsoleColor.Cyan);
                for (int x = 0; x < GameState.GameWidth - 2; x++)
                {
                    char ch = screenBuffer[y, x];
                    ConsoleColor color = GetCharColor(ch);
                    WriteToBuffer(x + 1, startY + 3 + y, ch, color);
                }
                WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 3 + y, '║', ConsoleColor.Cyan);
            }
            // Bottom border
            WriteToBuffer(0, startY + 3 + GAME_DISPLAY_HEIGHT, '╚', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                WriteToBuffer(i, startY + 3 + GAME_DISPLAY_HEIGHT, '═', ConsoleColor.Cyan);
            WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 3 + GAME_DISPLAY_HEIGHT, '╝', ConsoleColor.Cyan);
        }
        
        /// <summary>
        /// Get color for character - similar to SetCharColor but returns color instead of setting it
        /// </summary>
        private ConsoleColor GetCharColor(char ch)
        {
            return ch switch
            {
                '█' => ConsoleColor.Green,
                'o' or 'Ø' or '◊' => ConsoleColor.Yellow,
                '·' => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };
        }

        /// <summary>
        /// Hiển thị overlay GAME OVER vào buffer
        /// </summary>
        private void RenderGameOverOverlayToBuffer()
        {
            string gameOverText = " GAME OVER! ";
            int startX = GameState.GameWidth / 2 - gameOverText.Length / 2;
            int overlayY = PLAYER_SCREEN_HEIGHT;
            
            for (int i = 0; i < gameOverText.Length; i++)
            {
                WriteToBuffer(startX + i, overlayY, gameOverText[i], ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Render footer với thông tin cả hai player vào buffer (giống SinglePlayer)
        /// </summary>
        private void RenderDualPlayerFooterToBuffer()
        {
            int footerY = TOTAL_DISPLAY_HEIGHT - 4;
            // Footer border
            for (int i = 0; i < MENU_BORDER_WIDTH; i++)
                WriteToBuffer(i, footerY, '═', ConsoleColor.Cyan);
            // Thông tin điểm số
            string scoreLine = $"Player 1: {player1State.Score} {(player1State.GameOver ? "(THUA)" : "")} | Player 2: {player2State.Score} {(player2State.GameOver ? "(THUA)" : "")}";
            for (int i = 0; i < scoreLine.Length && i < MENU_BORDER_WIDTH; i++)
                WriteToBuffer(i, footerY + 1, scoreLine[i], ConsoleColor.White);
            // Hướng dẫn controls
            string controlsLine = "[W] Player 1 bay | [↑] Player 2 bay | [ESC] Thoát | [SPACE] Bắt đầu";
            for (int i = 0; i < controlsLine.Length && i < MENU_BORDER_WIDTH; i++)
                WriteToBuffer(i, footerY + 2, controlsLine[i], ConsoleColor.Gray);
            // Footer border dưới
            for (int i = 0; i < MENU_BORDER_WIDTH; i++)
                WriteToBuffer(i, footerY + 3, '═', ConsoleColor.Cyan);
        }

        /// <summary>
        /// Render game over menu vào buffer
        /// </summary>
        private void RenderGameOverMenuToBuffer()
        {
            // Clear area first
            for (int y = 5; y < 20; y++)
            {
                for (int x = 10; x < 56; x++)
                {
                    WriteToBuffer(x, y, ' ', ConsoleColor.White);
                }
            }
            
            // Menu border
            int menuStartX = 15;
            int menuStartY = 8;
            int menuWidth = 36;
            int menuHeight = 8;
            
            // Draw border
            WriteToBuffer(menuStartX, menuStartY, '╔', ConsoleColor.White);
            for (int i = 1; i < menuWidth - 1; i++)
            {
                WriteToBuffer(menuStartX + i, menuStartY, '═', ConsoleColor.White);
            }
            WriteToBuffer(menuStartX + menuWidth - 1, menuStartY, '╗', ConsoleColor.White);
            
            // Menu content
            string[] menuLines = [
                "",
                "           GAME OVER",
                "",
                $"    Player 1 Score: {player1State.Score}",
                $"    Player 2 Score: {player2State.Score}",
                "",
                "    Choi lai",
                "    Ve menu chinh"
            ];
            
            for (int line = 0; line < menuLines.Length && line < menuHeight - 2; line++)
            {
                WriteToBuffer(menuStartX, menuStartY + 1 + line, '║', ConsoleColor.White);
                
                string text = menuLines[line];
                if (line == 6 || line == 7) // Menu options
                {
                    bool isSelected = (line == 6 && gameOverSelectedIndex == 0) || 
                                     (line == 7 && gameOverSelectedIndex == 1);
                    ConsoleColor textColor = isSelected ? ConsoleColor.Black : ConsoleColor.White;
                    ConsoleColor bgColor = isSelected ? ConsoleColor.White : ConsoleColor.Black;
                    
                    // For simplicity in buffer, use different characters for selection
                    if (isSelected)
                    {
                        text = ">>> " + text.Trim() + " <<<";
                    }
                }
                
                for (int i = 0; i < text.Length && i < menuWidth - 2; i++)
                {
                    WriteToBuffer(menuStartX + 1 + i, menuStartY + 1 + line, text[i], ConsoleColor.White);
                }
                
                WriteToBuffer(menuStartX + menuWidth - 1, menuStartY + 1 + line, '║', ConsoleColor.White);
            }
            
            // Bottom border
            WriteToBuffer(menuStartX, menuStartY + menuHeight - 1, '╚', ConsoleColor.White);
            for (int i = 1; i < menuWidth - 1; i++)
            {
                WriteToBuffer(menuStartX + i, menuStartY + menuHeight - 1, '═', ConsoleColor.White);
            }
            WriteToBuffer(menuStartX + menuWidth - 1, menuStartY + menuHeight - 1, '╝', ConsoleColor.White);
        }

        /// <summary>
        /// Vẽ pipes vào buffer - logic từ GameRenderer
        /// </summary>
        private void DrawPipesIntoBuffer(char[,] buffer, GameState playerState)
        {
            foreach (var pipe in playerState.Pipes)
            {
                DrawSinglePipeIntoBuffer(buffer, pipe);
            }
        }
        
        /// <summary>
        /// Vẽ một pipe vào buffer - logic từ GameRenderer 
        /// </summary>
        private void DrawSinglePipeIntoBuffer(char[,] buffer, Pipe pipe)
        {
            // Scale pipe position cho display nhỏ hơn
            float scaleX = (float)(GameState.GameWidth - 2) / GameState.GameWidth;
            float scaleY = (float)GAME_DISPLAY_HEIGHT / GameState.GameHeight;
            
            int scaledPipeX = (int)(pipe.X * scaleX);
            int scaledTopHeight = (int)(pipe.TopHeight * scaleY);
            int scaledBottomHeight = (int)(pipe.BottomHeight * scaleY);
            
            // Vẽ pipe trên
            for (int y = 0; y <= scaledTopHeight && y < GAME_DISPLAY_HEIGHT; y++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = scaledPipeX + dx;
                    if (x >= 0 && x < GameState.GameWidth - 2)
                    {
                        buffer[y, x] = '█';
                    }
                }
            }
            
            // Vẽ pipe dưới
            for (int y = GAME_DISPLAY_HEIGHT - scaledBottomHeight; y < GAME_DISPLAY_HEIGHT; y++)
            {
                if (y >= 0)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int x = scaledPipeX + dx;
                        if (x >= 0 && x < GameState.GameWidth - 2)
                        {
                            buffer[y, x] = '█';
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Vẽ bird vào buffer - logic từ GameRenderer với animation tương tự SinglePlayerGameMode
        /// </summary>
        private void DrawBirdIntoBuffer(char[,] buffer, GameState playerState)
        {
            // Scale bird position cho display nhỏ hơn
            float scaleX = (float)(GameState.GameWidth - 2) / GameState.GameWidth;
            float scaleY = (float)GAME_DISPLAY_HEIGHT / GameState.GameHeight;
            
            int scaledBirdX = (int)(GameState.BirdX * scaleX);
            int scaledBirdY = (int)(playerState.BirdY * scaleY);
            
            if (scaledBirdX >= 0 && scaledBirdX < GameState.GameWidth - 2 && 
                scaledBirdY >= 0 && scaledBirdY < GAME_DISPLAY_HEIGHT)
            {
                // Animation cho chim - dựa vào frame counter như SinglePlayerGameMode
                char birdChar = GetBirdCharacter(playerState.FrameCounter, playerState.BirdVelocity);
                buffer[scaledBirdY, scaledBirdX] = birdChar;
            }
        }
        
        /// <summary>
        /// Lấy ký tự bird với animation - tương tự SinglePlayerGameMode
        /// </summary>
        private char GetBirdCharacter(int frameCounter, float velocity)
        {
            // Hiệu ứng "cánh chim" theo frame
            bool wingUp = (frameCounter / 3) % 2 == 0;
            
            // Thay đổi hình dạng theo vận tốc (hướng bay)
            if (velocity < -2) // Bay lên nhanh
            {
                return wingUp ? 'Ø' : 'ø';
            }
            else if (velocity > 2) // Rơi nhanh
            {
                return wingUp ? '◊' : '♦';
            }
            else // Bay bình thường
            {
                return wingUp ? 'o' : '°';
            }
        }
        
        /// <summary>
        /// Set màu cho ký tự - tương tự GameRenderer
        /// </summary>
        private void SetCharColor(char ch)
        {
            Console.ForegroundColor = ch switch
            {
                '█' => ConsoleColor.Green,
                'o' or 'Ø' or '◊' or 'ø' or '°' or '♦' => ConsoleColor.Yellow,
                '·' => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };
        }

        /// <summary>
        /// Center text trong một độ rộng nhất định
        /// </summary>
        private string CenterText(string text, int width)
        {
            if (text.Length >= width) return text.Substring(0, width);
            
            int padding = (width - text.Length) / 2;
            return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
        }
        
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            // Xử lý input cho game over menu (chỉ sau khi delay) - như SinglePlayerGameMode
            if (showGameOverMenu)
            {
                // Chỉ cho phép input sau khi delay để người chơi thấy kết quả
                if (DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800))
                {
                    HandleGameOverMenuInput(keyInfo);
                }
                return;
            }
            
            // Nếu đang đếm ngược thì không nhận input khác
            if (isCountingDown)
                return;
            
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted && !isCountingDown)
                    {
                        isCountingDown = true;
                        countdownValue = 3;
                        countdownStartTime = DateTime.Now;
                    }
                    break;
                case ConsoleKey.W:
                    if (gameStarted && !player1State.GameOver)
                    {
                        Jump(player1State);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (gameStarted && !player2State.GameOver)
                    {
                        Jump(player2State);
                    }
                    break;
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }
        
        /// <summary>
        /// Xử lý input cho game over menu - tương tự SinglePlayerGameMode
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
        /// Restart game với cùng cài đặt - tương tự SinglePlayerGameMode
        /// </summary>
        private void RestartGame()
        {
            showGameOverMenu = false;
            player1State.Reset();
            player2State.Reset();

            // Khởi tạo lại game với border dimensions chính xác
            InitializeGameForPlayer(player1State);
            InitializeGameForPlayer(player2State);

            gameStarted = false;
            firstRender = true; // Reset để clear screen khi restart
        }
        
        /// <summary>
        /// Jump method cho player - tương tự SinglePlayerGameMode
        /// </summary>
        protected new void Jump(GameState playerState)
        {
            if (!playerState.GameStarted)
            {
                playerState.GameStarted = true;
            }

            playerState.BirdVelocity = GameState.JumpStrength;
        }
        
        public override bool IsGameOver()
        {
            // Game chỉ kết thúc khi người chơi chọn thoát (shouldExit = true) - như SinglePlayerGameMode
            // Không kết thúc khi cả hai player GameOver = true vì lúc đó chúng ta đang hiển thị game over menu
            return shouldExit;
        }
        
        /// <summary>
        /// Check nếu cả hai player đã game over để trigger menu
        /// </summary>
        private bool AreBothPlayersGameOver()
        {
            return player1State.GameOver && player2State.GameOver;
        }
        
        /// <summary>
        /// Xác định người thắng
        /// </summary>
        private string GetWinner()
        {
            if (player1State.GameOver && player2State.GameOver)
            {
                if (player1State.Score > player2State.Score)
                    return "PLAYER 1";
                else if (player2State.Score > player1State.Score)
                    return "PLAYER 2";
                else
                    return "HOA";
            }
            else if (player1State.GameOver)
                return "PLAYER 2";
            else if (player2State.GameOver)
                return "PLAYER 1";
            
            return "DANG CHOI";
        }

        /// <summary>
        /// Hiển thị hiệu ứng countdown đặc biệt ở giữa mỗi màn hình player (giống đua xe)
        /// </summary>
        private void RenderCountdownOverlayToBuffer()
        {
            // Tính vị trí trung tâm cho mỗi player
            int centerX = MENU_BORDER_WIDTH / 2;
            int centerY1 = PLAYER_SCREEN_HEIGHT / 2 + 1;
            int centerY2 = PLAYER_SCREEN_HEIGHT + (PLAYER_SCREEN_HEIGHT / 2) + 1;

            // Hiệu ứng số lớn
            string[] bigNumbers = new string[4];
            ConsoleColor color = ConsoleColor.Yellow;
            // bool blink = false;
            string display = countdownValue > 0 ? countdownValue.ToString() : "GO!";

            if (countdownValue == 3)
            {
                bigNumbers = [
                    "  █████  ",
                    " ██   ██ ",
                    "      ██ ",
                    "    ███   ",
                    "      ██  ",
                    " ██   ██ ",
                    "  █████  "
                ];
                color = ConsoleColor.Yellow;
            }
            else if (countdownValue == 2)
            {
                bigNumbers = [
                    "  █████  ",
                    " ██   ██ ",
                    "      ██ ",
                    "   ███   ",
                    "  ██     ",
                    " ██      ",
                    " ███████ "
                ];
                color = ConsoleColor.Yellow;
            }
            else if (countdownValue == 1)
            {
                bigNumbers = [
                    "    ██   ",
                    "   ███   ",
                    "  ████   ",
                    "    ██   ",
                    "    ██   ",
                    "    ██   ",
                    "  ██████ "
                ];
                // Nhấp nháy đỏ vàng
                color = (DateTime.Now.Millisecond < 500) ? ConsoleColor.Red : ConsoleColor.Yellow;
                // blink = true;
            }
            else // GO!
            {
                bigNumbers = [
                    "  █████   ██████  ",
                    " ██   ██ ██    ██ ",
                    " ██   ██ ██    ██ ",
                    " ██   ██ ██    ██ ",
                    " ██   ██ ██    ██ ",
                    " ██   ██ ██    ██ ",
                    "  █████   ██████  "
                ];
                color = ConsoleColor.Green;
            }

            // Vẽ cho cả 2 player
            void DrawBigNumber(int centerY)
            {
                int startY = centerY - bigNumbers.Length / 2;
                int startX = centerX - bigNumbers[0].Length / 2;
                for (int row = 0; row < bigNumbers.Length; row++)
                {
                    for (int col = 0; col < bigNumbers[row].Length; col++)
                    {
                        char ch = bigNumbers[row][col];
                        if (ch != ' ')
                        {
                            WriteToBuffer(startX + col, startY + row, ch, color);
                        }
                    }
                }
            }
            DrawBigNumber(centerY1);
            DrawBigNumber(centerY2);
        }
    }
}
