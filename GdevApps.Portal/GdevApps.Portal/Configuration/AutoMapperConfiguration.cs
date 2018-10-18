using AutoMapper;
using GdevApps.Portal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Configuration
{
    internal static class AutoMapperConfiguration
    {
        public static MapperConfiguration MapperConfiguration
            => new MapperConfiguration(config =>
          {
              config.CreateMap<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens, BLL.Models.AspNetUsers.AspNetUserToken>();
              config.CreateMap<BLL.Models.GDevSpreadSheetService.GradebookStudent, BLL.Models.GDevClassroomService.GoogleStudent>();
              config.CreateMap<GdevApps.BLL.Models.GDevClassroomService.GoogleClassSheet, ClassSheetsViewModel>();
              config.CreateMap<GdevApps.BLL.Models.GDevClassroomService.GradeBook, GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>()
              .ForMember(opt => opt.CreatedByNavigation, x => x.Ignore())
              .ForMember(opt => opt.ParentGradeBook, x => x.Ignore());

              config.CreateMap<GdevApps.BLL.Models.AspNetUsers.Parent, GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>();
              config.CreateMap<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder, GdevApps.BLL.Models.GDevDriveService.Folder>();
              config.CreateMap<ClassSheetsViewModel, GdevApps.BLL.Models.GDevClassroomService.GradeBook>();
          });
    }
}
