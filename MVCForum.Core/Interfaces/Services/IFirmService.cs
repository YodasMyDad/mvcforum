using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IFirmService
    {
        IList<MembershipFirm> AllFirms();
        //////void Save(MembershipRole user);
        void Delete(MembershipFirm firm);
        MembershipFirm GetFirm(string firmname);
        //IList<MembershipFirm> GetUsersForFrim(string firmName);
        MembershipFirm Get(Guid id);
        void CreateFirm(MembershipFirm firm);
        MembershipFirm GetFirmBySlug(string slug);
        MembershipFirm GetFirm(Guid Id);
    }
}
