using System.Windows.Threading;

namespace Hangman.Services
{
    public class GameTimerService : IGameTimerService
    {
        private DispatcherTimer? _gameTimer;
        private DispatcherTimer? _transitionTimer;
        private DispatcherTimer? _feedbackTimer;

        public event EventHandler? GameTimerTick;

        public void StartGameTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _gameTimer.Tick += (s, e) => GameTimerTick?.Invoke(this, EventArgs.Empty);
            _gameTimer.Start();
        }

        public void StopGameTimer()
        {
            _gameTimer?.Stop();
        }

        public void StartTransitionTimer(Action callback)
        {
            _feedbackTimer?.Stop();     
            _transitionTimer?.Stop();

            _transitionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _transitionTimer.Tick += (s, e) =>
            {
                _transitionTimer.Stop();
                callback();
            };
            _transitionTimer.Start();
        }

        public void StopTransitionTimer() => _transitionTimer?.Stop();

        public void StartFeedbackTimer(Action callback)
        {
            _feedbackTimer?.Stop();
            _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2000) };
            _feedbackTimer.Tick += (s, e) =>
            {
                _feedbackTimer.Stop();
                callback();
            };
            _feedbackTimer.Start();
        }

        public void StopFeedbackTimer() => _feedbackTimer?.Stop();
    }
}
