﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TetrisClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Representation _representation;
        private Tetromino _tetromino;
        private Tetromino _nextTetromino;
        private DispatcherTimer _dpt;
        private TimeSpan _tickInterval = new(0, 0, 0, 0, 700);
        private bool _paused; // Default value is false

        /// <summary>
        /// Initializes the component and the timer, then creates the first next Tetromino
        /// so it can be used in the NewTetromino method.
        /// After that it renders the new next tetromino and renders the board with the first tetromino.
        /// </summary>
        public MainWindow()
        {
            _representation = new Representation();
            
            InitializeComponent();
            Timer();
            
            _nextTetromino = new Tetromino(3, 0);
            NewTetromino();

            Board();
        }
        
        /// <summary>
        /// Clears the board otherwise for each movement a new tetromino will be displayed on top of
        /// the already existing one. Then Renders the tetromino.
        /// </summary>
        private void Board()
        {
            TetrisGrid.Children.Clear();
            RenderTetromino(_tetromino, TetrisGrid);
            RenderBoard();
        }

        /// <summary>
        /// Sets the next tetromino as the current tetromino and than creates a new next tetromino and does
        /// the same with the matrices.
        /// Also sets the start position of the current tetromino. 
        /// </summary>
        private void NewTetromino()
        {
            NextGrid.Children.Clear();
            _tetromino = _nextTetromino;
            _nextTetromino = new Tetromino(4, 0);
            RenderTetromino(_nextTetromino, NextGrid);
        }
        
        /// <summary>
        /// Start a DispatcherTimer because those don't interupt the program
        /// This timer is used for determining the drop speed of tetrominoes.
        /// </summary>
        private void Timer()
        {
            _dpt = new DispatcherTimer();
            _dpt.Tick += dispatcherTimer_Tick;
            _dpt.Interval = _tickInterval;
            _dpt.Start();
        }
        
        /// <summary>
        /// Starts a dispatcherTimer because those are non blocking.
        /// This timer is used to determine the speed at which tetromino's
        /// are falling 
        /// </summary>
        /// <param name="sender"></param> 
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DropTetromino();
            Board();
            DevelopmentInfo(); //TODO remove before release
        }

        private void DropTetromino()
        {
            if (_representation.IsInRangeOfBoard(_tetromino, givenYOffset: 1)  //if in range of the board
                && !_representation.CheckCollision(_tetromino, givenYOffset: 1)) //if not collides with other tetromino's
                _tetromino.OffsetY++;
            else
            {
                _representation.PutTetrominoInBoard(_tetromino);
                NewTetromino();
                Board();
            }
        }

        private void RenderBoard()
        {
            for (var y = 0; y < _representation.Board.GetLength(0); y++)
            for (var x = 0; x < _representation.Board.GetLength(1); x++)
            {
                var block = _representation.Board[y, x];
                if(block == 0) continue; //block does not need to be rendered when it is 0 because its empty
                
                var rectangle = CreateRectangle(
                    ConvertNumberToBrush(_representation.Board[y,x])); // TODO Fix colors corresponding to tetromino
                TetrisGrid.Children.Add(rectangle);
                
                Grid.SetRow(rectangle, y);
                Grid.SetColumn(rectangle, x);
            }
        }

        /// <summary>
        /// Constructs the given tetromino by getting the int[,] from the matrix. For each cell that
        /// is not '1' it creates nothing because that should be empty. For every 1 a block will be drawn.
        /// Then creates a rectangle of the mapped tetromino and places it in the given grid (trough) param.
        /// </summary>
        /// <param name="matrixValue">int[,] from the matrix that belongs to the tetromino parameter</param>
        /// <param name="tetromino"></param>
        /// <param name="grid">TetrisGrid or NextGrid for next tetromino</param>
        private void RenderTetromino(Tetromino tetromino, Grid grid)
        {
            tetromino.CalculatePositions().ForEach(coordinate => {
                var (y, x) = coordinate;
                var rectangle = CreateRectangle(Tetromino.DetermineColor(tetromino.shape));
                grid.Children.Add(rectangle);

                Grid.SetRow(rectangle, y);
                Grid.SetColumn(rectangle, grid == TetrisGrid ? x : x - 4);
            });
        }


        /// <summary>
        /// Checks if a rotation would cross the border and if so, corrects the position accordingly.
        /// </summary>

        /// <summary>
        /// C# function that triggers when a key is pressed.
        /// This is how the user controls the game
        /// </summary>
        /// <param name="e">pressed key</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_paused)
                return;

            switch (e.Key)
            {
                //move right
                case Key.Right when _representation.IsInRangeOfBoard(_tetromino, 1)
                                    && !_representation.CheckCollision(_tetromino, givenYOffset: 0, givenXOffset: 1):
                    _tetromino.OffsetX++;
                    break;
                //move left
                case Key.Left when _representation.IsInRangeOfBoard(_tetromino, -1)
                                    && !_representation.CheckCollision(_tetromino, givenYOffset: 0, givenXOffset: -1):
                    _tetromino.OffsetX--;
                    break;
                //Rotate clockwise
                case Key.Up:
                    _tetromino.Matrix = _tetromino.Matrix.Rotate90();
                    CorrectRotation();
                    break;
                //Rotate counter clockwise
                case Key.Down:
                    _tetromino.Matrix = _tetromino.Matrix.Rotate90CounterClockwise();
                    CorrectRotation();
                    break;
                //move down
                case Key.Space when _representation.IsInRangeOfBoard(_tetromino, 0, 1)
                                    && !_representation.CheckCollision(_tetromino, givenYOffset: 1):
                    _tetromino.OffsetY++;
                    break;
                //Only used in development
                case Key.E:
                    _tetromino.OffsetY--;
                    break;
                default:
                    return;
            }
            Board();
        }

        private void CorrectRotation()
        {
            if (_representation.IsInRangeOfBoard(_tetromino)) return; //return when check is not necessary 
            
            //left side of the board
            if (_tetromino.OffsetX < 0)
            {
                _tetromino.OffsetX = 0;
                return;
            }

            //right side of the board
            var xCoordinates = _tetromino.CalculatePositions().Select(coordinate => coordinate.Item2).ToList();
            _tetromino.OffsetX -= xCoordinates.Max() - _representation.Board.GetLength(1)+1 ;
        }

        private void Quit(object sender, RoutedEventArgs routedEventArgs) => Application.Current.Shutdown();

        private void Pause(object sender, RoutedEventArgs routedEventArgs)
        {
            _paused = !_paused;

            var button = (Button) sender;
            button.Content = ReferenceEquals(button.Content, "Pause") ? "Resume" : "Pause";

            if (_paused) _dpt.Stop();
            else _dpt.Start();
        }
        
        private static Rectangle CreateRectangle(Brush color) => new()
            {
                Width = 30, // Width of a 'cell' in the Grid
                Height = 30, // Height of a 'cell' in the Grid
                Stroke = Brushes.Black, // Border
                StrokeThickness = 0.75, // Border thickness
                Fill = color // Background color
            };

        private static Brush ConvertNumberToBrush(int num)
        {
            return num switch
            {
                1 => Tetromino.DetermineColor(TetrominoShape.O),
                2 => Tetromino.DetermineColor(TetrominoShape.T),
                3 => Tetromino.DetermineColor(TetrominoShape.J),
                4 => Tetromino.DetermineColor(TetrominoShape.L),
                5 => Tetromino.DetermineColor(TetrominoShape.S),
                6 => Tetromino.DetermineColor(TetrominoShape.Z),
                7 => Tetromino.DetermineColor(TetrominoShape.I),
                _ => throw new ArgumentOutOfRangeException(nameof(num), num, null)
            };
        }

        // For debugging purposes
        private void DevelopmentInfo()
        {
            var i = 1;
            foreach (var cell in _representation.Board)
            {
                
                if (i % 10 == 0)
                {
                    Console.WriteLine(cell);
                }
                else Console.Write(cell);

                i++;
            }
        }
    }
}