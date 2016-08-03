using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class AnnualMeetingRegistrationMapping : EntityTypeConfiguration<AnnualMeetingRegistration>
    {
        public AnnualMeetingRegistrationMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.RegistrationDate).IsRequired();
            Property(x => x.EventDate).IsRequired();
            Property(x => x.EMail).IsRequired().HasMaxLength(200);
            Property(x => x.FirstName).IsRequired();
            Property(x => x.Surname).IsRequired();
            Property(x => x.FirmName).IsRequired();

        }
    }
}
