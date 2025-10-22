using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace EmployeeCrudApi.Models
{
    public class Product
    {
        // Usaremos este Id como clave primaria en Mongo (_id)
        [BsonId]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        // Stock between 0 and 100
        [Range(0, 100)]
        public int Stock { get; set; } = 0;
    }
}
