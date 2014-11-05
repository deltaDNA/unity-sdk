using UnityEngine;
using UnityTest;

[IntegrationTest.DynamicTestAttribute("DeltaDNAIntegrationTests")]
[IntegrationTest.SucceedWithAssertions]
[IntegrationTest.TimeoutAttribute(1)]
public class TestSDKStarted : MonoBehaviour {

	// Tests that the SDK has been started by someother object.
	// Shows how to do custom integration tests.
	public void Update()
	{
		// Assert that the DeltaDNA SDK has been started.
		if (DeltaDNA.SDK.Instance.IsInitialised) {
			IntegrationTest.Pass(gameObject);
		}
	}
}
