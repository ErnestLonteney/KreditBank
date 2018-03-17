namespace GlobusBank
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PayerKredit : DbContext
    {
        public PayerKredit()
            : base("name=PayerKredit")
        {
        }

        public virtual DbSet<Paying> Payings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Paying>()
                .Property(e => e.SumPay)
                .HasPrecision(10, 4);
        }
    }
}
