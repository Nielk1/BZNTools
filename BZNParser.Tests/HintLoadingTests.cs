using BZNParser.Battlezone;
using Xunit;

namespace BZNParser.Tests;

public class HintLoadingTests
{
    [Fact]
    public void LoadsBattlezoneHintsCopiedBesideTheApplication()
    {
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "BZ1_ClassLabels.txt")));
        var hints = BattlezoneBZNHints.BuildHintsBZ1();
        Assert.NotNull(hints);
        Assert.NotNull(hints.ClassLabels);
        Assert.NotEmpty(hints.ClassLabels);
        Assert.NotNull(hints.EnumerationPrjID);
        Assert.NotEmpty(hints.EnumerationPrjID);
    }

    [Fact]
    public void LoadsBattlezone2HintsCopiedBesideTheApplication()
    {
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "BZ2_ClassLabels.txt")));
        var hints = BattlezoneBZNHints.BuildHintsBZ2();
        Assert.NotNull(hints);
        Assert.NotNull(hints.ClassLabels);
        Assert.Contains("deposit", hints.ClassLabels["cepool01"]);
    }
}
