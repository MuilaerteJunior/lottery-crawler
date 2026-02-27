using LotteryCrawler.Study;

namespace LotteryCrawler.Study.Tests
{
    public class StudyAppTests
    {

        [Fact]
        public void ProcessArgs_WithStudyAndVerbosity_ParsesExpectedValues()
        {
            // Arrange
            var args = new[] { "s", "7", "-v" };

            // Act
            var result = StudyApp.ProcessArgs(args);

            // Assert
            Assert.Equal("s", result.optionMenu);
            Assert.Equal((short)7, result.howManyNumbers);
            Assert.True(result.verbosity);
        }

        [Theory]
        [InlineData("h")]
        [InlineData("r")]
        [InlineData("s")]
        public void ProcessArgs_Validate_Input_Option(string option)
        {
            // Arrange
            var args = new[] { option };

            // Act
            var result = StudyApp.ProcessArgs(args);

            // Assert
            Assert.Equal(option, result.optionMenu);
        }
    }
}
