using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualMaze.Assets.Scripts.Utils
{
/// <summary>
/// Represents an optional value that can either contain a value or be empty.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
    public class Optional<T>
    {
        private readonly T value;
        private readonly bool hasValue;

        /// <summary>
        /// Creates an instance of Optional with a value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        private Optional(T value)
        {
            this.value = value;
            this.hasValue = true;
        }

        private Optional() {
            this.value = default(T);
            this.hasValue = false;
        }

        /// <summary>
        /// Gets an instance of Optional with a specified value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>An instance of Optional containing the specified value.</returns>
        public static Optional<T> Some(T value) => new Optional<T>(value);

        /// <summary>
        /// Gets an instance of Optional representing an empty value.
        /// </summary>
        public static Optional<T> None => new Optional<T>();

        /// <summary>
        /// Gets a value indicating whether this instance has a value.
        /// </summary>
        public bool HasValue => hasValue;

        /// <summary>
        /// Gets the value contained in this instance.
        /// Throws an exception if this instance is empty.
        /// </summary>
        public T Value => hasValue ? value : throw new InvalidOperationException("Optional has no value.");

        /// <summary>
        /// Applies a function to the value if it exists.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to apply to the value.</param>
        /// <returns>An Optional containing the result of the function, or an empty Optional if this instance is empty.</returns>
        public Optional<TResult> Map<TResult>(Func<T, TResult> func)
        {
            return hasValue ? Optional<TResult>.Some(func(value)) : Optional<TResult>.None;
        }

        /// <summary>
        /// Applies a function to the value if it exists, flattening the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to apply to the value.</param>
        /// <returns>An Optional containing the result, or an empty Optional if this instance is empty.</returns>
        public Optional<TResult> FlatMap<TResult>(Func<T, Optional<TResult>> func)
        {
            return hasValue ? func(value) : Optional<TResult>.None;
        }

        public Optional<T> Filter(Func<T, bool> predicate) {
            if (HasValue && predicate(Value))
            {
                return this;
            }
            else
            {
                return None;
            }
        }

        
    }
}