using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio;

namespace CnSharp.VisualStudio.Extensions
{
    public  static class DocumentExtensions
    {
        public static string GetSelectionText(this Document doc)
        {
            var ts = (TextSelection)doc.Selection;
            return ts?.Text;
        }

        public static string GetText(this Document doc)
        {
            var textDocument = (TextDocument)doc.Object(null);
            var startPoint = textDocument.StartPoint.CreateEditPoint();
            return startPoint.GetText(textDocument.EndPoint);
        }

        public static string GetText(this TextDocument doc)
        {
            var startPoint = doc.StartPoint.CreateEditPoint();
            return startPoint.GetText(doc.EndPoint);
        }

        public static void  ReplaceSelection(this Document doc,string text)
        {
            ((TextSelection)doc.Selection).Insert(text);
        }

        public static void ReplaceSelection(this TextDocument doc, string text)
        {
            doc.Selection.Insert(text);
        }

        public static void Insert(this Document doc, string text)
        {
            var document = (TextDocument)doc.Object(null);
            document.EndPoint.CreateEditPoint().Insert(text);
        }

        public static void Insert(this TextDocument doc, string text)
        {
            doc.EndPoint.CreateEditPoint().Insert(text);
        }

        public static void Overwrite(this Document doc, string text)
        {
            var document = (TextDocument)doc.Object(null);
            //document.ReplacePattern(@".+", text, (int) vsFindOptions.vsFindOptionsRegularExpression);
            var startPoint = document.StartPoint.CreateEditPoint();
            startPoint.Delete(document.EndPoint);
            document.Selection.Insert(text);
        }

        public static void Overwrite(this TextDocument document, string text)
        {
            //document.ReplacePattern(@".+", text, (int) vsFindOptions.vsFindOptionsRegularExpression);
            var startPoint = document.StartPoint.CreateEditPoint();
            startPoint.Delete(document.EndPoint);
            document.Selection.Insert(text);
        }

        ///// <summary>
        ///// open a new Document Window
        ///// </summary>
        ///// <param name="serviceProvider"></param>
        ///// <param name="filePath"></param>
        ///// <returns></returns>
        //public static IVsWindowFrame OpenDocumentInNewWindow(this IServiceProvider serviceProvider, string filePath)
        //{
        //    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        //        return null;

        //    IVsUIHierarchy hierarchy;
        //    uint itemId;
        //    IVsWindowFrame frame = null;
        //    if (!VsShellUtilities.IsDocumentOpen(serviceProvider, filePath,
        //            VSConstants.LOGVIEWID_Primary, out hierarchy, out itemId, out frame))
        //    {
        //        VsShellUtilities.OpenDocument(serviceProvider, filePath,
        //            VSConstants.LOGVIEWID_Primary, out hierarchy, out itemId, out frame);
        //    }

        //    frame?.Show();

        //    return frame;
        //}
    }
}
