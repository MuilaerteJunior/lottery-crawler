namespace LotteryCrawler.App.Tests
{
    public class AppTests
    {

        [Theory]
        [InlineData("h")]
        [InlineData("Q")]
        public void ProcessArgs_Validate_Input_Option(string option)
        {
            // Arrange
            var args = new[] { option };

            // Act
            var result = App.ProcessArgs(args, new CoreConfig());

            // Assert
            Assert.Equal(option, result.optionMenu, ignoreCase: true);
        }
    }
}
