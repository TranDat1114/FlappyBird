using System;
using System.Collections.Generic;
using System.Threading;

class FlappyBirdGame
{
    // === TỔNG HỢP CÁC THÔNG SỐ TRẢI NGHIỆM NGƯỜI CHƠI - ĐƯỢC TÍNH TOÁN ĐỒNG BỘ ===
    private static int gameWidth = 80;
    private static int gameHeight = 20;
    private static int birdX = 10;
    private static int birdY = 8; // Vị trí khởi đầu ở giữa màn hình (20/2 - 2)
    private static int score = 0;
    private static bool gameOver = false;
    private static bool gameStarted = false;
    private static Random random = new Random();
    
    // === VẬT LÝ CHIM - CÂN BẰNG CHO KHUNG NHỎ 20x80 ===
    private static float birdVelocity = 0f;
    private static float gravity = 0.06f; // Cân bằng: đủ nhanh để cảm thấy tự nhiên, đủ chậm để kiểm soát
    private static float jumpStrength = -0.8f; // Giảm xuống: vừa đủ để vượt gap mà không bay quá cao
    private static float maxFallSpeed = 1.0f; // Cân bằng: nhanh nhưng vẫn kiểm soát được
    private static int frameCounter = 0;
    
    // === PIPE VÀ DIFFICULTY - PROGRESSION HỢP LÝ ===
    private static List<Pipe> pipes = new List<Pipe>();
    private static int pipeSpacing = 25; // Tối ưu cho rhythm: không quá xa, không quá gần
    private static int lastPipeX = gameWidth;
    private static int difficultyLevel = 1; // Bắt đầu từ level 1 thay vì 4
    private static int pipeSpeed = 4; // Chậm hơn ban đầu để học
    
    // === GAP SIZE ĐỘNG - TĂNG DẦN THEO SKILL ===
    private static int baseGapSize = 9; // Gap lớn ban đầu cho người mới
    private static int minGapSize = 6; // Gap tối thiểu vẫn chơi được
    
    // === ANIMATION & EFFECTS ===
    private static int birdAnimationFrame = 0;
    
    // Tối ưu cho addictive gameplay
    private static float difficultyGrowthRate = 0.008f; // Tăng độ khó từ từ để tạo addiction        // Thêm biến cho hệ thống tăng độ khó tối ưu
    
    // Thêm biến cho double buffering để tránh nhấp nháy
    private static char[,] currentScreen = new char[gameHeight, gameWidth];
    private static char[,] previousScreen = new char[gameHeight, gameWidth];
    
    // Tối ưu hiệu suất rendering
    private static bool forceFullRedraw = false;
    private static int lastScore = 0;
    private static int lastDifficultyLevel = 0;
    private static bool lastGameStarted = false;
    
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
    
    private class Pipe
    {
        public int X { get; set; }
        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }
        public int GapSize { get; set; }
        
