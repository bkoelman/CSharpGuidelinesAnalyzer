﻿using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    public readonly struct WordToken : IEquatable<WordToken>
    {
        [NotNull]
        public string Text { get; }

        public WordTokenKind Kind { get; }

        public WordToken([NotNull] string text, WordTokenKind kind)
        {
            Guard.NotNull(text, nameof(text));

            Text = text;
            Kind = kind;
        }

        public bool Equals(WordToken other)
        {
            return other.Text == Text && other.Kind == Kind;
        }

        public override bool Equals(object obj)
        {
            return obj is WordToken wordToken && Equals(wordToken);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Kind.GetHashCode();
        }

        public override string ToString()
        {
            return Kind + ": " + Text;
        }

        public static bool operator ==(WordToken left, WordToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WordToken left, WordToken right)
        {
            return !(left == right);
        }
    }
}
