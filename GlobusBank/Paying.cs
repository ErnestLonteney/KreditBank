namespace GlobusBank
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Paying")]
    public partial class Paying
    {
        [Key]
        [Column(Order = 0, TypeName = "date")]
        public DateTime DatePay { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal SumPay { get; set; }

        [Key]
        [Column(Order = 1)]
        public bool TypePay { get; set; }
    }
}
