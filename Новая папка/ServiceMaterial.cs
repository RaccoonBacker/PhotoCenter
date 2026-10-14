using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("ServiceMaterials")]
    public class ServiceMaterial
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ServiceID")]
        public int ServiceID { get; set; }

        [Column("MaterialID")]
        public int MaterialID { get; set; }

        [Column("Quantity")]
        public decimal Quantity { get; set; }

        // Навигационные свойства
        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }

        [ForeignKey("MaterialID")]
        public virtual Material Material { get; set; }
    }
}