        public Pipe(int x, int gapSize = 9) // Sử dụng baseGapSize mặc định
        {
            X = x;
            GapSize = gapSize;
            
            // Tối ưu vị trí ống cho khung 20 height - tạo trải nghiệm cân bằng
            // Đảm bảo gap luôn ở vùng có thể chơi được (tránh quá gần viền)
            int minTopHeight = Math.Max(3, gameHeight / 5); // Ít nhất 3 pixel từ viền trên
            int maxTopHeight = Math.Min(gameHeight - GapSize - 4, (gameHeight * 3) / 4); // Ít nhất 4 pixel từ viền dưới
            
            TopHeight = random.Next(minTopHeight, maxTopHeight);
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
            forceFullRedraw = true; // Buộc vẽ lại toàn bộ màn hình
            
            // Khởi tạo màn hình trống để tránh nhấp nháy ban đầu
            InitializeScreen();
            
            // Tạo ống đầu tiên với gap size lớn nhất để khuyến khích người chơi mới
            pipes.Clear();
            pipes.Add(new Pipe(gameWidth - 1, baseGapSize)); // Gap 9 cho level 1
            
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long lastFrameTime = 0;
        const long targetFrameTime = 16; // 60 FPS = ~16.67ms per frame
        
        while (!gameOver)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            
            // Chỉ update nếu đủ thời gian đã trôi qua
            if (currentTime - lastFrameTime >= targetFrameTime)
            {
                Update();
                Draw();
                lastFrameTime = currentTime;
            }
            
            // Sleep ngắn để không làm CPU quá tải
            Thread.Sleep(1);
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
        
        // Giới hạn tốc độ rơi tối đa để tránh rơi quá nhanh
        if (birdVelocity > maxFallSpeed)
        {
            birdVelocity = maxFallSpeed;
        }
        
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
            
            // Tạo ống mới với gap size động và spacing được tối ưu
            if (pipes.Count == 0 || pipes[pipes.Count - 1].X < gameWidth - pipeSpacing)
            {
                // === HỆ THỐNG GAP SIZE ĐỘNG - GIẢM DẦN THEO SKILL ===
                // Level 1-3: Gap 9 (học cách chơi)
                // Level 4-6: Gap 8 (trung bình) 
                // Level 7-9: Gap 7 (khó)
                // Level 10+: Gap 6 (chuyên nghiệp)
                int currentGapSize = baseGapSize - (difficultyLevel / 3);
                currentGapSize = Math.Max(minGapSize, currentGapSize);
                
                pipes.Add(new Pipe(gameWidth - 1, currentGapSize));
            }
        }
        
