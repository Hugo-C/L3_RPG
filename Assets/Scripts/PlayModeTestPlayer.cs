using System.Collections;

namespace DefaultNamespace
{
    public class PlayModeTestPlayer
    {
        //[NUnit.Framework.Test]
        public void PlayModeTestPlayerSimplePasses() {
            // Use the Assert class to test conditions.
            
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        /*[UnityEngine.TestTools.UnityTest]
        public IEnumerator PlayModeTestPlayerWithEnueratorPasses() {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }*/
    }
}