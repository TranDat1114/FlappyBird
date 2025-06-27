using FlappyBird.Enum;
using FlappyBird.Game.Modes;

namespace FlappyBird.Game
{
    /// <summary>
    /// Factory để tạo các chế độ chơi
    /// </summary>
    public static class GameModeFactory
    {
        public static IGameMode CreateGameMode(GameMode gameMode)
        {
            return gameMode switch
            {
                GameMode.SinglePlayer => new SinglePlayerGameMode(),
                GameMode.TwoPlayer => new TwoPlayerGameMode(),
                GameMode.DualAI => new DualAIGameMode(),
                GameMode.SplitScreenAI => new SplitScreenAIGameMode(),
                GameMode.AITournament => new AITournamentGameMode(),
                _ => new SinglePlayerGameMode()
            };
        }
        
        public static GameMode MenuActionToGameMode(MenuAction menuAction)
        {
            return menuAction switch
            {
                MenuAction.SinglePlayer => GameMode.SinglePlayer,
                MenuAction.TwoPlayer => GameMode.TwoPlayer,
                MenuAction.DualAI => GameMode.DualAI,
                MenuAction.SplitScreenAI => GameMode.SplitScreenAI,
                MenuAction.AITournament => GameMode.AITournament,
                _ => GameMode.SinglePlayer
            };
        }
    }
}
