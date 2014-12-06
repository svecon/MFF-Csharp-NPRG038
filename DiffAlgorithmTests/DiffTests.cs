using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using DiffAlgorithm;

namespace DiffAlgorithmTests
{
    [TestClass]
    public class DiffTests
    {
        public static string TestHelper(DiffAlgorithm.Item[] diffItems)
        {
            StringBuilder ret = new StringBuilder();
            foreach (var item in diffItems)
                ret.Append(item.ToString());

            return ret.ToString();
        }

        [TestMethod]
        public void DiffAllSame()
        {
            var a = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            Diff d = new Diff();

            Assert.AreEqual(TestHelper(d.DiffText(
                a,
                a,
                false, false, false)), "");
        }

        [TestMethod]
        public void DiffAllChanged()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n'),
                "0,1,2,3,4,5,6,7,8,9".Replace(',', '\n'),
                false, false, false)), "12.10.0.0*");
        }

        [TestMethod]
        public void DiffTestSnake()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,c,d,e,f".Replace(',', '\n'),
                "b,c,d,e,f,x".Replace(',', '\n'),
                false, false, false)), "1.0.0.0*0.1.6.5*");
        }

        [TestMethod]
        public void DiffSomeChanges()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "a,b,-,c,d,e,f,f".Replace(',', '\n'),
                "a,b,x,c,e,f".Replace(',', '\n'),
                false, false, false)), "1.1.2.2*1.0.4.4*1.0.6.5*");
        }

        [TestMethod]
        public void DiffComplex()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n'),
                "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n'),
                false, false, false)), "1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*");
        }

        [TestMethod]
        public void DiffComplex2()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n'),
                "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n'),
                false, false, false)), "1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*");
        }

        [TestMethod]
        public void DiffSingleLineVsMultiple()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "F".Replace(',', '\n'),
                "0,F,1,2,3,4,5,6,7".Replace(',', '\n'),
                false, false, false)), "0.1.0.0*0.7.1.2*");
        }

        [TestMethod]
        public void DiffHelloWorld()
        {
            Diff d = new Diff();
            Assert.AreEqual(TestHelper(d.DiffText(
                "HELLO\nWORLD".Replace(',', '\n'),
                "\n\nhello\n\n\n\nworld\n".Replace(',', '\n'),
                false, false, false)), "2.8.0.0*");
        }

    }
}
