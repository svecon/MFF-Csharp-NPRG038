using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using DiffAlgorithm;
using DiffAlgorithm.TwoWay;

namespace DiffAlgorithmTests
{
    [TestClass]
    public class DiffTests
    {
        public static string TestHelper(DiffItem[] diffDiffItems)
        {
            var ret = new StringBuilder();
            foreach (DiffItem item in diffDiffItems)
                ret.Append(item.ToString());

            return ret.ToString();
        }

        [TestMethod]
        public void DiffAllSame()
        {
            string a = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(TestHelper(d.DiffText(
                a,
                a)), "");
        }

        [TestMethod]
        public void DiffAllChanged()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n'),
                "0,1,2,3,4,5,6,7,8,9".Replace(',', '\n'))),
                "12.10.0.0*");
        }

        [TestMethod]
        public void DiffTestSnake()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,c,d,e,f".Replace(',', '\n'),
                "b,c,d,e,f,x".Replace(',', '\n'))),
                "1.0.0.0*0.1.6.5*");
        }

        [TestMethod]
        public void DiffSomeChanges()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,-,c,d,e,f,f".Replace(',', '\n'),
                "a,b,x,c,e,f".Replace(',', '\n'))),
                "1.1.2.2*1.0.4.4*1.0.7.6*");
        }

        [TestMethod]
        public void DiffComplex()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n'),
                "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n'))),
                "1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*");
        }

        [TestMethod]
        public void DiffLongChainOfRepeats()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,a,a,a,a,a,a,a,a,a".Replace(',', '\n'),
                "a,a,a,a,-,a,a,a,a,a".Replace(',', '\n'))),
                "0.1.4.4*1.0.9.10*");
        }

        [TestMethod]
        public void DiffComplex2()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n'),
                "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n'))), 
                "1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*");
        }

        [TestMethod]
        public void DiffSingleLineVsMultiple()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "F".Replace(',', '\n'),
                "0,F,1,2,3,4,5,6,7".Replace(',', '\n'))),
                "0.1.0.0*0.7.1.2*");
        }

        [TestMethod]
        public void DiffHelloWorld()
        {
            var d = new DiffHelper();
            Assert.AreEqual(TestHelper(d.DiffText(
                "HELLO\nWORLD".Replace(',', '\n'),
                "\n\nhello\n\n\n\nworld\n".Replace(',', '\n'))),
                "2.8.0.0*");
        }

    }
}
