using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;

using dotless.Core;
using dotless.Core.Parser.Tree;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Exceptions;
using dotless.Core.Parser;
using dotless.Core.Importers;
using dotless.Core.Input;


namespace OrangeBits.Compilers
{
    /// <summary>
    /// 
    /// </summary>
    public class LessCompiler : ICompiler
    {
        //--------------------------------------------------------------------------
        //
        //	ICompiler Methods
        //
        //--------------------------------------------------------------------------

        #region CompileResults
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
        public CompileResults Compile(string inPath, string outPath)
        {
            using (StreamReader sr = new StreamReader(inPath))
            {
                LessEngine engine = new LessEngine();
                SetCurrentFilePath(engine.Parser, inPath);
                var fileReader = new FileReader();
                var source = fileReader.GetFileContents(inPath);
                var output = engine.TransformToCss(source, inPath);
				if (engine.LastTransformationSuccessful)
				{
					using (StreamWriter sw = new StreamWriter(outPath))
					{
						sw.WriteLine(OrangeBits.GetHeader(inPath));
						sw.Write(output);
					}
				}
				else
				{
					throw new Exception("Error compiling LESS");
				}
            }
            return null;
        }
        #endregion

        //--------------------------------------------------------------------------
        //
        //	Helper Methods
        //
        //--------------------------------------------------------------------------

        #region SetCurrentFilePath
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lessParser"></param>
        /// <param name="currentFilePath"></param>
        private void SetCurrentFilePath(Parser lessParser, string currentFilePath)
        {
            var importer = lessParser.Importer as Importer;
            if (importer != null)
            {
                var fileReader = importer.FileReader as FileReader;

                if (fileReader == null)
                {
                    importer.FileReader = fileReader = new FileReader();
                }

                var pathResolver = fileReader.PathResolver as ImportedFilePathResolver;

                if (pathResolver != null)
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
        #endregion
    }

    #region ImportedFilePathResolver
    /// <summary>
    /// 
    /// </summary>
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
            return Path.Combine(Path.GetDirectoryName(this.currentFilePath), path);
        }
    }
    #endregion

}


