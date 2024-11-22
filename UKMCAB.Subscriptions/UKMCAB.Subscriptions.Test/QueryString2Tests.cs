using UKMCAB.Subscriptions.Core.Common;

namespace UKMCAB.Subscriptions.Test;

public class QueryString2Tests
{
    [Test]
    public void Test()
    {
        var q1 = QueryString2.Parse("a=b&a=c&d=e&f=g&g=h").ToString();
        var q2 = QueryString2.Parse("?a=b&a=c&d=e&f=g&g=h&").ToString();
        var q3 = QueryString2.Parse("&d=e&f=g&a=b&a=c&g=h&&&&").ToString();
        var q4 = QueryString2.Parse("&g=h&a=c&d=e&f=g&a=b&").ToString();

        Assert.That(new[] { q1, q2, q3, q4 }.All(x => x.Equals(q1)));
    }
}