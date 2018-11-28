using AutoMapper;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models.SharedViewModels;
using GdevApps.Portal.Models.TeacherViewModels;
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

              config.CreateMap<GdevApps.BLL.Models.GDevSpreadSheetService.StudentReport, ReportsViewModel>();
              config.CreateMap<GdevApps.BLL.Models.GDevSpreadSheetService.GradebookReportSubmission, GdevApps.Portal.Data.StudentSubmission>()
              .ForMember(s => s.Percent, d=> d.MapFrom(o => o.Percent*100));
              config.CreateMap<GdevApps.BLL.Models.GDevSpreadSheetService.GradebookSettings, ReportSettings>();

              config.CreateMap<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent, BLL.Models.GDevSpreadSheetService.GradebookParent>()
              .ForMember(d => d.Email, s => s.MapFrom(o => o.Email))
              .ForMember(d => d.Name, s => s.MapFrom(o => o.Name))
              .ForMember(d => d.HasAccount, x => x.Ignore());

              config.CreateMap<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher, GdevApps.BLL.Models.AspNetUsers.Teacher>();
              config.CreateMap<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook, GdevApps.BLL.Models.GDevSpreadSheetService.ParentSpreadsheet>()
              .ForMember(d => d.MainGradeBookName, s => s.MapFrom(o => o.MainGradeBook.Name))
              .ForMember(d => d.MainGradeBookLink, s => s.MapFrom(o => o.MainGradeBook.Link));

            // when mapping from DAL.Gradebook to BLL.Gradebook the ClassRoom name transforms into ClassRoomId
            // when mapping from DAL.Gradebook to BLL.Gradebook MainGradeBookName, MainGradeBookLink and MainGradeBookId are ignored
              config.CreateMap<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook, GdevApps.BLL.Models.GDevSpreadSheetService.ParentSpreadsheet>()
              .ForMember(d => d.MainGradeBookName, s => s.Ignore())
              .ForMember(d => d.MainGradeBookId, s => s.Ignore())
              .ForMember(d => d.MainGradeBookLink, s => s.Ignore())
              .ForMember(d => d.ClassroomName, s => s.MapFrom(o => o.ClassroomId));

              config.CreateMap<GdevApps.BLL.Models.AspNetUsers.PortalRole, GdevApps.Portal.Models.ManageViewModels.PortalRoleViewModel>();
              config.CreateMap<GdevApps.BLL.Models.AspNetUsers.PortalUser, GdevApps.Portal.Models.ManageViewModels.PortalUserViewModel>();
              
          });
    }
}
