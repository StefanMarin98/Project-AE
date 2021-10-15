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
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult ShowCart()
        {

            string query = "SELECT ISNULL(MAX(cart_id),0) FROM Carts";
            int exists=0;
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
                exists = (int)com.ExecuteScalar();
                con.Close();
            }
            if (exists == 0)
            {
                var db = new Database1Entities();
                Cart cart = new Cart();
                cart.Invoice_number = " ";
                cart.Total_price = 0;
                cart.Date = System.DateTime.Now;
                db.Carts.Add(cart);
                db.SaveChanges();

                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand com = new SqlCommand(query, con);
                    con.Open();
                    exists = (int)com.ExecuteScalar();
                    con.Close();
                }
            }
            query = "SELECT total_price FROM Carts where cart_id=" + exists ;
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand com = new SqlCommand(query, con);
                con.Open();
                ViewBag.Total_price = Convert.ToDouble( com.ExecuteScalar());
                con.Close();
            }
            List<Product> products = new List<Product>();
            products = GetCartProducts(exists);
            return View(products);
        }

        [NonAction]
        private List<Product> GetCartProducts(int id)
        {
            string query = "SELECT p.*, cp.cart_product_id FROM Cart_products cp JOIN Products p on p.product_id=cp.product_id WHERE cp.cart_id=" + id;
            List<Product> products = new List<Product>();
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
            int[] ids = new int[100];
            int i = 0;
            if (constr.ToLower().StartsWith("metadata="))
            {
                System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(constr);
                constr = efBuilder.ProviderConnectionString;
            }
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
                            ids[i] = Convert.ToInt32(sdr["Cart_product_id"]);
                            i++;
                        }
                    }
                    con.Close();
                }
                ViewBag.Cart_product_id = ids;
                return products;
            }
        }

        public ActionResult RemoveFromCart(int Cart_product_id)
        {
            string query = "DELETE FROM Cart_Products WHERE cart_product_id=" + Cart_product_id;
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
            query = "SELECT isnull(sum(p.price),0) FROM products p JOIN cart_products cp on cp.product_id=p.product_id";
            float sum = 0;
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                sum = (float)Convert.ToDouble(cmd.ExecuteScalar());
                con.Close();
            }
            query = "SELECT distinct(c.cart_id) FROM carts c left JOIN cart_products cp on cp.cart_id=c.cart_id";
            int cart_id;
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                cart_id = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
            }
            var db = new Database1Entities();
            var c = db.Set<Cart>().SingleOrDefault(o => o.Cart_id == cart_id);
            c.Total_price = sum;
            db.SaveChanges();


            return RedirectToAction("ShowCart");
        }
    }
}