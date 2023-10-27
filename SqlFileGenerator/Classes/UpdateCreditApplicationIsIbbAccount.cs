using FluentMigrator;

namespace SqlFileGenerator.Classes
{
    [Migration(02302930290)]
    public class UpdateCreditApplicationIsIbbAccount : Migration
    {
        public override void Down()
        {
            Execute.Sql("update stage.credit_application set is_ibb_account = 'false' where created_date < '2022/11/10'and home_branch = 2500");
        }

        public override void Up()
        {
            Execute.Sql("update stage.credit_application set is_ibb_account = 'true' where created_date < '2022/11/10'and home_branch = 2500");
        }
    }
}
