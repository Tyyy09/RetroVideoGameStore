using RetroVideoGameStore.Models;
using System.ComponentModel.DataAnnotations;

public class OrderDetail
{
    public int OrderDetailId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public double Price { get; set; }

    // FK
    [Required]
    public int OrderId { get; set; }

    // FK
    [Required]
    public int ProductId { get; set; }

    // parent refs
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}