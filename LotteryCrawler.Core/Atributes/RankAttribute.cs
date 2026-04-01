using System;

namespace LotteryCrawler.Core.Atributes
{
    internal sealed class RankAttribute : Attribute
    {
        internal int Value { get; }

        internal RankAttribute(int value)
        {
            if (value is < 1 or > 10)
                throw new ArgumentOutOfRangeException(nameof(value), "Rank must be between 1 and 10.");

            Value = value;
        }
    }
}
    