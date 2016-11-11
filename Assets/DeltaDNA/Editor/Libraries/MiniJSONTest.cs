using NUnit.Framework;
using System.Threading;

namespace DeltaDNA {

    class MiniJSONTest {

        [Test]
        public void SerialisingDecimalPointsWithLocaleUsingCommas() {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sk-SK");

            Assert.AreEqual("1.23", MiniJSON.Json.Serialize(1.23d));
        }
    }
}
