using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{

    #region Users

    public class SingleMemberListViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "User Name")]        
        public string UserName { get; set; }

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Roles")]
        public string[] Roles { get; set; }
    }

    public class MemberListViewModel
    {
        [Required]
        [Display(Name = "Users")]
        public IList<SingleMemberListViewModel> Users { get; set; }

        [Required]
        [Display(Name = "Roles")]
        public IList<MembershipRole> AllRoles { get; set; }

        public Guid Id { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }

    }

    public class MemberEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        [StringLength(50)]
        public string UserName { get; set; }

        [Display(Name = "Password Question")]
        public string PasswordQuestion { get; set; }

        [Display(Name = "Password Answer")]
        public string PasswordAnswer { get; set; }

        [Display(Name = "Email Address")]
        [Email]
        public string Email { get; set; }

        [Display(Name = "Signature")]
        [StringLength(1000)]
        //[UIHint("bbeditor"), AllowHtml]
        [UIHint("markdowneditor"), AllowHtml]
        public string Signature { get; set; }

        [Display(Name = "Age")]
        [Numeric]
        public int? Age { get; set; }

        [Display(Name = "Location")]
        [StringLength(100)]
        public string Location { get; set; }

        [Display(Name = "Website")]
        [Url]
        [StringLength(100)]
        public string Website { get; set; }

        [Display(Name = "Twitter Url")]
        [Url]
        [StringLength(60)]
        public string Twitter { get; set; }

        [Display(Name = "Facebook Page")]
        [Url]
        [StringLength(60)]
        public string Facebook { get; set; }

        [Display(Name = "User is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "User is Locked Out")]
        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public int FailedPasswordAnswerAttempt { get; set; }


        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Roles")]
        public string[] Roles { get; set; }

        public IList<MembershipRole> AllRoles { get; set; }

    }

    #endregion

    #region Roles

    public class AjaxRoleUpdateViewModel
    {
        public Guid Id { get; set; }
        public string[] Roles { get; set; }
    }

    public class RoleListViewModel
    {
        public IList<MembershipRole> MembershipRoles { get; set; }
    }

    public class RoleViewModel
    {
        [Required]
        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

    }


    #endregion

}
