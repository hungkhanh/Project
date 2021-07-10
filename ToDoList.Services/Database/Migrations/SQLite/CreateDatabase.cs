using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;


namespace ToDoList.Services.Database.Migrations.SQLite
{
    public class CreateDatabase : Migration
    {
        protected override void Up( MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "DueDate",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinishDate",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeDue",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTime",
                table: "tblTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartDate",
                table: "tblTasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
               name: "DueDate",
               table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "TimeDue",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "FinishDate",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "ReminderTime",
                table: "tblTasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "tblTasks");
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("ToDoList.Model.BoardIFM", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Name");

                b.Property<string>("Notes");

                b.HasKey("Id");

                b.ToTable("tblBoards");
            });

            modelBuilder.Entity("ToDoList.Model.TaskIFM", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<int>("BoardId");

                b.Property<string>("Category");

                b.Property<string>("ColorKey");

                b.Property<int>("ColumnIndex");

                b.Property<string>("DateCreated");

                b.Property<string>("Description");

                b.Property<string>("DueDate");

                b.Property<string>("FinishDate");

                b.Property<string>("ReminderTime");

                b.Property<string>("StartDate");

                b.Property<string>("Tags");

                b.Property<string>("TimeDue");

                b.Property<string>("Title");

                b.HasKey("Id");

                b.HasIndex("BoardId");

                b.ToTable("tblTasks");
            });

            modelBuilder.Entity("ToDoList.Model.TaskIFM", b =>
            {
                b.HasOne("ToDoList.Model.BoardIFM", "Board")
                    .WithMany("Tasks")
                    .HasForeignKey("BoardId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
#pragma warning restore 612, 618
        }
    }
}
