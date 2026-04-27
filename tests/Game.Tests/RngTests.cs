using FluentAssertions;
using Game.Core;

namespace Game.Tests;

public class RngTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var seed = new GameSeed(12345);
        var rng1 = new DeterministicRng(seed);
        var rng2 = new DeterministicRng(seed);

        var sequence1 = Enumerable.Range(0, 100).Select(_ => rng1.Next()).ToList();
        var sequence2 = Enumerable.Range(0, 100).Select(_ => rng2.Next()).ToList();

        sequence1.Should().Equal(sequence2);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var rng1 = new DeterministicRng(new GameSeed(111));
        var rng2 = new DeterministicRng(new GameSeed(222));

        var sequence1 = Enumerable.Range(0, 10).Select(_ => rng1.Next()).ToList();
        var sequence2 = Enumerable.Range(0, 10).Select(_ => rng2.Next()).ToList();

        sequence1.Should().NotEqual(sequence2);
    }

    [Fact]
    public void NextWithMax_StaysInRange()
    {
        var rng = new DeterministicRng(new GameSeed(999));

        for (int i = 0; i < 1000; i++)
        {
            var value = rng.Next(10);
            value.Should().BeGreaterThanOrEqualTo(0);
            value.Should().BeLessThan(10);
        }
    }

    [Fact]
    public void NextWithMinMax_StaysInRange()
    {
        var rng = new DeterministicRng(new GameSeed(888));

        for (int i = 0; i < 1000; i++)
        {
            var value = rng.Next(5, 15);
            value.Should().BeGreaterThanOrEqualTo(5);
            value.Should().BeLessThan(15);
        }
    }

    [Fact]
    public void NextDouble_StaysInZeroToOneRange()
    {
        var rng = new DeterministicRng(new GameSeed(777));

        for (int i = 0; i < 1000; i++)
        {
            var value = rng.NextDouble();
            value.Should().BeGreaterThanOrEqualTo(0.0);
            value.Should().BeLessThan(1.0);
        }
    }

    [Fact]
    public void Shuffle_SameSeed_ProducesSameOrder()
    {
        var seed = new GameSeed(54321);

        var list1 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var list2 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        new DeterministicRng(seed).Shuffle(list1);
        new DeterministicRng(seed).Shuffle(list2);

        list1.Should().Equal(list2);
    }

    [Fact]
    public void Shuffle_PreservesAllElements()
    {
        var rng = new DeterministicRng(new GameSeed(666));
        var original = new List<int> { 1, 2, 3, 4, 5 };
        var shuffled = new List<int>(original);

        rng.Shuffle(shuffled);

        shuffled.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Fork_ProducesDifferentSequence()
    {
        var rng = new DeterministicRng(new GameSeed(111));
        var forked = rng.Fork();

        var mainSequence = Enumerable.Range(0, 10).Select(_ => rng.Next()).ToList();
        var forkedSequence = Enumerable.Range(0, 10).Select(_ => forked.Next()).ToList();

        mainSequence.Should().NotEqual(forkedSequence);
    }

    [Fact]
    public void Fork_IsDeterministic()
    {
        var seed = new GameSeed(999);

        var rng1 = new DeterministicRng(seed);
        rng1.Next(); // advance
        var forked1 = rng1.Fork();

        var rng2 = new DeterministicRng(seed);
        rng2.Next(); // advance same amount
        var forked2 = rng2.Fork();

        var seq1 = Enumerable.Range(0, 10).Select(_ => forked1.Next()).ToList();
        var seq2 = Enumerable.Range(0, 10).Select(_ => forked2.Next()).ToList();

        seq1.Should().Equal(seq2);
    }
}
