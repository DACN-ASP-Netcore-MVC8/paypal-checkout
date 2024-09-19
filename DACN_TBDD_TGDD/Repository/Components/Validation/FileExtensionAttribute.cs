using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DACN_TBDD_TGDD.Repository.Components.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;

        public FileExtensionAttribute(params string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant().TrimStart('.');
                if (_allowedExtensions.Contains(fileExtension))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"Invalid file type. Allowed types are: {string.Join(", ", _allowedExtensions)}.");
                }
            }

            return new ValidationResult("No file uploaded.");
        }
    }
}
