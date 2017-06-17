using System;
using System.Text;

namespace BandoriScoreVisualizer
{
    public static class Utilities
    {
        public static string GetDetails(this Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
