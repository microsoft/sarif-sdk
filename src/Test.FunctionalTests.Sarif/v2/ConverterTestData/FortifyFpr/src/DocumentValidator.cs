using System;

namespace FortifyFprConverter
{
    public class DocumentValidator
    {
        public DocumentValidator() { }

        /// <summary>
        /// Validate the uploaded document against Open XML regulations
        /// </summary>
        private void IsDocumentValid()
        {
            OpenXmlValidator validator = new OpenXmlValidator();
            var errors = validator.Validate(_wordDocument);
            if (errors.Count() != 0)
                _validations.Add(new Validation(GetResource(ResourceKeys.ERROR_DocumentInvalidOrCurrupted)));
        }

    }
}