        // Kiểm tra va chạm
        CheckCollision();
    }
    
    static void UpdateDifficulty()
    {
        // === HỆ THỐNG TĂNG ĐỘ KHÓ ĐƯỢC THIẾT KẾ CHO TRẢI NGHIỆM TỐI ƯU ===
        // Tăng level mỗi 5 điểm để có thời gian thích nghi
        int newDifficultyLevel = (score / 5) + 1;
        
        if (newDifficultyLevel != difficultyLevel)
        {
            difficultyLevel = newDifficultyLevel;
            
            // === PIPE SPEED - TĂNG DẦN NHƯNG KHÔNG QUÁ NHANH ===
            // Level 1-2: 4 frame/move (chậm - học cách chơi)
            // Level 3-4: 3 frame/move (trung bình)
            // Level 5-6: 2 frame/move (nhanh)
            // Level 7+: 1 frame/move (tối đa)
            if (difficultyLevel <= 2) pipeSpeed = 4;
            else if (difficultyLevel <= 4) pipeSpeed = 3;
            else if (difficultyLevel <= 6) pipeSpeed = 2;
            else pipeSpeed = 1;
            
            // GHI CHÚ: Vật lý chim (gravity, jumpStrength, maxFallSpeed) KHÔNG thay đổi
            // Chỉ pipe speed và gap size thay đổi để tạo trải nghiệm cân bằng
        }
    }
    
    static void ResetGame()
    {
        // === RESET TẤT CẢ THÔNG SỐ VỀ TRẠNG THÁI TỐI ƯU ===
        birdY = 8; // Vị trí giữa màn hình
        birdVelocity = 0f;
        score = 0;
        gameOver = false;
        gameStarted = false;
        frameCounter = 0;
        
        // === CỐ ĐỊNH VẬT LÝ CHIM - KHÔNG THAY ĐỔI THEO LEVEL ===
        gravity = 0.06f;
        jumpStrength = -0.8f; // Vừa đủ để vượt gap 9 mà không bay quá cao cho khung 20
        maxFallSpeed = 1.0f;
        
        // === RESET DIFFICULTY VỀ LEVEL 1 ===
        difficultyLevel = 1;
        pipeSpeed = 4; // Chậm nhất để bắt đầu
        birdAnimationFrame = 0;
        
        // Reset tracking variables cho rendering tối ưu
        lastScore = 0;
        lastDifficultyLevel = 0;
        lastGameStarted = false;
        forceFullRedraw = true;
        
        // === TẠO PIPE ĐẦU TIÊN VỚI GAP LỚN NHẤT ===
        pipes.Clear();
        pipes.Add(new Pipe(gameWidth - 1, baseGapSize)); // Gap 9 cho người mới
        
        // Reset màn hình buffer
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                previousScreen[y, x] = ' ';
            }
        }
        
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
            // Collision detection với một chút "forgiveness" để trải nghiệm tốt hơn
            // Giảm hitbox một chút để người chơi cảm thấy "may mắn" khi vượt qua
            if (birdX >= pipe.X - 1 && birdX <= pipe.X + 1) // Giảm từ 2 xuống 1
            {
                if (birdY <= pipe.TopHeight || birdY >= gameHeight - pipe.BottomHeight - 1)
                {
                    gameOver = true; // Bật lại collision detection
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
        
        // Vẽ chim với animation responsive hơn
        if (birdX >= 1 && birdX < gameWidth - 1 && birdY >= 1 && birdY < gameHeight - 1)
        {
            // === ANIMATION CHIM RESPONSIVE VỚI VẬT LÝ MỚI ===
            char currentBirdChar;
            if (birdVelocity < -0.12f) // Điều chỉnh để phù hợp với jumpStrength -0.8f
            {
                currentBirdChar = '^';  // Bay lên
            }
            else if (birdVelocity > 0.12f) // Giữ nguyên để phù hợp với gravity 0.06f
            {
                currentBirdChar = 'v';  // Rơi xuống
            }
            else
            {
                currentBirdChar = birdChar;  // Bình thường
            }
            
            screen[birdY, birdX] = currentBirdChar;
            
            // Thêm hiệu ứng cánh chim nhanh hơn
            birdAnimationFrame = (birdAnimationFrame + 1) % 4; // Giảm từ 6 xuống 4 để nhanh hơn
            if (birdAnimationFrame < 2) // Giảm từ 3 xuống 2
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
        
        // Chỉ cập nhật UI text khi có thay đổi để giảm flickering
        bool uiChanged = (score != lastScore) || (difficultyLevel != lastDifficultyLevel) || 
                        (gameStarted != lastGameStarted) || forceFullRedraw;
                        
        if (uiChanged)
        {
            RenderUI();
            lastScore = score;
            lastDifficultyLevel = difficultyLevel;
            lastGameStarted = gameStarted;
            forceFullRedraw = false;
        }
    }
    
    static void RenderUI()
    {
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
            string statusMsg = $"Điểm: {score} | Level: {difficultyLevel}";
            Console.Write(statusMsg.PadRight(gameWidth));
            Console.SetCursorPosition(0, gameHeight + 1);
            string controlMsg = "SPACE: Bay lên | ESC: Thoát";
            Console.Write(controlMsg.PadRight(gameWidth));
        }
    }
    
    static void RenderOptimized(char[,] newScreen)
    {
        // Batch các thay đổi để giảm số lần gọi SetCursorPosition
        var changes = new List<(int x, int y, char ch)>();
        
        // Thu thập tất cả các thay đổi
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                if (newScreen[y, x] != previousScreen[y, x])
                {
                    changes.Add((x, y, newScreen[y, x]));
                    previousScreen[y, x] = newScreen[y, x];
                }
            }
        }
        
        // Áp dụng các thay đổi theo batch để tối ưu performance
        if (changes.Count > 0)
        {
            // Sắp xếp theo y rồi x để tối ưu cursor movement
            changes.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            
            int currentX = -1, currentY = -1;
            foreach (var (x, y, ch) in changes)
            {
                // Chỉ di chuyển cursor khi cần thiết
                if (x != currentX + 1 || y != currentY)
                {
                    Console.SetCursorPosition(x, y);
                }
                Console.Write(ch);
                currentX = x;
                currentY = y;
            }
        }
    }
}
