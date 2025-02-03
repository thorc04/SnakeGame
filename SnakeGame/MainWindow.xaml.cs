using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;
        private SolidColorBrush snakeBodyBrush = Brushes.Black;
        private SolidColorBrush snakeHeadBrush = Brushes.Black;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        private UIElement snakeFood = null;
        private Random rnd = new Random();
        private int snakeLength;
        private int currentScore = 0;
        private int topScore = 0; 
        private DispatcherTimer gameTickTimer = new DispatcherTimer();

      
        private MediaPlayer moveSound = new MediaPlayer();
        private MediaPlayer foodSound = new MediaPlayer();
        private MediaPlayer gameOverSound = new MediaPlayer();

        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;

        public MainWindow()
        {
            InitializeComponent();

            // Sounds 
            moveSound.Open(new Uri(@"C:\Users\thori\source\repos\SnakeGame\SnakeGame\SoundEffects\move.mp3"));
            foodSound.Open(new Uri(@"C:\Users\thori\source\repos\SnakeGame\SnakeGame\SoundEffects\food.mp3"));
            gameOverSound.Open(new Uri(@"C:\Users\thori\source\repos\SnakeGame\SnakeGame\SoundEffects\gameover.mp3"));

            gameTickTimer.Tick += GameTickTimer_Tick;
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            await Task.Delay(100);
            StartNewGame();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void StartNewGame()
        {
            bdrGameOver.Visibility = Visibility.Collapsed;

            ClearSnakeAndFood(); 

            snakeLength = SnakeStartLength;
            currentScore = 0;
            snakeDirection = SnakeDirection.Right;

         
            snakeParts.Add(new SnakePart
            {
                Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5)
            });

            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);
            DrawSnake();
            DrawSnakeFood();
            UpdateGameStatus();

            gameTickTimer.IsEnabled = true;
        }


        private void ClearSnakeAndFood()
        {
            foreach (var element in GameArea.Children.OfType<UIElement>().ToList())
            {
                if (element is Rectangle == false)  
                {
                    GameArea.Children.Remove(element);
                }
            }
        }

        private void DrawGameArea()
        {
            int rows = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int columns = (int)(GameArea.ActualWidth / SnakeSquareSize);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (x + y) % 2 == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ABD650")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3D048"))

                    };

                    GameArea.Children.Add(rect);
                    Canvas.SetTop(rect, y * SnakeSquareSize);
                    Canvas.SetLeft(rect, x * SnakeSquareSize);
                }
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush
                    };

                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            if (snakeParts.Count == 0)
                return;

            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;

            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            // Move sound
            moveSound.Stop();
            moveSound.Play();

            snakeParts.Add(new SnakePart
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }

            DrawSnake();
            DoCollisionCheck();
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();

            Image appleImage = new Image
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/apple.png"))
            };

            snakeFood = appleImage;

            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }




        //  Food Position
        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);

            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        private void BtnResetHighScore_Click(object sender, RoutedEventArgs e)
        {
            topScore = 0; 
            tbTopScore.Text = topScore.ToString(); 
        }

        private void BdrGameOver_PreviewKeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Space)
            {
               
                e.Handled = true;
            }
        }




        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            // Food collision
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) &&
                (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            // Wall collision "END GAME"
            if ((snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth) ||
                (snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight))
            {
                EndGame();
            }

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) &&
                    (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                {
                    EndGame();
                }
            }
        }

        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();

            // Food Sound
            foodSound.Stop();
            foodSound.Play();
        }

        private void UpdateGameStatus()
        {
            // Score
            this.tbStatusScore.Text = currentScore.ToString();
            if (currentScore > topScore)
            {
                topScore = currentScore;
                this.tbTopScore.Text = topScore.ToString();
            }
        }

        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;

            gameOverSound.Stop();
            gameOverSound.Play();

            bdrGameOver.Visibility = Visibility.Visible;
        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

            // No movment if dead
            if (e.Key == Key.Space)
            {
                StartNewGame();
                return; 
            }

            if (bdrGameOver.Visibility == Visibility.Visible)
                return;

            if (snakeParts.Count == 0)
                return;

            SnakeDirection originalSnakeDirection = snakeDirection;

            switch (e.Key)
            {
                // Arrows
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;

                // WASD
                case Key.W:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.S:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.A:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.D:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
            }

            if (snakeDirection != originalSnakeDirection)
            {
                MoveSnake();
            }
        }



        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
