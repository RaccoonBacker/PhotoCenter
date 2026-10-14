using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoCenterWeb.DTOs.Orders
{
    public class OrderCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ClientID { get; set; }

        [Range(1, int.MaxValue)]
        public int? EmployeeID { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }
    }
}