using System;
using System.Linq;
using DiffAlgorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffAlgorithmTests
{
    [TestClass]
    public class Diff3Tests
    {
        public static string J(DiffAlgorithm.Diff3Item[] diff3Items)
        {
            return String.Join<Diff3Item>("#", diff3Items);
        }

        [TestMethod]
        public void Diff3AllSame()
        {
            string a = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                a,
                a,
                a)), "");
        }

        [TestMethod]
        public void Diff3InsertedLeft()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            string l = "a,b,c,inserted new line,another inserted,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                o)), "3.3.3^0.2.0");
        }

        [TestMethod]
        public void Diff3InsertedRight()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            string l = "a,b,c,inserted new line,another inserted,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                o,
                l)), "3.3.3^0.0.2");
        }

        [TestMethod]
        public void Diff3InsertedLeftRight()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            string l = "a,b,c,inserted left1,another inserted left,d,e,f,g,h,i,j,inserted left3,k,l".Replace(',', '\n');
            string r = "a,b,c,d,e,f,another inserted right,right2,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                r)), "3.3.3^0.2.0#6.8.6^0.0.2#10.12.12^0.1.0");
        }

        [TestMethod]
        public void Diff3ChangedLeft()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            string l = "a,b,line3,line4,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                o)), "2.2.2^2.2.2");
        }

        [TestMethod]
        public void Diff3ChangedRight()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
            string l = "a,b,line3,line4,e,f,g,h,i,j,k,l".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                o,
                l)), "2.2.2^2.2.2");
        }

        [TestMethod]
        public void Diff3Complex()
        {
            string o = "a, tento radek se zmeni, c, tento radek se smazal, a tennto se taky smazal, d, e, f, g, h, i, j, k".Replace(',', '\n');
            string l = "a, zmeneny radek, c, d, muj novy radek prvni, muj novy radek druhy, e, f, tento radek pribyl, g, h, muj novy radek, i, j, k".Replace(',', '\n');
            string r = "a, zmeneny radek, c, d, e, f, tento radek pribyl s konfliktem, g, h, i, j, jeho novy radek, jedo druhy radek, k".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                r)), "1.1.1^1.1.1#3.3.3^2.0.0#6.4.4^0.2.0#8.8.6^0.1.1!!#10.11.9^0.1.0#12.14.11^0.0.2");
        }

        [TestMethod]
        public void Diff3TwoSimpleOverflows()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k".Replace(',', '\n');
            string l = "a,b,i1,i2,c,d,echange,fchange,g,h,i,j,k".Replace(',', '\n');
            string r = "a,b,d,e,f,gchange,h,i,j,k".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                r)), "2.2.2^1.3.0!!#4.6.3^3.3.3!!");
        }

        [TestMethod]
        public void Diff3SimpleOverflowAndLongOverflow()
        {
            string o = "a,b,c,d,e,f,g,h,i,j,k".Replace(',', '\n');
            string l = "a,b,i1,i2,c,d,echange,fchange,g,hchange,inserted row,i,j,k".Replace(',', '\n');
            string r = "a,b,d,e,f,gchange,h,ichange,jchange,k".Replace(',', '\n');
            var d = new DiffHelper();

            Assert.AreEqual(J(d.DiffText(
                o,
                l,
                r)), "2.2.2^1.3.0!!#4.6.3^6.7.6!!");
        }
    }
}
