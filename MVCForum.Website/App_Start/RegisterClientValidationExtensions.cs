using DataAnnotationsExtensions.ClientValidation;
using MVCForum.Website.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(RegisterClientValidationExtensions), "Start")]
 
namespace MVCForum.Website.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}