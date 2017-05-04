using System;
using bsk___proba_2;
using NUnit.Framework;

namespace UnitTestProject1
{
    [TestFixture]
    public class TestRBACa
    {
        [Test]
        public void TestMethod1()
        {
            TestDelegate testDelegate = ()=>RBACowyConnector.Inicjalizuj("localhost", "tomasz152", "bombelek", "15000");
            Assert.DoesNotThrow(testDelegate);
        }
    }
}
