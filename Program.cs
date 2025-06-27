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
    
    // ASCII Art Characters for better UI design
    private static char birdChar = '♦';           // Diamond bird character
    private static char pipeChar = '█';          // Solid block for pipes
    private static char borderHorizontal = '═';  // Double horizontal line
    private static char borderVertical = '║';    // Double vertical line
    private static char borderCornerTL = '╔';    // Top-left corner
    private static char borderCornerTR = '╗';    // Top-right corner
    private static char borderCornerBL = '╚';    // Bottom-left corner
    private static char borderCornerBR = '╝';    // Bottom-right corner
    private static char backgroundChar = '·';    // Light dot for background pattern
    private static int birdAnimationFrame = 0;   // For bird animation
    
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
        
        // Vòng lặp chính để cho phép restart game
        bool exitProgram = false;
        while (!exitProgram)
        {
            // Reset game state trước khi bắt đầu
            gameOver = false;
            gameStarted = false;
            
            // Khởi tạo màn hình trống để tránh nhấp nháy ban đầu
            InitializeScreen();
            
            // Tạo ống đầu tiên
            pipes.Clear();
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
                    exitProgram = true; // Thoát hoàn toàn
                }
                // Loại bỏ xử lý phím R trong lúc chơi - chỉ cho phép khi game over
            }
            
            gameThread.Join();
            
            // Nếu game over và không phải do ESC, cho phép restart
            if (!exitProgram)
            {
                Console.SetCursorPosition(0, gameHeight + 2);
                Console.WriteLine($"GAME OVER! Điểm số cuối cùng: {score}");
                Console.WriteLine("R: Chơi lại | ESC: Thoát");
                
                // Đợi input sau game over
                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.R)
                    {
                        ResetGame();
                        break; // Restart game
                    }
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        exitProgram = true;
                        break; // Thoát
                    }
                }
            }
        }
        
        Console.SetCursorPosition(0, gameHeight + 3);
        Console.WriteLine("Cảm ơn bạn đã chơi!");
    }
    
    static void GameLoop()
    {
        while (!gameOver)
        {
            Update();
            Draw();
            Thread.Sleep(16); // 60 FPS (1000ms / 60 ≈ 16ms)
        }
        
        // Vẽ frame cuối cùng khi game over
        Draw();
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
    
    static void ResetGame()
    {
        // Reset tất cả biến về trạng thái ban đầu
        birdY = 5;
        score = 0;
        gameOver = false; // Quan trọng: reset gameOver
        gameStarted = false;
        birdVelocity = 0f;
        gravity = 0.15f;
        frameCounter = 0;
        difficultyLevel = 1;
        pipeSpeed = 2;
        birdAnimationFrame = 0;
        
        // Xóa tất cả ống
        pipes.Clear();
        
        // Tạo ống đầu tiên
        pipes.Add(new Pipe(gameWidth - 1, 8));
        
        // Reset màn hình buffer để tránh hiển thị lỗi
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                previousScreen[y, x] = ' ';
            }
        }
        
        // Xóa màn hình
        Console.Clear();
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
            // Kiểm tra va chạm với ống (ống giờ rộng hơn)
            if (birdX >= pipe.X - 2 && birdX <= pipe.X + 2)
            {
                if (birdY <= pipe.TopHeight || birdY >= gameHeight - pipe.BottomHeight - 1)
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
        
        // Khởi tạo nền với pattern nhẹ
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                // Tạo pattern nền với dots nhẹ
                if ((x + y) % 8 == 0 && y > 0 && y < gameHeight - 1 && x > 0 && x < gameWidth - 1)
                    screen[y, x] = backgroundChar;
                else
                    screen[y, x] = ' ';
            }
        }
        
        // Vẽ viền game với ASCII box drawing characters
        // Viền trên
        for (int x = 1; x < gameWidth - 1; x++)
        {
            screen[0, x] = borderHorizontal;
        }
        // Viền dưới
        for (int x = 1; x < gameWidth - 1; x++)
        {
            screen[gameHeight - 1, x] = borderHorizontal;
        }
        // Viền trái và phải
        for (int y = 1; y < gameHeight - 1; y++)
        {
            screen[y, 0] = borderVertical;
            screen[y, gameWidth - 1] = borderVertical;
        }
        // Góc viền
        screen[0, 0] = borderCornerTL;
        screen[0, gameWidth - 1] = borderCornerTR;
        screen[gameHeight - 1, 0] = borderCornerBL;
        screen[gameHeight - 1, gameWidth - 1] = borderCornerBR;
        
        // Vẽ ống với thiết kế đẹp hơn
        foreach (var pipe in pipes)
        {
            if (pipe.X >= 1 && pipe.X < gameWidth - 1)
            {
                // Ống trên - vẽ với độ dày 3 pixel
                for (int y = 1; y <= pipe.TopHeight; y++)
                {
                    if (y >= 1 && y < gameHeight - 1)
                    {
                        // Vẽ ống chính
                        screen[y, pipe.X] = pipeChar;
                        
                        // Vẽ viền ống nếu có chỗ
                        if (pipe.X - 1 >= 1)
                            screen[y, pipe.X - 1] = pipeChar;
                        if (pipe.X + 1 < gameWidth - 1)
                            screen[y, pipe.X + 1] = pipeChar;
                    }
                }
                
                // Vẽ mũ ống trên (cap)
                if (pipe.TopHeight + 1 < gameHeight - 1 && pipe.TopHeight >= 1)
                {
                    for (int capX = pipe.X - 2; capX <= pipe.X + 2; capX++)
                    {
                        if (capX >= 1 && capX < gameWidth - 1)
                            screen[pipe.TopHeight, capX] = '▀';
                    }
                }
                
                // Ống dưới - vẽ với độ dày 3 pixel
                for (int y = gameHeight - pipe.BottomHeight - 1; y < gameHeight - 1; y++)
                {
                    if (y >= 1 && y < gameHeight - 1)
                    {
                        // Vẽ ống chính
                        screen[y, pipe.X] = pipeChar;
                        
                        // Vẽ viền ống nếu có chỗ
                        if (pipe.X - 1 >= 1)
                            screen[y, pipe.X - 1] = pipeChar;
                        if (pipe.X + 1 < gameWidth - 1)
                            screen[y, pipe.X + 1] = pipeChar;
                    }
                }
                
                // Vẽ mũ ống dưới (cap)
                int bottomCapY = gameHeight - pipe.BottomHeight - 1;
                if (bottomCapY > 1 && bottomCapY < gameHeight - 1)
                {
                    for (int capX = pipe.X - 2; capX <= pipe.X + 2; capX++)
                    {
                        if (capX >= 1 && capX < gameWidth - 1)
                            screen[bottomCapY, capX] = '▄';
                    }
                }
            }
        }
        
        // Vẽ chim với animation
        if (birdX >= 1 && birdX < gameWidth - 1 && birdY >= 1 && birdY < gameHeight - 1)
        {
            // Animation cho chim dựa trên velocity
            char currentBirdChar;
            if (birdVelocity < -0.5f)
            {
                currentBirdChar = '^';  // Bay lên
            }
            else if (birdVelocity > 0.5f)
            {
                currentBirdChar = 'v';  // Rơi xuống
            }
            else
            {
                currentBirdChar = birdChar;  // Bình thường
            }
            
            screen[birdY, birdX] = currentBirdChar;
            
            // Thêm hiệu ứng cánh chim
            birdAnimationFrame = (birdAnimationFrame + 1) % 6;
            if (birdAnimationFrame < 3)
            {
                // Cánh lên
                if (birdX - 1 >= 1) screen[birdY, birdX - 1] = '~';
            }
            else
            {
                // Cánh xuống
                if (birdX - 1 >= 1) screen[birdY, birdX - 1] = '_';
            }
        }
        
        // Chỉ cập nhật những ô thay đổi để tránh nhấp nháy
        RenderOptimized(screen);
        
        // Hiển thị thông tin ở dưới màn hình game với thiết kế đẹp hơn
        Console.SetCursorPosition(0, gameHeight);
        if (!gameStarted)
        {
            string startMsg = "Nhấn SPACE để bắt đầu trò chơi!";
            Console.Write(startMsg.PadRight(gameWidth));
            Console.SetCursorPosition(0, gameHeight + 1);
            string exitMsg = "ESC: Thoát";
            Console.Write(exitMsg.PadRight(gameWidth));
        }
        else
        {
            string statusMsg = $"Điểm: {score} | Level: {difficultyLevel} | Tốc độ: MAX";
            Console.Write(statusMsg.PadRight(gameWidth));
            Console.SetCursorPosition(0, gameHeight + 1);
            string controlMsg = "SPACE: Bay lên | ESC: Thoát";
            Console.Write(controlMsg.PadRight(gameWidth));
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
