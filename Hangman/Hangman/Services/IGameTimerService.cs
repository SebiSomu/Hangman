namespace Hangman.Services
{
    public interface IGameTimerService
    {
        event EventHandler? GameTimerTick;
        void StartGameTimer();
        void StopGameTimer();

        void StartTransitionTimer(Action callback);
        void StopTransitionTimer();

        void StartFeedbackTimer(Action callback);
        void StopFeedbackTimer();
    }
}
