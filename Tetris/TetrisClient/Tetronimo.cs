﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using static TetrisClient.TetronimoShape;

namespace TetrisClient
{
    /// <summary>
    /// Enum that represents the different kinds of tetronimo's.
    /// </summary>
    public enum TetronimoShape
    {
        O,
        T,
        J,
        L,
        S,
        Z,
        I
    }

    public class Tetronimo
    {
        public TetronimoShape shape { get; private set; }
        
        public Matrix Matrix { get; set; }
        public int OffsetX;
        public int OffsetY;

        /// <summary>
        /// Default start position is at the left top (0,0).
        /// Constructor overloading is used if alternate spawnpoints
        /// are being chosen.
        /// </summary>
        public Tetronimo() => new Tetronimo(0, 0);

        public Tetronimo(int offsetX, int offsetY)
        {
            var generatedShape = GenerateShape();
            shape = generatedShape;
            Matrix = CreateShape(generatedShape);
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
        }
        
        //gets all y,x positions
        public List<(int, int)> CalculatePositions() 
        {
            var coordinates = new List<(int, int)>();
            for (var y = 0; y < Matrix.Value.GetLength(0); y++)
            for (var x = 0; x < Matrix.Value.GetLength(1); x++)
            {
                if(Matrix.Value[y,x] == 0) continue; //block does not need to be rendered when it is 0 because its empty
                coordinates.Add((y + OffsetY, x + OffsetX));
            }
            return coordinates;
        }

        /// <summary>
        /// Picks a random tetronimo.
        /// </summary>
        /// <returns>TetronimoShape enum</returns>
        private static TetronimoShape GenerateShape()
        {
            var values = Enum.GetValues(typeof(TetronimoShape));
            return (TetronimoShape) values.GetValue(new Random().Next(values.Length));
        }

        /// <summary>
        /// Gives back the 3D array that corresponds with the given tetronimo shape enum.
        /// </summary>
        /// <param name="shape">TetronimoShape enum</param>
        /// <returns>3D array that represents a tetronimo of the passed enum</returns>
        /// <exception cref="ArgumentOutOfRangeException">when an invalid entry is passed</exception>
        private static Matrix CreateShape(TetronimoShape shape) => shape switch
        {
            O => new Matrix( new [,]{{1, 1}, {1, 1}}),
            T => new Matrix( new [,]{{1, 1, 1}, {0, 1, 0}, {0, 0, 0}}),
            J => new Matrix( new [,]{{0, 1, 0}, {0, 1, 0}, {1, 1, 0}}),
            L => new Matrix( new [,]{{0, 1, 0}, {0, 1, 0}, {0, 1, 1}}),
            S => new Matrix( new [,]{{0, 1, 1}, {1, 1, 0}, {0, 0, 0}}),
            Z => new Matrix( new [,]{{1, 1, 0}, {0, 1, 1}, {0, 0, 0}}),
            I => new Matrix( new [,]{{0, 0, 0, 0}, {1, 1, 1, 1}, {0, 0, 0, 0}, {0, 0, 0, 0}}),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };

        /// <summary>
        /// Tetronimo's have specific colors.
        /// Here those colors are bound to the corresponding enum shape. 
        /// </summary>
        /// <param name="shape">TetronimoShape enum</param>
        /// <returns>color that corresponds with the given tetronimo shope</returns>
        /// <exception cref="ArgumentOutOfRangeException">when an invalid entry is passed</exception>
        public static Brush DetermineColor(TetronimoShape shape) => shape switch
        {
            O => Brushes.Yellow,
            T => Brushes.Purple,
            J => Brushes.Blue,
            L => Brushes.Orange,
            S => Brushes.Green,
            Z => Brushes.Red,
            I => Brushes.Aqua,
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };
    }
}