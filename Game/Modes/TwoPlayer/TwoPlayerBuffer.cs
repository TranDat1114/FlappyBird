using System;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Quản lý double buffering cho TwoPlayerGameMode để tránh flicker
    /// </summary>
    public class TwoPlayerBuffer
    {
        // === CONSTANTS ===
        public const int MENU_BORDER_WIDTH = 66;
        public const int TOTAL_DISPLAY_HEIGHT = 36;

        // === BUFFERING ARRAYS ===
        private char[,] previousBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private char[,] currentBuffer = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] previousColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] currentColorBuffer = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        
        public bool BufferInitialized { get; private set; } = false;

        /// <summary>
        /// Initialize double buffering arrays to reduce flicker
        /// </summary>
        public void InitializeBuffers()
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

            BufferInitialized = true;
        }

        /// <summary>
        /// Clear current buffer for next frame
        /// </summary>
        public void ClearCurrentBuffer()
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
        public void WriteToBuffer(int x, int y, char ch, ConsoleColor color = ConsoleColor.White)
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
        public void FlushBufferToConsole()
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
    }
}
