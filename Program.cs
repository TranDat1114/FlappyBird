using System;
using System.Collections.Generic;
using System.Threading;

class FlappyBirdGame
{
    private static int gameWidth = 80;
    private static int gameHeight = 20;
    private static int birdX = 10;
    private static int birdY = 5; // Bắt đầu ở cao hơn (thay vì gameHeight / 2)
    private static int score = 0;
    private static bool gameOver = false;
    private static bool gameStarted = false; // Thêm trạng thái game chưa bắt đầu
    private static Random random = new Random();
    
    private static List<Pipe> pipes = new List<Pipe>();
    private static int pipeSpacing = 20;
    private static int lastPipeX = gameWidth;
    
    // Thêm biến cho vật lý mượt mà hơn
    private static float birdVelocity = 0f;
    private static float gravity = 0.15f; // Giảm gravity cho 60 FPS
    private static float jumpStrength = -1.2f; // Điều chỉnh lực nhảy cho 60 FPS
    private static int frameCounter = 0;
    
    // Thêm biến cho hệ thống tăng độ khó
    private static int difficultyLevel = 1;
    private static int pipeSpeed = 2; // Tốc độ ban đầu rất nhanh (frame delay)
    
    // Thêm biến cho double buffering để tránh nhấp nháy
    private static char[,] currentScreen = new char[gameHeight, gameWidth];
    private static char[,] previousScreen = new char[gameHeight, gameWidth];
    
    private class Pipe
    {
        public int X { get; set; }
        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }
        public int GapSize { get; set; }
        
        public Pipe(int x, int gapSize = 8)
        {
            X = x;
            GapSize = gapSize; // Cho phép thay đổi kích thước gap
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
        
        // Khởi tạo màn hình trống để tránh nhấp nháy ban đầu
        InitializeScreen();
        
        // Tạo ống đầu tiên
        pipes.Add(new Pipe(gameWidth - 1, 8));
        
        Thread gameThread = new Thread(GameLoop);
        gameThread.Start();
        
        // Xử lý đầu vào
        while (!gameOver)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                if (!gameStarted)
                {
                    gameStarted = true; // Bắt đầu game khi nhấn space đầu tiên
                }
                birdVelocity = jumpStrength; // Sử dụng velocity thay vì thay đổi trực tiếp position
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
            Thread.Sleep(16); // 60 FPS (1000ms / 60 ≈ 16ms)
        }
    }
    
    static void Update()
    {
        frameCounter++;
        
        // Chỉ cập nhật game khi đã bắt đầu
        if (!gameStarted)
        {
            return; // Chim đứng yên cho đến khi nhấn space
        }
        
        // Cập nhật độ khó dựa trên điểm số
        UpdateDifficulty();
        
        // Vật lý chim với gravity và velocity mượt mà hơn
        birdVelocity += gravity;
        birdY += (int)Math.Round(birdVelocity);
        
        // Giới hạn vị trí chim
        if (birdY < 1) 
        {
            birdY = 1;
            birdVelocity = 0;
        }
        if (birdY >= gameHeight - 1)
        {
            gameOver = true;
            return;
        }
        
        // Di chuyển ống với tốc độ động theo độ khó
        if (frameCounter % pipeSpeed == 0)
        {
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
            
            // Tạo ống mới với gap size phụ thuộc vào độ khó
            if (pipes.Count == 0 || pipes[pipes.Count - 1].X < gameWidth - pipeSpacing)
            {
                int currentGapSize = Math.Max(6, 9 - difficultyLevel); // Gap giảm theo level
                pipes.Add(new Pipe(gameWidth - 1, currentGapSize));
            }
        }
        
        // Kiểm tra va chạm
        CheckCollision();
    }
    
    static void UpdateDifficulty()
    {
        // Tăng độ khó mỗi 5 điểm
        int newDifficultyLevel = (score / 5) + 1;
        
        if (newDifficultyLevel != difficultyLevel)
        {
            difficultyLevel = newDifficultyLevel;
            
            // Giữ tốc độ tối đa (pipeSpeed = 2) từ đầu
            pipeSpeed = 2; // Luôn giữ tốc độ tối đa
            
            // Tăng gravity nhẹ theo level để tăng độ khó
            gravity = 0.15f + (difficultyLevel - 1) * 0.03f; // Tăng gravity nhiều hơn
        }
    }
    
    static void InitializeScreen()
    {
        // Khởi tạo màn hình trống
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                previousScreen[y, x] = ' ';
            }
        }
        
        // Xóa màn hình một lần duy nhất
        Console.Clear();
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
        // Vẽ vào buffer thay vì trực tiếp ra console
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
        
        // Chỉ cập nhật những ô thay đổi để tránh nhấp nháy
        RenderOptimized(screen);
        
        // Hiển thị thông tin ở dưới màn hình game
        Console.SetCursorPosition(0, gameHeight);
        if (!gameStarted)
        {
            Console.Write("Nhấn SPACE để bắt đầu trò chơi!".PadRight(gameWidth));
            Console.SetCursorPosition(0, gameHeight + 1);
            Console.Write("ESC để thoát".PadRight(gameWidth));
        }
        else
        {
            Console.Write($"Điểm số: {score} | Level: {difficultyLevel} | Tốc độ: MAX".PadRight(gameWidth));
            Console.SetCursorPosition(0, gameHeight + 1);
            Console.Write("Nhấn SPACE để bay lên, ESC để thoát".PadRight(gameWidth));
        }
    }
    
    static void RenderOptimized(char[,] newScreen)
    {
        // Chỉ cập nhật những ô pixel thay đổi
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                if (newScreen[y, x] != previousScreen[y, x])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(newScreen[y, x]);
                    previousScreen[y, x] = newScreen[y, x];
                }
            }
        }
    }
}
