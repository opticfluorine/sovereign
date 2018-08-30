using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineUtil.Monads
{

    /// <summary>
    /// Implementation of the "Maybe" monad.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class Maybe<T>
    {

        /// <summary>
        /// Exception type thrown when no value is present.
        /// </summary>
        [System.Serializable]
        public class NoValueException : Exception
        {
            public NoValueException() { }
            public NoValueException(string message) : base(message) { }
            public NoValueException(string message, Exception inner) : base(message, inner) { }
            protected NoValueException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        /// <summary>
        /// Whether a value is present.
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Underlying value.
        /// </summary>
        private T value;

        /// <summary>
        /// Provides access to the underlying value.
        /// </summary>
        /// <exception cref="NoValueException">
        /// Thrown if the property getter is invoked when there is no value present.
        /// </exception>
        public T Value
        {
            get
            {
                if (!HasValue) throw new NoValueException();
                return value;
            }

            set
            {
                this.value = value;
                HasValue = true;
            }
        }

        /// <summary>
        /// Creates a Maybe with no underlying value.
        /// </summary>
        public Maybe() { }

        /// <summary>
        /// Creates a Maybe with an underlying value.
        /// </summary>
        /// <param name="value">Underlying value.</param>
        public Maybe(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the underlying value, or returns a default value if
        /// no underlying value is present.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Underlying value, or defaultValue if no value is present.</returns>
        public T OrElseDefault(T defaultValue)
        {
            return HasValue ? Value : defaultValue;
        }

        /// <summary>
        /// Gets the underlying value, or returns the value provided by another
        /// function if no underlying value is present.
        /// </summary>
        /// <param name="supplier">Function to supply a value if none present.</param>
        /// <returns>Value, or the return value of supplier if no value is present.</returns>
        public T OrElse(Func<T> supplier)
        {
            return HasValue ? Value : supplier();
        }

    }

}
