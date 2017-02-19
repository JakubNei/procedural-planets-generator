using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// http://www.dreamincode.net/forums/topic/203446-worst-c%23-code-youve-ever-seen-in-production/page__view__findpost__p__1191628
// "When I first heard of this project, which has been in ongoing development for almost 8 years, we were told to NEVER, EVER touch the VirtualBrain class because it housed the code that ran the entire system."

namespace MyEngine
{
    /// <summary>
    /// Summary description for VirtualBrain.
    /// </summary>
    public class VirtualBrain
    {
        /// <summary>
        /// VirtualBrain constructor
        /// </summary>
        public VirtualBrain()
        {

        }

        /// <summary>
        /// Gets the Ultimate Answer to Life the Universe and Everything
        /// </summary>
        /// <returns>the Ultimate Answer to Life the Universe and Everything</returns>
        /// <remarks>http://en.wikipedia.org/wiki/The_Answer_to_Life,_the_Universe,_and_Everything</remarks>
        public int GetTheUltimateAnswerToLifeTheUniverseAndEverything()
        {
            return 1 + 5 * 8 + 1;
        }
    }

}
