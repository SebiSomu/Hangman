using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Hangman.Controls
{
    public class HangmanDrawing : Canvas
    {
        public static readonly DependencyProperty WrongGuessesProperty =
            DependencyProperty.Register(
                nameof(WrongGuesses),
                typeof(int),
                typeof(HangmanDrawing),
                new PropertyMetadata(0, OnWrongGuessesChanged));

        public int WrongGuesses
        {
            get => (int)GetValue(WrongGuessesProperty);
            set => SetValue(WrongGuessesProperty, value);
        }

        private static void OnWrongGuessesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HangmanDrawing)d).DrawHangman();
        }

        public HangmanDrawing()
        {
            Width = 250;
            Height = 300;
            Background = Brushes.Transparent;
            DrawHangman();
        }

        private void DrawHangman()
        {
            Children.Clear();

            // Base 
            DrawLine(50, 280, 200, 280, 4); // Base line
            DrawLine(80, 280, 80, 50, 4);   // Vertical pole
            DrawLine(80, 50, 180, 50, 4);   // Horizontal top
            DrawLine(180, 50, 180, 80, 3);  // Rope

            // Head
            if (WrongGuesses >= 1)
                DrawEllipse(160, 80, 40, 40, 3);

            // Body
            if (WrongGuesses >= 2)
                DrawLine(180, 120, 180, 200, 3);

            // Left arm
            if (WrongGuesses >= 3)
                DrawLine(180, 140, 140, 170, 3);

            // Right arm
            if (WrongGuesses >= 4)
                DrawLine(180, 140, 220, 170, 3);

            // Left leg
            if (WrongGuesses >= 5)
                DrawLine(180, 200, 140, 250, 3);

            // Right leg
            if (WrongGuesses >= 6)
                DrawLine(180, 200, 220, 250, 3);
        }

        private void DrawLine(double x1, double y1, double x2, double y2, double thickness)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 70, 229)),
                StrokeThickness = thickness
            };
            Children.Add(line);
        }

        private void DrawEllipse(double x, double y, double width, double height, double thickness)
        {
            var ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(Color.FromRgb(79, 70, 229)),
                StrokeThickness = thickness,
                Fill = Brushes.Transparent
            };
            SetLeft(ellipse, x);
            SetTop(ellipse, y);
            Children.Add(ellipse);
        }
    }
}
