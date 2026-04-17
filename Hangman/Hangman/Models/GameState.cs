namespace Hangman.Models
{
    public class GameState
    {
        public string CurrentWord { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string GuessedLetters { get; set; } = string.Empty;
        public int WrongGuesses { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxWrongGuesses { get; set; } = 6;
        public int TimeRemaining { get; set; } = 30;
        public bool IsGameOver { get; set; }
        public bool IsWon { get; set; }
        public string Username { get; set; } = string.Empty;
        public Guid? SaveId { get; set; }

        public string GetMaskedWord()
        {
            var result = new System.Text.StringBuilder();
            foreach (var letter in CurrentWord.ToUpper())
            {
                if (letter == ' ')
                    result.Append("  ");
                else if (GuessedLetters.Contains(letter))
                    result.Append(letter + " ");
                else
                    result.Append("_ ");
            }
            return result.ToString().Trim();
        }

        public bool IsLetterGuessed(char letter)
        {
            return GuessedLetters.Contains(char.ToUpper(letter));
        }

        public bool GuessLetter(char letter)
        {
            letter = char.ToUpper(letter);
            if (GuessedLetters.Contains(letter))
                return false;

            GuessedLetters += letter;

            if (CurrentWord.ToUpper().Contains(letter))
            {
                CheckWinCondition();
                return true;
            }
            else
            {
                WrongGuesses++;
                CheckLossCondition();
                return false;
            }
        }

        private void CheckWinCondition()
        {
            foreach (var letter in CurrentWord.ToUpper())
            {
                if (letter != ' ' && !GuessedLetters.Contains(letter))
                    return;
            }
            IsWon = true;
            IsGameOver = true;
        }

        private void CheckLossCondition()
        {
            if (WrongGuesses >= MaxWrongGuesses || TimeRemaining <= 0)
            {
                IsGameOver = true;
            }
        }

        public void DecrementTimer()
        {
            if (TimeRemaining > 0 && !IsGameOver)
            {
                TimeRemaining--;
                if (TimeRemaining == 0)
                {
                    IsGameOver = true;
                }
            }
        }
    }
}
