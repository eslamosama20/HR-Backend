//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using SmartAndPro.ERP.Domains.Security;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SmartAndPro.ERP.Infrastructure.Data.Configuration
//{
//    public class UserConfigurations : IEntityTypeConfiguration<User>
//    {
//        public void Configure(EntityTypeBuilder<User> builder)
//        {
//            //builder.Property(p => p.Id)
//            //    .HasColumnType("Uniqueidentifier");
//            builder.Property(p => p.Email).HasMaxLength(200);
//            builder.Property(p => p.ArabicFirstName).HasMaxLength(50);
//            builder.Property(p => p.ArabicMiddleName).HasMaxLength(50);
//            builder.Property(p => p.ArabicLastName).HasMaxLength(50);
//            builder.Property(p => p.EnglishFirstName).HasMaxLength(50);
//            builder.Property(p => p.EnglishMiddleName).HasMaxLength(50);
//            builder.Property(p => p.EnglishLastName).HasMaxLength(50);
//            builder.Property(p => p.Password).HasMaxLength(200);
//            //builder.Property(u => u.DateOfBirth)
//            //.HasColumnType("datetime");
//            //builder.Property(u => u.CreatedAt)
//            //.HasColumnType("datetime");
//            //builder.Property(u => u.CreatedBy)
//            //.HasColumnType("Uniqueidentifier");

//        }
//    }
//}
