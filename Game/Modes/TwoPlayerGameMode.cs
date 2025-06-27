using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ chơi 2 người
    /// </summary>
    public class TwoPlayerGameMode : GameModeBase
    {
        private GameState player1State = new GameState();
        private GameState player2State = new GameState();
        private bool gameStarted = false;
        
        public override void Initialize()
        {
            player1State.Reset();
            player2State.Reset();
            
            // Initialize pipes for both players
            var pipe1 = new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random);
            var pipe2 = new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random);
            
            player1State.Pipes.Add(pipe1);
            player2State.Pipes.Add(pipe2);
            
            gameStarted = false;
        }
        
        public override void Update()
        {
            if (!gameStarted) return;
            
            // Update both players
            UpdatePlayer(player1State);
            UpdatePlayer(player2State);
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
            Console.Clear();
            
            // Render split screen
            RenderSplitScreen();
            
            // Render UI
            RenderTwoPlayerUI();
        }
        
        private void RenderSplitScreen()
        {
            // Top border
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', 38) + "╦" + new string('═', 38) + "╗");
            Console.WriteLine("║" + new string(' ', 16) + "PLAYER 1" + new string(' ', 14) + "║" + new string(' ', 16) + "PLAYER 2" + new string(' ', 14) + "║");
            Console.WriteLine("╠" + new string('═', 38) + "╬" + new string('═', 38) + "╣");
            
            // Game areas
            for (int y = 0; y < GameState.GameHeight - 3; y++)
            {
                Console.Write("║");
                
                // Player 1 area
                for (int x = 0; x < 38; x++)
                {
                    char ch = GetCharAt(player1State, x, y);
                    SetCharColor(ch);
                    Console.Write(ch);
                }
                
                Console.ResetColor();
                Console.Write("║");
                
                // Player 2 area  
                for (int x = 0; x < 38; x++)
                {
                    char ch = GetCharAt(player2State, x, y);
                    SetCharColor(ch);
                    Console.Write(ch);
                }
                
                Console.ResetColor();
                Console.WriteLine("║");
            }
            
            // Bottom border
            Console.WriteLine("╚" + new string('═', 38) + "╩" + new string('═', 38) + "╝");
        }
        
        private char GetCharAt(GameState state, int x, int y)
        {
            // Scale coordinates
            int scaledX = (int)((float)x / 38 * GameState.GameWidth);
            int scaledY = (int)((float)y / (GameState.GameHeight - 3) * GameState.GameHeight);
            
            // Check bird position
            if (scaledX == GameState.BirdX && scaledY == state.BirdY)
            {
                return state.BirdVelocity < 0 ? '^' : state.BirdVelocity > 0 ? 'v' : '♦';
            }
            
            // Check pipes
            foreach (var pipe in state.Pipes)
            {
                if (scaledX >= pipe.X - 1 && scaledX <= pipe.X + 1)
                {
                    if (scaledY <= pipe.TopHeight || scaledY >= GameState.GameHeight - pipe.BottomHeight - 1)
                    {
                        return '█';
                    }
                }
            }
            
            // Border
            if (scaledY == 0 || scaledY == GameState.GameHeight - 1)
                return '═';
            if (scaledX == 0 || scaledX == GameState.GameWidth - 1)
                return '║';
            
            return ' ';
        }
        
        private void SetCharColor(char ch)
        {
            switch (ch)
            {
                case '♦':
                case '^':
                case 'v':
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case '█':
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case '═':
                case '║':
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }
        }
        
        private void RenderTwoPlayerUI()
        {
            Console.SetCursorPosition(0, GameState.GameHeight + 1);
            
            if (!gameStarted)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("*** CHE DO HAI NGUOI CHOI ***");
                Console.WriteLine("Player 1: W để bay | Player 2: ↑ để bay");
                Console.WriteLine("Nhấn SPACE để bắt đầu | ESC: Menu");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Player 1: {player1State.Score} điểm {(player1State.GameOver ? "(THUA)" : "")}");
                Console.WriteLine($"Player 2: {player2State.Score} điểm {(player2State.GameOver ? "(THUA)" : "")}");
                
                if (IsGameOver())
                {
                    var winner = GetWinner();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"*** {winner} THANG! *** | R: Choi lai | ESC: Menu");
                }
            }
            
            Console.ResetColor();
        }
        
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted)
                    {
                        gameStarted = true;
                        player1State.GameStarted = true;
                        player2State.GameStarted = true;
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
                    
                case ConsoleKey.R:
                    if (IsGameOver())
                    {
                        Initialize();
                    }
                    break;
                    
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }
        
        public override bool IsGameOver()
        {
            return (player1State.GameOver && player2State.GameOver) || shouldExit;
        }
        
        private string GetWinner()
        {
            if (player1State.GameOver && player2State.GameOver)
            {
                if (player1State.Score > player2State.Score)
                    return "PLAYER 1";
                else if (player2State.Score > player1State.Score)
                    return "PLAYER 2";
                else
                    return "HÒA";
            }
            else if (player1State.GameOver)
                return "PLAYER 2";
            else if (player2State.GameOver)
                return "PLAYER 1";
            
            return "ĐANG CHƠI";
        }
    }
}
