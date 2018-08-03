using System;

namespace MS.IT.HRE.CMOE.Web.Models
{
    public class ContentFileViewModel : BaseViewModel
    {
        #region Public Properties
        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }

        public string TemplatePath { get; set; }

        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        #endregion
    }
}