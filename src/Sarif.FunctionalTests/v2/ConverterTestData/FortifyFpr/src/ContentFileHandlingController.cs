using System;

namespace FortifyFprConverter
{
    public class ContentFileHandlingController
    {
        public ContentFileHandlingController() { }

        public CMOEViewModel Model { get; set; }

        [Dependency]
        public ContentFileViewModel ViewModel { get; set; }

        [Dependency]
        public ICachedDataProvider CachedData { get; set; }

        /// <param name="frmContent"> Download form input parameters </param>
        /// <returns>Action Result object</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Download(ContentFileViewModel frmContent)
        {
            try
            {
                this.ViewModel = frmContent;

                if (ViewModel.DocumentId == Guid.Empty)
                {
                }
            }
            catch { }
        }

        public void SomeMethod()
        {
            ViewModel.TemplatePath = (ViewModel.DocumentType == Const.DoucmentTypeKeyResults) ? Server.MapPath(Request.ApplicationPath) + Const.KeyResultsDocumentTemplatePath :
                                          Server.MapPath(Request.ApplicationPath) + Const.CompetencyDocumentTemplatePath;

            byte[] byteFile = System.IO.File.ReadAllBytes(ViewModel.TemplatePath);

            // Write the byte array in a memory stream
            System.IO.MemoryStream streamPackage = new System.IO.MemoryStream();
        }
    }
}