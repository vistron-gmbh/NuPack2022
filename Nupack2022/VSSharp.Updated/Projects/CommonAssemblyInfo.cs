using System;

namespace CnSharp.VisualStudio.Extensions.Projects
{
    [Serializable]
    public class CommonAssemblyInfo : IComparable<CommonAssemblyInfo>
    {
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Product { get; set; }
        public string Company { get; set; }
        public string Trademark { get; set; }

        #region Implementation of IComparable<in CommonAssemblyInfo>

        public int CompareTo(CommonAssemblyInfo other)
        {
            if (String.CompareOrdinal(Version, other.Version) != 0)
                return -1;
            if (String.CompareOrdinal(Copyright, other.Copyright) != 0)
                return -1;
            if (String.CompareOrdinal(Product, other.Product) != 0)
                return -1;
            if (String.CompareOrdinal(Company, other.Company) != 0)
                return -1;
            if (String.CompareOrdinal(Trademark, other.Trademark) != 0)
                return -1;
            return 0;
        }

        #endregion
    }
}