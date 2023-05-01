namespace ContiniousInterop.Tests;

public class TestLogGeneration
{
    [TestMethod]
    public void WillPass() { }

    [TestMethod]
    public void WillFail() => Assert.Fail();

    [TestMethod]
    [Ignore]
    public void Willignore() { }
}
