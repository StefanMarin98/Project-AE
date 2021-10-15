using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AE.Models.Extended
{
    public partial class Product
    {
        [DisplayName("Product name")]
        [Required(ErrorMessage = "Product name is mandatory")]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [DisplayName("Product model")]
        [Required(ErrorMessage = "Product model is mandatory")]
        [StringLength(50, MinimumLength = 3)]
        public string Model { get; set; }

        [DisplayName("Product brand")]
        [Required(ErrorMessage = "Product brand is mandatory")]
        [StringLength(50, MinimumLength = 3)]
        public string Brand { get; set; }

        [DisplayName("Product price")]
        [Required(ErrorMessage = "Product price is mandatory")]
        [Range(0, 100000, ErrorMessage = "Price must be positive")]
        public double Price { get; set; }

        [DisplayName("Other properties")]
        [Required(ErrorMessage = "Properties are mandatory")]
        [StringLength(50, MinimumLength = 3)]
        public string Other_properties { get; set; }

        [DisplayName("Product photo")]
        [DataType(DataType.Upload)]
        public byte[] Photo { get; set; }
    }
}