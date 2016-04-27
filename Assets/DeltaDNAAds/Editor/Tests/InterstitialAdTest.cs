using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using DeltaDNA;

namespace DeltaDNAAds {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class InterstitialAdTest {

        [Test]
        public void CreatedWithoutEngagement()
        {
            var interstitialAd = InterstitialAd.Create();

            Assert.IsNotNull(interstitialAd);
            CollectionAssert.IsEmpty(interstitialAd.Parameters);
        }

        [Test]
        public void CreatedWithEngagement()
        {
            var engagement = new Engagement("testDecisionPoint");
            var interstitialAd = InterstitialAd.Create(engagement);

            Assert.IsNotNull(interstitialAd);
            CollectionAssert.IsEmpty(interstitialAd.Parameters);
        }

        [Test]
        public void ReturnsNullIfAdShowPointIsFalse()
        {
            var engagement = new Engagement("testDecisionPoint");
            engagement.Raw = "{ \"parameters\": { \"adShowPoint\" : false } }";
           
            var interstitialAd = InterstitialAd.Create(engagement);

            Assert.IsNull(interstitialAd);
        }

        [Test]
        public void CreatedIfAdShowPointIsTrue()
        {
            var engagement = new Engagement("testDecisionPoint");
            engagement.Raw = "{ \"parameters\": { \"adShowPoint\" : true } }";
           
            var interstitialAd = InterstitialAd.Create(engagement);

            Assert.IsNotNull(interstitialAd);
            CollectionAssert.AreEquivalent(new JSONObject() {
                { "adShowPoint", true }
            }, interstitialAd.Parameters);
        }
    }
}
