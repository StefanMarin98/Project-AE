//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AE.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Cart_products_jnl
    {
        public int Cart_product_id { get; set; }
        public int Product_id { get; set; }
        public int Cart_id { get; set; }
    
        public virtual Carts_jnl Carts_jnl { get; set; }
        public virtual Product Product { get; set; }
    }
}
