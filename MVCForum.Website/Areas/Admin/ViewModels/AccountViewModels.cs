using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    #region Users Points

    public class ManageUsersPointsViewModel
    {
        public MembershipUser User { get; set; }
        public List<MembershipUserPoints> AllPoints { get; set; }

        [Display(Name = "Amount of points to give this user")]
        public int? Amount { get; set; }

        [Display(Name = "Notes about this point allocation")]
        [MaxLength(400)]
        public string Note { get; set; } 

        public Guid Id { get; set; }
    }

    #endregion

    #region Users

    public class SingleMemberListViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "User Name")]        
        public string UserName { get; set; }

        [Display(Name = "Job Title")]
        public String JobTitle { get; set; }

        [Display(Name = "City")]
        public String City { get; set; }

        [Display(Name = "Country")]
        public String Country { get; set; }

        [Display(Name = "Email")]
        public String Email { get; set; }

        [Display(Name = "Firm")]
        public MembershipFirm MembershipFirm { get; set; }
        public Guid MembershipFirmId { get; set; }

        [Display(Name = "Voting Member")]
        public bool IsVotingMember { get; set; }

        [Display(Name = "Firm")]
        public List<SelectListItem> MemberFirms { get; set; }

        [Display(Name = "Firm Name")]
        public String MembershipFirmName { get; set; }

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "User is Disabled")]
        public bool IsBanned { get; set; }

        [Display(Name = "Roles")]
        public string[] Roles { get; set; }
    }

    public class SingleRegisteredMemberListViewModel
    {
        [Required]
        public Guid RegId { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Job Title")]
        public String JobTitle { get; set; }

        [Display(Name = "City")]
        public String City { get; set; }

        [Display(Name = "Country")]
        public String Country { get; set; }

        [Display(Name = "Email")]
        public String Email { get; set; }

        [Display(Name = "Firm")]
        public MembershipFirm MembershipFirm { get; set; }
        public Guid MembershipFirmId { get; set; }

        [Display(Name = "Voting Member")]
        public bool IsVotingMember { get; set; }

        [Display(Name = "Firm")]
        public List<SelectListItem> MemberFirms { get; set; }

        [Display(Name = "Firm Name")]
        public String MembershipFirmName { get; set; }

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "User is Disabled")]
        public bool IsBanned { get; set; }

        [Display(Name = "Roles")]
        public string[] Roles { get; set; }
    }

    public class RegisteredMemberListViewModel
    {
        [Required]
        [Display(Name = "Registered User Name")]
        public string RegUserName { get; set; }

        [Display(Name = "Registered User Job Title")]
        public String RegJobTitle { get; set; }

        [Display(Name = "Registered User Voting Member")]
        public bool RegIsVotingMember { get; set; }

        [Display(Name = "Firm")]
        public String RegMembershipFirmName { get; set; }


        [Display(Name = "Firm Size Banding")]
        public String RegMembershipFirmSizeBanding { get; set; }

        [Display(Name = "Firm Is Vendor")]
        public bool RegMembershipFirmVendor { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "User Job Title")]
        public String JobTitle { get; set; }

        [Display(Name = "Email")]
        public String Email { get; set; }

        [Display(Name = "Voting Member")]
        public bool IsVotingMember { get; set; }

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
        public int TotalPages { get; set; }
        public string Search { get; set; }
        public bool ShowDisabled { get; set; }
    }

    public class UserPointChartItem
    {
        public MembershipUserPoints MembershipUserPoints { get; set; }
        public Post Post { get; set; }
        public Vote Vote { get; set; }
        public Domain.DomainModel.Badge Badge { get; set; }
        public TopicTag TopicTag { get; set; }

    }

    public class MemberEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        [StringLength(150)]
        public string UserName { get; set; }

        [Display(Name = "Job Title")]
        [StringLength(250)]
        public string JobTitle { get; set; }

        [Display(Name = "Firm")]
        public string MembershipFirm { get; set; }
        public Guid MembershipFirmId { get; set; }

        [Display(Name = "User is Voting Member")]
        public bool IsVotingMember { get; set; }

        [Display(Name = "Users Uploaded Avatar")]
        public string Avatar { get; set; }

        [Display(Name = "Password Question")]
        public string PasswordQuestion { get; set; }

        [Display(Name = "Password Answer")]
        public string PasswordAnswer { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Signature")]
        [StringLength(1000)]
        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Signature { get; set; }

        [Display(Name = "Age")]
        [Range(0, int.MaxValue)]
        public int? Age { get; set; }

        [Display(Name = "Location")]
        [StringLength(100)]
        public string Location { get; set; }

        [Display(Name = "City")]
        public String City { get; set; }

        [Display(Name = "Country")]
        public String Country { get; set; }
        [Display(Name = "Website")]
        [System.ComponentModel.DataAnnotations.Url]
        [StringLength(100)]
        public string Website { get; set; }

        [Display(Name = "Twitter Url")]
        [System.ComponentModel.DataAnnotations.Url]
        [StringLength(60)]
        public string Twitter { get; set; }

        [Display(Name = "Facebook Page")]
        [System.ComponentModel.DataAnnotations.Url]
        [StringLength(60)]
        public string Facebook { get; set; }

        [Display(Name = "User is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Disable email notifications for this member")]
        public bool DisableEmailNotifications { get; set; }
        
        [Display(Name = "Disable posting. The user will not be able to post or create topics")]
        public bool DisablePosting { get; set; }
        
        [Display(Name = "Disable private messages for this user")]
        public bool DisablePrivateMessages { get; set; }

        [Display(Name = "Disable file uploading on posts and topics for this user")]
        public bool DisableFileUploads { get; set; }

        [Display(Name = "User is Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "User is Disabled")]
        public bool IsBanned { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Roles")]
        public string[] Roles { get; set; }

        public IList<MembershipRole> AllRoles { get; set; }

        [Display(Name = "Firm")]
        public IList<MembershipFirm> AllFirms { get; set; }
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
    #region Firms
    public class FirmListViewModel
    {
        public IList<MembershipFirm> MembershipFirms { get; set; }
    }
    public class FirmViewModel
    {
        [Required]
        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Firm Name")]
        [StringLength(256)]
        public string FirmName { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(256)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(256)]
        public string Address2 { get; set; }

        [Display(Name = "Address Line 3")]
        [StringLength(256)]
        public string Address3 { get; set; }

        [Display(Name = "City")]
        [StringLength(256)]
        public string City { get; set; }

        [Display(Name = "State/County")]
        [StringLength(256)]
        public string County { get; set; }

        [Display(Name = "Country")]
        [StringLength(256)]
        public string Country { get; set; }

        [Display(Name = "ZIP/Postcode")]
        [StringLength(256)]
        public string Postcode { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Size Banding")]
        public string SizeBanding { get; set; }

        [Display(Name = "US")]
        public bool US { get; set; }

        [Display(Name = "Canada")]
        public bool Canada { get; set; }

        [Display(Name = "UK")]
        public bool UK { get; set; }

        [Display(Name = "EMEA")]
        public bool EMEA { get; set; }

        [Display(Name = "APAC")]
        public bool APAC { get; set; }

        [Display(Name = "Other")]
        public bool Other { get; set; }

        [Display(Name = "Professional Services?")]
        public bool ProfessionalServices { get; set; }

        [Display(Name = "Vendor?")]
        public bool Vendor { get; set; }

        [Display(Name = "Joined")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Last Modified")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime LastModified { get; set; }

        [Display(Name = "Member Info Check")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> MemberInfoCheck { get; set; }

        [Display(Name = "Size Check")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> SizeCheck { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Website")]
        [System.ComponentModel.DataAnnotations.Url]
        [StringLength(100)]
        public string Website { get; set; }
        public IEnumerable<SelectListItem> SizeBandings { get; set; }
    }

    #endregion

}
