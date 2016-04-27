using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using DeltaDNA;

namespace DeltaDNAAds {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class RewardedAdTest {

        [Test]
        public void CreatedWithoutEngagement()
        {
            var rewardedAd = RewardedAd.Create();

            Assert.IsNotNull(rewardedAd);
            CollectionAssert.IsEmpty(rewardedAd.Parameters);
        }

        [Test]
        public void CreatedWithEngagement()
        {
            var engagement = new Engagement("testDecisionPoint");
            var rewardedAd = RewardedAd.Create(engagement);

            Assert.IsNotNull(rewardedAd);
            CollectionAssert.IsEmpty(rewardedAd.Parameters);
        }

        [Test]
        public void ReturnsNullIfAdShowPointIsFalse()
        {
            var engagement = new Engagement("testDecisionPoint");
            engagement.Raw = "{ \"parameters\": { \"adShowPoint\" : false } }";
           
            var rewardedAd = RewardedAd.Create(engagement);

            Assert.IsNull(rewardedAd);
        }

        [Test]
        public void CreatedIfAdShowPointIsTrue()
        {
            var engagement = new Engagement("testDecisionPoint");
            engagement.Raw = "{ \"parameters\": { \"adShowPoint\" : true } }";
           
            var rewardedAd = RewardedAd.Create(engagement);

            Assert.IsNotNull(rewardedAd);
            CollectionAssert.AreEquivalent(new JSONObject() {
                { "adShowPoint", true }
            }, rewardedAd.Parameters);
        }
    }
}
