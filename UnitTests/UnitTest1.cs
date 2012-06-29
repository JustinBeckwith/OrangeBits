using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrangeBits.Compilers;
using dotless.Core;
using System.IO;
using dotless.Core.Parser;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.configuration;
using dotless.Core.Parser.Tree;

namespace UnitTests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class UnitTest1
	{
		public UnitTest1()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;
        

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestMethod1()
		{
            string rootPath = @"C:\Users\justbe\Dropbox\Code\OrangeBits\Demo\";
			string inPath = rootPath + @"importTest.less";
			string outPath = rootPath + @"importTest.css";
           

            LessEngine engine = new LessEngine();
            SetCurrentFilePath(engine.Parser, inPath);
                                    
            var fileReader = new FileReader();
            var source = fileReader.GetFileContents(inPath);
            var css = engine.TransformToCss(source, inPath);           
		}
        
        private void SetCurrentFilePath(Parser lessParser, string currentFilePath)
        {
            var importer = lessParser.Importer as Importer;
            if(importer != null)
            {
                var fileReader = importer.FileReader as FileReader;

                if(fileReader == null)
                {
                    importer.FileReader = fileReader = new FileReader();
                }

                var pathResolver = fileReader.PathResolver as ImportedFilePathResolver;

                if(pathResolver != null)
                {
                    pathResolver.CurrentFilePath = currentFilePath;
                }
                else
                {
                   fileReader.PathResolver = new ImportedFilePathResolver(currentFilePath);
                }
            }
            else
            {
                throw new InvalidOperationException("Unexpected importer type on dotless parser");
            }          
        }
	}



    public class ImportedFilePathResolver : IPathResolver
    {
        private string currentFileDirectory;
        private string currentFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedFilePathResolver"/> class.
        /// </summary>
        /// <param name="currentFilePath">The path to the currently processed file.</param>
        public ImportedFilePathResolver(string currentFilePath)
        {
            CurrentFilePath = currentFilePath;
        }

        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        public string CurrentFilePath
        {
            get { return currentFilePath; }
            set
            {
                currentFilePath = value;
                currentFileDirectory = Path.GetDirectoryName(value);
            }
        }

        public string GetFullPath(string path)
        {
            return Path.GetDirectoryName(this.currentFilePath) + path;
        }
    }
}
