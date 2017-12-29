namespace MvcForum.Web.Application.ActionFilterAttributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Core.Interfaces.Services;

    [AttributeUsage(AttributeTargets.Property)]
    public class MustBeTrueAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly ILocalizationService _localizationService;

        public MustBeTrueAttribute()
        {
            _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
            ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ErrorMessage = _localizationService.GetResourceString(ErrorMessage.Trim()),
                ValidationType = "mustbetrue"
            };
        }

        public override bool IsValid(object value)
        {
            return value != null && value is bool && (bool) value;
        }
    }
}