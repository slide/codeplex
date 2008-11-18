/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
using System.Globalization;

namespace Microsoft.Scripting {
    /// <summary>
    /// Stores the location of a span of text in a source file.
    /// TODO: move to Microsoft.Linq.Expressions
    /// TODO: review public APIs !!!
    ///       Lots of stuff on here that is not used by the compiler
    /// </summary>
    [Serializable]
    public struct SourceSpan {
        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        /// <summary>
        /// Constructs a new span with a specific start and end location.
        /// </summary>
        /// <param name="start">The beginning of the span.</param>
        /// <param name="end">The end of the span.</param>
        public SourceSpan(SourceLocation start, SourceLocation end) {
            ValidateLocations(start, end);
            this._start = start;
            this._end = end;
        }

        private static void ValidateLocations(SourceLocation start, SourceLocation end) {
            if (start.IsValid && end.IsValid) {
                if (start > end) {
                    throw Error.StartEndMustBeOrdered();
                }
            } else {
                if (start.IsValid || end.IsValid) {
                    throw Error.StartEndCanOnlyBothBeInvalid();
                }
            }
        }

        /// <summary>
        /// The start location of the span.
        /// </summary>
        public SourceLocation Start {
            get { return _start; }
        }

        /// <summary>
        /// The end location of the span. Location of the first character behind the span.
        /// </summary>
        public SourceLocation End {
            get { return _end; }
        }

        /// <summary>
        /// Length of the span (number of characters inside the span).
        /// </summary>
        public int Length {
            get { return _end.Index - _start.Index; } 
        }

        /// <summary>
        /// A valid span that represents no location.
        /// </summary>
        public static readonly SourceSpan None = new SourceSpan(SourceLocation.None, SourceLocation.None);

        /// <summary>
        /// An invalid span.
        /// </summary>
        public static readonly SourceSpan Invalid = new SourceSpan(SourceLocation.Invalid, SourceLocation.Invalid);

        /// <summary>
        /// Whether the locations in the span are valid.
        /// </summary>
        public bool IsValid {
            get { return _start.IsValid && _end.IsValid; }
        }

        /// <summary>
        /// Compares two specified Span values to see if they are equal.
        /// </summary>
        /// <param name="left">One span to compare.</param>
        /// <param name="right">The other span to compare.</param>
        /// <returns>True if the spans are the same, False otherwise.</returns>
        public static bool operator ==(SourceSpan left, SourceSpan right) {
            return left._start == right._start && left._end == right._end;
        }

        /// <summary>
        /// Compares two specified Span values to see if they are not equal.
        /// </summary>
        /// <param name="left">One span to compare.</param>
        /// <param name="right">The other span to compare.</param>
        /// <returns>True if the spans are not the same, False otherwise.</returns>
        public static bool operator !=(SourceSpan left, SourceSpan right) {
            return left._start != right._start || left._end != right._end;
        }

        public override bool Equals(object obj) {
            if (!(obj is SourceSpan)) return false;

            SourceSpan other = (SourceSpan)obj;
            return _start == other._start && _end == other._end;
        }

        public override string ToString() {
            return _start.ToString() + " - " + _end.ToString();
        }

        public override int GetHashCode() {
            // 7 bits for each column (0-128), 9 bits for each row (0-512), xor helps if
            // we have a bigger file.
            return (_start.Column) ^ (_end.Column << 7) ^ (_start.Line << 14) ^ (_end.Line << 23);
        }

        internal string ToDebugString() {
            return String.Format(CultureInfo.CurrentCulture, "{0}-{1}", _start.ToDebugString(), _end.ToDebugString());
        }
    }
}
