namespace Hangman.Services
{
    public class GameTimerServiceFactory : IGameTimerServiceFactory
    {
        public IGameTimerService Create() => new GameTimerService();
    }
}
