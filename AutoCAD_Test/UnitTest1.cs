using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoCAD_Version;

namespace AutoCAD_Test
{
    [TestClass]
    public class UnitTest1
    {
        MainWindowModel model = null;

        [TestInitialize]
        public void Setup()
        {
            model = new MainWindowModel();
        }

        [TestMethod]
        public void TestStatusBarText()
        {
            Assert.AreEqual(model.StatusBarText, "Click Browse to select a folder to process...");
        }

        [TestMethod]
        public void TestFirst6Chars()
        {
            string filePath = "";
            Assert.AreEqual(model.GetFirstChars(filePath), ("", ""));
        }

        [TestCleanup]
        public void TearDown()
        {
            model = null;
        }
    }
}
