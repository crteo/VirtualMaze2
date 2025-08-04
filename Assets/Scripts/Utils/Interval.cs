using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Represents a closed interval with integer start and end points.
/// </summary>
namespace VirtualMaze.Assets.Utils {
    public class Interval {
        /// <summary>
        /// Gets the start point of the interval.
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Gets the end point of the interval.
        /// </summary>
        public int End { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Interval"/> class with the specified start and end points.
        /// </summary>
        /// <param name="start">The start point of the interval.</param>
        /// <param name="end">The end point of the interval.</param>
        /// <exception cref="ArgumentException">Thrown when the start point is greater than the end point.</exception>
        public Interval(int start, int end) {
            if (start > end) {
                throw new ArgumentException("Start value must be less than or equal to End value.");
            }

            Start = start;
            End = end;
        }

        /// <summary>
        /// Determines whether the interval contains the specified point.
        /// </summary>
        /// <param name="point">The point to check for containment.</param>
        /// <returns>True if the interval contains the specified point; otherwise, false.</returns>
        public bool Contains(int point) {
            return point >= Start && point <= End;
        }

        /// <summary>
        /// Determines whether the current interval overlaps with another interval.
        /// </summary>
        /// <param name="otherInterval">The interval to check for overlap.</param>
        /// <returns>True if the intervals overlap; otherwise, false.</returns>
        public bool Overlaps(Interval otherInterval) {
            return (otherInterval.Contains(this.Start) ||
            otherInterval.Contains(this.End) || 
            this.Contains(otherInterval.Start) || 
            this.Contains(otherInterval.End));
        }

        /// <summary>
        /// Calculates the length of the interval.
        /// </summary>
        /// <returns>The length of the interval.</returns>
        public int Length() {
            return End - Start + 1;
        }
    }
}
