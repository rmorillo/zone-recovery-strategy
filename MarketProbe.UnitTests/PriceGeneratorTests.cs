using System;
using Xunit;

namespace MarketProbe.UnitTests
{
    public class PriceGeneratorTests
    {
        [Fact]
        public void BasicTest()
        {
            //Arrange
            var random = new Random(unchecked((int)DateTime.Now.Ticks));

            var priceGenerator = new PriceGenerator(DateTime.Now.Millisecond, 4, 10, random.Next(800, 1300), 0.004, 0.002, 3);

            //Act
            var nextQuote = priceGenerator.NextQuote;

            //Assert
            Assert.True(nextQuote.Bid > 0 && nextQuote.Ask > 0);
        }
    }
}
