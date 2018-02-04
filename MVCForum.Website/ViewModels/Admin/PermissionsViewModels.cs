namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Core.Models.Entities;
    using Core.Models.General;

    public class ChoosePermissionsViewModel
    {
        public List<MembershipRole> MembershipRoles { get; set; }
        public List<Permission> Permissions { get; set; }
    }

    public class EditPermissionsViewModel
    {
        public MembershipRole MembershipRole { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<Category> Categories { get; set; }
        public PermissionSet CurrentGlobalPermissions { get; set; }
    }

    public class AddTypeViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [DisplayName("Permission Type Name")]
        [StringLength(200)]
        public string Name { get; set; }

        [DisplayName("Is Global Permission")]
        public bool IsGlobal { get; set; }
    }

    public class EditTypeViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [DisplayName("Permission Type Name")]
        [StringLength(200)]
        public string Name { get; set; }
    }

    public class AjaxEditPermissionViewModel
    {
        [Required]
        public bool HasPermission { get; set; }

        [Required]
        public Guid Permission { get; set; }

        [Required]
        public Guid MembershipRole { get; set; }

        [Required]
        public Guid Category { get; set; }
    }
}