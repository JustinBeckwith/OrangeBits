namespace OrangeBits.Compilers
{
    /// <summary>
    /// 
    /// </summary>
    internal class LessCompiler : NodeCompilerBase
    {
        public LessCompiler()
            : base("/c .\\bin\\lessc.cmd \"{0}\" \"{1}\"")
        {
        }
    }

}


