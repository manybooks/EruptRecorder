using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EruptRecorder.Settings;

namespace EruptRecorderUnitTest.Settings
{
    [TestClass]
    public class SettingsTest
    {
        [DataRow(true, 1, "jpg", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "bmp", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "abcde", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "*", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [TestMethod]
        public void FileExtensionValidationTest(bool isActive, int index, string fileExtension, string srcDir, string destDir)
        {
            CopySetting copySetting = new CopySetting();
            copySetting.srcDir = srcDir;
            copySetting.destDir = destDir;

            Assert.IsTrue(copySetting.IsValid());
        }

        [DataRow(true, 1, "Jpg", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, ".bmp", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "あ", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "123", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "a*", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, "", @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [DataRow(true, 1, null, @"C:\Users\user\storage", @"C:\Users\user\storage")]
        [TestMethod]
        public void FileExtensionValidationWithInvalidArgsTest(bool isActive, int index, string fileExtension, string srcDir, string destDir)
        {
            CopySetting copySetting = new CopySetting();
            copySetting.srcDir = srcDir;
            copySetting.destDir = destDir;

            Assert.ThrowsException<InvalidSettingsException>(() => copySetting.IsValid());
        }
    }
}
