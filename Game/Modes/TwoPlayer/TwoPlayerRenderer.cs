using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Xử lý rendering cho TwoPlayerGameMode
    /// </summary>
    public class TwoPlayerRenderer
    {
        // === CONSTANTS ===
        private const int MENU_BORDER_WIDTH = 66;
        private const int GAME_DISPLAY_HEIGHT = 11;
        private const int PLAYER_SCREEN_HEIGHT = 15;
        private const int FOOTER_HEIGHT = 6;
        private const int TOTAL_DISPLAY_HEIGHT = 36;

        private readonly TwoPlayerBuffer buffer;

        public TwoPlayerRenderer(TwoPlayerBuffer buffer)
        {
            this.buffer = buffer;
        }

        /// <summary>
        /// Render hai màn hình game xếp chồng vào buffer - tối ưu cho anti-flicker
        /// </summary>
        public void RenderDualStackedScreensToBuffer(GameState player1State, GameState player2State)
        {
            // === PLAYER 1 SCREEN ===
            RenderPlayerScreenToBuffer(player1State, "PLAYER 1", 0);

            // === PLAYER 2 SCREEN ===  
            RenderPlayerScreenToBuffer(player2State, "PLAYER 2", PLAYER_SCREEN_HEIGHT);
        }

        /// <summary>
        /// Render một màn hình player vào buffer tại vị trí chỉ định (giống SinglePlayer)
        /// </summary>
        public void RenderPlayerScreenToBuffer(GameState playerState, string playerName, int startY)
        {
            // Header
            buffer.WriteToBuffer(0, startY, '╔', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, startY, '═', ConsoleColor.Cyan);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, startY, '╗', ConsoleColor.Cyan);
            
            // Info line
            string info = $" {playerName} | Score: {playerState.Score} {(playerState.GameOver ? "(GAME OVER)" : "")}";
            buffer.WriteToBuffer(0, startY + 1, '║', ConsoleColor.Cyan);
            for (int i = 0; i < info.Length && i < MENU_BORDER_WIDTH - 2; i++)
                buffer.WriteToBuffer(i + 1, startY + 1, info[i], ConsoleColor.White);
            for (int i = info.Length + 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, startY + 1, ' ', ConsoleColor.White);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 1, '║', ConsoleColor.Cyan);
            
            // Border dưới info
            buffer.WriteToBuffer(0, startY + 2, '╠', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, startY + 2, '═', ConsoleColor.Cyan);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 2, '╣', ConsoleColor.Cyan);
            
            // Game area
            char[,] screenBuffer = new char[GAME_DISPLAY_HEIGHT, GameState.GameWidth - 2];
            for (int y = 0; y < GAME_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < GameState.GameWidth - 2; x++)
                    screenBuffer[y, x] = ((x + y) % 4 == 0) ? '·' : ' ';
            
            DrawPipesIntoBuffer(screenBuffer, playerState);
            DrawBirdIntoBuffer(screenBuffer, playerState);
            
            for (int y = 0; y < GAME_DISPLAY_HEIGHT; y++)
            {
                buffer.WriteToBuffer(0, startY + 3 + y, '║', ConsoleColor.Cyan);
                for (int x = 0; x < GameState.GameWidth - 2; x++)
                {
                    char ch = screenBuffer[y, x];
                    ConsoleColor color = GetCharColor(ch);
                    buffer.WriteToBuffer(x + 1, startY + 3 + y, ch, color);
                }
                buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 3 + y, '║', ConsoleColor.Cyan);
            }
            
            // Bottom border
            buffer.WriteToBuffer(0, startY + 3 + GAME_DISPLAY_HEIGHT, '╚', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, startY + 3 + GAME_DISPLAY_HEIGHT, '═', ConsoleColor.Cyan);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, startY + 3 + GAME_DISPLAY_HEIGHT, '╝', ConsoleColor.Cyan);
        }

        /// <summary>
        /// Render footer với 2 khung riêng biệt cho điểm số và hướng dẫn
        /// </summary>
        public void RenderDualPlayerFooterToBuffer(GameState player1State, GameState player2State)
        {
            int footerY = TOTAL_DISPLAY_HEIGHT - FOOTER_HEIGHT; // Bắt đầu footer từ vị trí chính xác

            // === KHUNG ĐIỂM SỐ ===
            // Viền trên khung điểm số
            buffer.WriteToBuffer(0, footerY, '╔', ConsoleColor.Yellow);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY, '═', ConsoleColor.Yellow);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY, '╗', ConsoleColor.Yellow);

            // Nội dung điểm số
            buffer.WriteToBuffer(0, footerY + 1, '║', ConsoleColor.Yellow);
            string scoreLine = $" ĐIỂM SỐ: Player 1: {player1State.Score} {(player1State.GameOver ? "(THUA)" : "")} | Player 2: {player2State.Score} {(player2State.GameOver ? "(THUA)" : "")}";
            for (int i = 0; i < scoreLine.Length && i < MENU_BORDER_WIDTH - 2; i++)
                buffer.WriteToBuffer(i + 1, footerY + 1, scoreLine[i], ConsoleColor.White);
            for (int i = scoreLine.Length + 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY + 1, ' ', ConsoleColor.White);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY + 1, '║', ConsoleColor.Yellow);

            // Viền dưới khung điểm số / viền trên khung hướng dẫn
            buffer.WriteToBuffer(0, footerY + 2, '╠', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY + 2, '═', ConsoleColor.Cyan);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY + 2, '╣', ConsoleColor.Cyan);

            // === KHUNG HƯỚNG DẪN ===
            // Nội dung hướng dẫn
            buffer.WriteToBuffer(0, footerY + 3, '║', ConsoleColor.Cyan);
            string controlsLine_1 = " ĐIỀU KHIỂN: [W] Player 1 bay | [↑] Player 2 bay";
            string controlsLine_2 = " PHÍM LỆNH : [ESC] Thoát | [SPACE] Bắt đầu/Chơi lại";
            for (int i = 0; i < controlsLine_1.Length && i < MENU_BORDER_WIDTH - 2; i++)
                buffer.WriteToBuffer(i + 1, footerY + 3, controlsLine_1[i], ConsoleColor.Gray);
            for (int i = controlsLine_1.Length + 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY + 3, ' ', ConsoleColor.Gray);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY + 3, '║', ConsoleColor.Cyan);

            // Line 2 hướng dẫn
            buffer.WriteToBuffer(0, footerY + 4, '║', ConsoleColor.Cyan);
            for (int i = 0; i < controlsLine_2.Length && i < MENU_BORDER_WIDTH - 2; i++)
                buffer.WriteToBuffer(i + 1, footerY + 4, controlsLine_2[i], ConsoleColor.Gray);
            for (int i = controlsLine_2.Length + 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY + 4, ' ', ConsoleColor.Gray);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY + 4, '║', ConsoleColor.Cyan);

            // Viền dưới khung hướng dẫn
            buffer.WriteToBuffer(0, footerY + 5, '╚', ConsoleColor.Cyan);
            for (int i = 1; i < MENU_BORDER_WIDTH - 1; i++)
                buffer.WriteToBuffer(i, footerY + 5, '═', ConsoleColor.Cyan);
            buffer.WriteToBuffer(MENU_BORDER_WIDTH - 1, footerY + 5, '╝', ConsoleColor.Cyan);
        }

        /// <summary>
        /// Hiển thị overlay GAME OVER vào buffer
        /// </summary>
        public void RenderGameOverOverlayToBuffer()
        {
            string gameOverText = " GAME OVER! ";
            int startX = GameState.GameWidth / 2 - gameOverText.Length / 2;
            int overlayY = PLAYER_SCREEN_HEIGHT;

            for (int i = 0; i < gameOverText.Length; i++)
            {
                buffer.WriteToBuffer(startX + i, overlayY, gameOverText[i], ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Hiển thị hiệu ứng countdown đặc biệt ở giữa mỗi màn hình player (giống đua xe)
        /// </summary>
        public void RenderCountdownOverlayToBuffer(int countdownValue)
        {
            // Tính vị trí trung tâm cho mỗi player
            int centerX = MENU_BORDER_WIDTH / 2;
            int centerY1 = PLAYER_SCREEN_HEIGHT / 2 + 1;
            int centerY2 = PLAYER_SCREEN_HEIGHT + (PLAYER_SCREEN_HEIGHT / 2) + 1;

            // Hiệu ứng số lớn
            string[] bigNumbers = new string[4];
            ConsoleColor color = ConsoleColor.Yellow;
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
                            buffer.WriteToBuffer(startX + col, startY + row, ch, color);
                        }
                    }
                }
            }
            DrawBigNumber(centerY1);
            DrawBigNumber(centerY2);
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
    }
}
