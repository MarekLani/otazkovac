using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using CQA.Models;
using System.Linq;

namespace Validations
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class PlaceExistentionValidation : ValidationAttribute, IClientValidatable
    {
        private CQADBContext db = new CQADBContext();


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
           
                
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[] { new ModelClientValidationSelectOneRule(FormatErrorMessage(metadata.DisplayName))};
        }

        public class ModelClientValidationSelectOneRule : ModelClientValidationRule
        {
            public ModelClientValidationSelectOneRule(string errorMessage)
            {
                ErrorMessage = errorMessage;
                ValidationType = "placeexistentionvalidation";
            }
        }
    }
}