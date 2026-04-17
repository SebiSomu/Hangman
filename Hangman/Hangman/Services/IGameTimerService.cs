namespace Hangman.Services
{
    public interface IGameTimerService
    {
        // --- Game countdown timer ---
        event EventHandler? GameTimerTick;
        void StartGameTimer();
        void StopGameTimer();

        // --- Level-transition timer ---
        void StartTransitionTimer(Action callback);
        void StopTransitionTimer();

        // --- Feedback display timer ---
        void StartFeedbackTimer(Action callback);
        void StopFeedbackTimer();
    }
}
