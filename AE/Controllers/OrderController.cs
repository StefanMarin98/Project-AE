using AE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AE.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult ShowOrders()
        {
            List<Product> products = new List<Product>();
            List<Cart_products_jnl> cpj = new List<Cart_products_jnl>();
            List<Carts_jnl> carts_jnl = new List<Carts_jnl>();
            string query = "SELECT cj.*, p.*, cpj.cart_id as cart_id_j, cpj.product_id as product_id_j, cpj.cart_product_id FROM Carts_jnl cj join Cart_products_jnl cpj on cpj.cart_id=cj.cart_id join products p on p.product_id=cpj.product_id";
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand com = new SqlCommand(query, con);
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            bool containsItem = products.Any(item => item.Product_id == Convert.ToInt32(sdr["Product_id"]));
                            if (containsItem == false)
                            {
                                products.Add(new Product
                                {
                                    Product_id = Convert.ToInt32(sdr["Product_id"]),
                                    Name = sdr["Name"].ToString(),
                                    Model = sdr["Model"].ToString(),
                                    Brand = sdr["Brand"].ToString(),
                                    Price = Convert.ToDouble(sdr["Price"]),
                                    Other_properties = sdr["Other_properties"].ToString(),
                                    Photo = (byte[])sdr["Photo"]
                                });
                            }
                            containsItem = carts_jnl.Any(item => item.Cart_id == Convert.ToInt32(sdr["Cart_id"]));
                            if (containsItem == false)
                            {
                                carts_jnl.Add(new Carts_jnl
                                {
                                    Cart_id = Convert.ToInt32(sdr["Cart_id"]),
                                    Invoice_number = sdr["Invoice_number"].ToString(),
                                    Date = Convert.ToDateTime(sdr["Date"]),
                                    Total_price = Convert.ToDouble(sdr["Total_price"])
                                });
                            }
                            containsItem = cpj.Any(item => item.Cart_product_id == Convert.ToInt32(sdr["Cart_product_id"]));
                            if (containsItem == false)
                            {
                                cpj.Add(new Cart_products_jnl
                                {
                                    Cart_product_id = Convert.ToInt32(sdr["Cart_product_id"]),
                                    Product_id = Convert.ToInt32(sdr["Product_id"]),
                                    Cart_id = Convert.ToInt32(sdr["Cart_id"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            HttpContext.Session["Products"] = products;
            HttpContext.Session["Orders"] = carts_jnl;
            HttpContext.Session["Cpj"] = cpj;
            return View();
        }

        public ActionResult AddOrder(int Cart_product_id)
        {
            List<Cart> carts = new List<Cart>();
            List<Cart_products> cart_products = new List<Cart_products>();
            cart_products = GetCartProducts(Cart_product_id);
            carts = GetCarts(Cart_product_id);
            int new_cart_id = 0;
            InsertCartsJnl(carts, ref new_cart_id);
            InsertCartPrductsJnl(cart_products, new_cart_id);
            DeleteCurrentCartProducts(cart_products[0].Cart_id);
            DeleteCurrentCarts(carts[0].Cart_id);
            return RedirectToAction("ShowOrders");
        }

        [NonAction]
        private List<Cart_products> GetCartProducts(int Cart_product_id)
        {
            string query = "SELECT c.cart_id FROM carts c left JOIN cart_products cp on cp.cart_id=c.cart_id WHERE cp.cart_product_id=" + Cart_product_id;
            int cart_id;
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                cart_id = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
            }
            query = "SELECT * FROM Cart_products cp where cp.cart_id=" + cart_id;
            List<Cart_products> cart_products = new List<Cart_products>();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            cart_products.Add(new Cart_products
                            {
                                Product_id = Convert.ToInt32(sdr["Product_id"]),
                                Cart_id = Convert.ToInt32(sdr["Cart_id"])
                            });
                        }
                        con.Close();
                    }
                    return cart_products;
                }
            }
        }

        [NonAction]
        private void InsertCartPrductsJnl(List<Cart_products> cart_products, int cart_new_id)
        {
            var db = new Database1Entities();
            foreach (Cart_products cp in cart_products)
            {
                Cart_products_jnl cpj = new Cart_products_jnl();
                cpj.Cart_id = cart_new_id;
                cpj.Product_id = cp.Product_id;
                db.Cart_products_jnl.Add(cpj);
                db.SaveChanges();
            }
        }

        [NonAction]
        private void DeleteCurrentCartProducts(int cart_id)
        {
            string query = "DELETE FROM Cart_Products WHERE cart_id=" + cart_id;
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            int i = 0;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand com = new SqlCommand(query, con);
                con.Open();
                i = com.ExecuteNonQuery();
                con.Close();
            }
        }

        [NonAction]
        private List<Cart> GetCarts(int cart_product_id)
        {
            string query = "SELECT c.cart_id FROM carts c left JOIN cart_products cp on cp.cart_id=c.cart_id WHERE cp.cart_product_id=" + cart_product_id;
            int cart_id;
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                cart_id = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
            }
            var db = new Database1Entities();
            var c = db.Set<Cart>().SingleOrDefault(o => o.Cart_id == cart_id);
            c.Date = System.DateTime.Now;
            c.Invoice_number = c.Cart_id + "-" + DateTime.Now.DayOfWeek + DateTime.Now.Month + DateTime.Now.Year;
            db.SaveChanges();
            List<Cart> carts = new List<Cart>();
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            query = "SELECT * FROM carts where cart_id=" + cart_id;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            carts.Add(new Cart
                            {
                                Cart_id = Convert.ToInt32(sdr["Cart_id"]),
                                Invoice_number = sdr["Invoice_number"].ToString(),
                                Date = Convert.ToDateTime(sdr["Date"]),
                                Total_price = Convert.ToDouble(sdr["Total_price"])
                            });
                        }
                        con.Close();
                    }
                    return carts;
                }
            }
        }

        [NonAction]
        private void InsertCartsJnl(List<Cart> carts, ref int cart_jnl)
        {
            var db = new Database1Entities();
            string invoice_number = "";
            foreach (Cart c in carts)
            {
                Carts_jnl cj = new Carts_jnl();
                cj.Date = c.Date;
                cj.Total_price = c.Total_price;
                cj.Invoice_number = c.Invoice_number;
                db.Carts_jnl.Add(cj);
                db.SaveChanges();
                invoice_number = c.Invoice_number;
            }
            string query = "Select cart_id FROM Carts_jnl WHERE Invoice_number='" + invoice_number + "'";
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand com = new SqlCommand(query, con);
                con.Open();
                cart_jnl = (int)com.ExecuteScalar();
                con.Close();
            }
        }

        [NonAction]
        private void DeleteCurrentCarts(int Cart_id)
        {
            string query = "DELETE FROM Carts WHERE cart_id=" + Cart_id;
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            int i = 0;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand com = new SqlCommand(query, con);
                con.Open();
                i = com.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}