using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GdevApps.Portal.Migrations
{
    public partial class portal_migrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {       
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "ParentSharedGradeBook");

            migrationBuilder.DropTable(
                name: "ParentStudent");

            migrationBuilder.DropTable(
                name: "Teacher");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "Folder");

            migrationBuilder.DropTable(
                name: "ParentGradeBook");

            migrationBuilder.DropTable(
                name: "SharedStatus");

            migrationBuilder.DropTable(
                name: "Parent");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "gradebook_license");

            migrationBuilder.DropTable(
                name: "FolderType");

            migrationBuilder.DropTable(
                name: "GradeBook");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "gradebook_license");
        }
    }
}
