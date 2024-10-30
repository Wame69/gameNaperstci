using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NapierstkiGame
{
    public partial class MainWindow : Window
    {
        private int hiddenCupIndex; private Random random; private List<Image> cups; private bool isShuffling;
        private const double Cup1StartX = 100;
        private const double Cup2StartX = 200;
        private const double Cup3StartX = 300;
        private const double StartY = 150;

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            cups = new List<Image> { Cup1, Cup2, Cup3 };
            ResetGame();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            hiddenCupIndex = random.Next(0, cups.Count);
            Ball.Visibility = Visibility.Hidden;
            ResultText.Text = "Запоминайте!";

            AnimateAllCupsUp(() =>
            {
                PositionBallUnderCup(hiddenCupIndex);
                Ball.Visibility = Visibility.Visible;

                AnimateAllCupsDown(() =>
                {
                    Ball.Visibility = Visibility.Hidden;
                    StartShuffleAnimation();
                });
            });
        }

        private void PositionBallUnderCup(int cupIndex)
        {
            var cup = cups[cupIndex];
            double ballX = Canvas.GetLeft(cup) + (cup.Width - Ball.Width) / 2;

            double ballY = Canvas.GetTop(cup) + (cup.Height - Ball.Height) / 2 + 25;
            Canvas.SetLeft(Ball, ballX);
            Canvas.SetTop(Ball, ballY);
        }

        private void AnimateAllCupsUp(Action completedAction = null)
        {
            int completedCount = 0;
            foreach (var cup in cups)
            {
                var animation = new DoubleAnimation(-30, TimeSpan.FromSeconds(0.3));
                if (completedAction != null)
                {
                    animation.Completed += (s, e) =>
                    {
                        completedCount++;
                        if (completedCount == cups.Count) completedAction();
                    };
                }
                cup.RenderTransform = new TranslateTransform();
                cup.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void AnimateAllCupsDown(Action completedAction = null)
        {
            int completedCount = 0;
            foreach (var cup in cups)
            {
                var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
                if (completedAction != null)
                {
                    animation.Completed += (s, e) =>
                    {
                        completedCount++;
                        if (completedCount == cups.Count) completedAction();
                    };
                }
                cup.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void StartShuffleAnimation()
        {
            ResultText.Text = "Перемешиваем!";
            isShuffling = true;
            EnableCups(false);

            int shuffleSteps = 5;
            int shuffleIndex = 0;

            void ShuffleStep()
            {
                if (shuffleIndex >= shuffleSteps)
                {
                    isShuffling = false;
                    ResultText.Text = "Угадайте, где мяч!";
                    EnableCups(true);
                    return;
                }

                int firstIndex = random.Next(cups.Count);
                int secondIndex;
                do
                {
                    secondIndex = random.Next(cups.Count);
                } while (secondIndex == firstIndex);

                SwapCupsPosition(firstIndex, secondIndex, () =>
                {
                    shuffleIndex++;
                    ShuffleStep();
                });
            }

            ShuffleStep();
        }


        private void SwapCupsPosition(int firstCupIndex, int secondCupIndex, Action completedAction = null)
        {
            var firstCup = cups[firstCupIndex];
            var secondCup = cups[secondCupIndex];

            double firstCupX = Canvas.GetLeft(firstCup);
            double secondCupX = Canvas.GetLeft(secondCup);

            var firstCupAnimation = new DoubleAnimation(firstCupX, secondCupX, TimeSpan.FromSeconds(0.3));

            var secondCupAnimation = new DoubleAnimation(secondCupX, firstCupX, TimeSpan.FromSeconds(0.3));

            if (completedAction != null)
            {
                secondCupAnimation.Completed += (s, e) => completedAction();
            }

            firstCup.BeginAnimation(Canvas.LeftProperty, firstCupAnimation);
            secondCup.BeginAnimation(Canvas.LeftProperty, secondCupAnimation);
        }

        private void AnimateCupMoveToRandomPosition(int cupIndex, Action completedAction = null)
        {
            var cup = cups[cupIndex];

            double targetX = random.Next(0, (int)GameCanvas.ActualWidth - (int)cup.Width);
            double targetY = random.Next(0, (int)GameCanvas.ActualHeight - (int)cup.Height);

            var leftAnimation = new DoubleAnimation(Canvas.GetLeft(cup), targetX, TimeSpan.FromSeconds(0.5));
            var topAnimation = new DoubleAnimation(Canvas.GetTop(cup), targetY, TimeSpan.FromSeconds(0.5));

            if (completedAction != null)
            {
                leftAnimation.Completed += (s, e) => completedAction();
            }

            cup.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            cup.BeginAnimation(Canvas.TopProperty, topAnimation);
        }

        private void ReturnCupsToStartPositions()
        {
            AnimateCupToPosition(Cup1, Cup1StartX, StartY);
            AnimateCupToPosition(Cup2, Cup2StartX, StartY);
            AnimateCupToPosition(Cup3, Cup3StartX, StartY);
        }

        private void AnimateCupToPosition(Image cup, double targetX, double targetY)
        {
            var leftAnimation = new DoubleAnimation(Canvas.GetLeft(cup), targetX, TimeSpan.FromSeconds(0.5));
            var topAnimation = new DoubleAnimation(Canvas.GetTop(cup), targetY, TimeSpan.FromSeconds(0.5));

            cup.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            cup.BeginAnimation(Canvas.TopProperty, topAnimation);
        }

        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (isShuffling) return;

            if (sender is Image clickedCup)
            {
                int selectedCupIndex = cups.IndexOf(clickedCup);
                EnableCups(false);

                AnimateCupUp(selectedCupIndex, () =>
                {
                    if (selectedCupIndex == hiddenCupIndex)
                    {
                        PositionBallUnderCup(hiddenCupIndex);
                        Ball.Visibility = Visibility.Visible;
                        ResultText.Text = "Поздравляем! Вы угадали!";
                    }
                    else
                    {
                        AnimateCupUp(hiddenCupIndex, () =>
                        {
                            PositionBallUnderCup(hiddenCupIndex);
                            Ball.Visibility = Visibility.Visible;
                        });
                        ResultText.Text = "Не угадали! Попробуйте снова.";
                    }

                    AnimateCupDown(selectedCupIndex);
                });
            }
        }

        private void AnimateCupUp(int cupIndex, Action completedAction = null)
        {
            var animation = new DoubleAnimation(-30, TimeSpan.FromSeconds(0.3));
            if (completedAction != null)
                animation.Completed += (s, e) => completedAction();

            cups[cupIndex].RenderTransform = new TranslateTransform();
            cups[cupIndex].RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void AnimateCupDown(int cupIndex, Action completedAction = null)
        {
            var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            if (completedAction != null)
                animation.Completed += (s, e) => completedAction();

            cups[cupIndex].RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void EnableCups(bool isEnabled)
        {
            foreach (var cup in cups)
            {
                cup.IsEnabled = isEnabled;
            }
        }

        private void ResetGame()
        {
            hiddenCupIndex = -1;
            Ball.Visibility = Visibility.Hidden;
            ResultText.Text = "Нажмите 'Начать игру', чтобы спрятать сюрприз!";
            EnableCups(false);

            Canvas.SetLeft(Cup1, Cup1StartX);
            Canvas.SetTop(Cup1, StartY);
            Canvas.SetLeft(Cup2, Cup2StartX);
            Canvas.SetTop(Cup2, StartY);
            Canvas.SetLeft(Cup3, Cup3StartX);
            Canvas.SetTop(Cup3, StartY);
        }
    }
}
