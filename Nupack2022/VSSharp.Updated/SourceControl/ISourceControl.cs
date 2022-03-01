using System.Collections.Generic;

namespace CnSharp.VisualStudio.Extensions.SourceControl
{
    public interface ISourceControl
    {
        /// <summary>
        /// check out file
        /// </summary>
        /// <param name="slnDir"></param>
        /// <param name="file"></param>
        /// <returns>-1 no version control; 0 check out failed ; >0 check out success</returns>
        int CheckOut(string slnDir, string file);

        /// <summary>
        /// get pending files
        /// </summary>
        /// <param name="projectDir"></param>
        /// <returns></returns>
        IEnumerable<string> GetPendingFiles(string projectDir);
    }
}