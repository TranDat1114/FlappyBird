using System;
using System.Collections.Generic;
using System.Threading;

class FlappyBirdGame
{
    private static int gameWidth = 80;
    private static int gameHeight = 20;
    private static int birdX = 10;
    private static int birdY = gameHeight / 2;
    private static int score = 0;
    private static bool gameOver = false;
    private static Random random = new Random();
    
    private static List<Pipe> pipes = new List<Pipe>();
    private static int pipeSpacing = 20;
    private static int lastPipeX = gameWidth;
    
    private class Pipe
    {
        public int X { get; set; }
        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }
        public int GapSize { get; set; } = 6;
        
        public Pipe(int x)
        {
            X = x;
            TopHeight = random.Next(2, gameHeight - GapSize - 2);
            BottomHeight = gameHeight - TopHeight - GapSize;
        }
    }
    
    static void Main()
    {
        Console.CursorVisible = false;
        
        // Chỉ thiết lập kích thước cửa sổ trên Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(gameWidth, gameHeight + 5);
                Console.SetBufferSize(gameWidth, gameHeight + 5);
            }
            catch
            {
                // Bỏ qua nếu không thể thiết lập kích thước
            }
        }
        
        // Tạo ống đầu tiên
        pipes.Add(new Pipe(gameWidth - 1));
        
        Thread gameThread = new Thread(GameLoop);
        gameThread.Start();
        
        // Xử lý đầu vào
        while (!gameOver)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                birdY -= 3; // Chim bay lên
                if (birdY < 0) birdY = 0;
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                gameOver = true;
            }
        }
        
        gameThread.Join();
        Console.SetCursorPosition(0, gameHeight + 2);
        Console.WriteLine($"Trò chơi kết thúc! Điểm số: {score}");
        Console.WriteLine("Nhấn phím bất kỳ để thoát...");
        Console.ReadKey();
    }
    
    static void GameLoop()
    {
        while (!gameOver)
        {
            Update();
            Draw();
            Thread.Sleep(150); // Tốc độ game
        }
    }
    
    static void Update()
    {
        // Chim rơi xuống do trọng lực
        birdY++;
        if (birdY >= gameHeight - 1)
        {
            gameOver = true;
            return;
        }
        
        // Di chuyển ống
        for (int i = pipes.Count - 1; i >= 0; i--)
        {
            pipes[i].X--;
            
            // Xóa ống đã qua
            if (pipes[i].X < -2)
            {
                pipes.RemoveAt(i);
                score++;
            }
        }
        
        // Tạo ống mới
        if (pipes.Count == 0 || pipes[pipes.Count - 1].X < gameWidth - pipeSpacing)
        {
            pipes.Add(new Pipe(gameWidth - 1));
        }
        
        // Kiểm tra va chạm
        CheckCollision();
    }
    
    static void CheckCollision()
    {
        foreach (var pipe in pipes)
        {
            // Kiểm tra va chạm với ống
            if (birdX >= pipe.X - 1 && birdX <= pipe.X + 1)
            {
                if (birdY <= pipe.TopHeight || birdY >= gameHeight - pipe.BottomHeight)
                {
                    gameOver = true;
                    return;
                }
            }
        }
    }
    
    static void Draw()
    {
        Console.Clear();
        
        // Vẽ khung trò chơi
        char[,] screen = new char[gameHeight, gameWidth];
        
        // Khởi tạo nền trống
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                screen[y, x] = ' ';
            }
        }
        
        // Vẽ biên trên và dưới
        for (int x = 0; x < gameWidth; x++)
        {
            screen[0, x] = '#';
            screen[gameHeight - 1, x] = '#';
        }
        
        // Vẽ biên trái và phải
        for (int y = 0; y < gameHeight; y++)
        {
            screen[y, 0] = '#';
            screen[y, gameWidth - 1] = '#';
        }
        
        // Vẽ ống
        foreach (var pipe in pipes)
        {
            if (pipe.X >= 0 && pipe.X < gameWidth)
            {
                // Ống trên
                for (int y = 1; y <= pipe.TopHeight; y++)
                {
                    if (pipe.X >= 0 && pipe.X < gameWidth && y >= 0 && y < gameHeight)
                        screen[y, pipe.X] = '|';
                }
                
                // Ống dưới
                for (int y = gameHeight - pipe.BottomHeight - 1; y < gameHeight - 1; y++)
                {
                    if (pipe.X >= 0 && pipe.X < gameWidth && y >= 0 && y < gameHeight)
                        screen[y, pipe.X] = '|';
                }
            }
        }
        
        // Vẽ chim
        if (birdX >= 0 && birdX < gameWidth && birdY >= 0 && birdY < gameHeight)
        {
            screen[birdY, birdX] = '@';
        }
        
        // Hiển thị màn hình
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                Console.Write(screen[y, x]);
            }
            Console.WriteLine();
        }
        
        // Hiển thị điểm số
        Console.WriteLine($"Điểm số: {score}");
        Console.WriteLine("Nhấn SPACE để bay lên, ESC để thoát");
    }
}
