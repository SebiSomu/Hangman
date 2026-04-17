namespace Hangman.Services
{
    public interface IGameTimerServiceFactory
    {
        IGameTimerService Create();
    }
}
