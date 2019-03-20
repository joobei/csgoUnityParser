
using NUnit.Framework;
using System.IO;

[TestFixture]
public class extensionsTest
{
    [Test]
    public void testContains()
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        Assert.True(alphabet.ContainsAll("A", "Z"));
        Assert.True(alphabet.ContainsAll(alphabet.ToCharArray()));
        Assert.True(alphabet.ContainsAll(new char[] { 'A' }));
        Assert.False(alphabet.Contains("a"));
    }

    [Test]
    public void testCleanInput()
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string validChars = alphabet + alphabet.ToLower() + "0123456789.";
        string pathExample = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        string cleanedString = invalidChars.CleanInput();
        string untouchedString = validChars.CleanInput();
        string pathUntouched = pathExample.CleanInput(true);

        Assert.AreEqual(validChars, untouchedString);
        Assert.AreEqual("", cleanedString);
        Assert.AreEqual(pathExample, pathUntouched);
    }
}

