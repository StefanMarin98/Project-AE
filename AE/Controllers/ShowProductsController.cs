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
    public class ShowProductsController : Controller
    {
        // GET: ShowProducts
        public ActionResult ShowProducts()
        {
            List<Product> products;
            if (TempData.ContainsKey("ProductsCategory"))
                products = (List<Product>)TempData["ProductsCategory"];
            else products = GetProducts();
            return View(products);
        }
        [NonAction]
        private List<Product> GetProducts()
        {
            string query = "SELECT * FROM Products";
            List<Product> products = new List<Product>();
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
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
                        }
                    }
                    con.Close();
                }

                return products;
            }
        }

        public ActionResult FilterCategory(string category)
        {
            List<Product> products = GetProductsByCategory(category);
            TempData["ProductsCategory"] = products;
            return RedirectToAction("ShowProducts");
        }

        [NonAction]
        private List<Product> GetProductsByCategory(string category)
        {
            string query = "SELECT * FROM Products WHERE Name='"+category+"'";
            List<Product> products = new List<Product>();
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
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
                        }
                    }
                    con.Close();
                }

                return products;
            }
        }

        public ActionResult AddProductToCart(int product_id, int price)
        {
            string query = "SELECT ISNULL(MAX(cart_id),0) FROM Carts";
            int exists = 0;
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

            var db = new Database1Entities();
            if (exists == 0)
            {
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
            Cart_products cp = new Cart_products();
            cp.Cart_id = exists;
            cp.Product_id = product_id;
            db.Cart_products.Add(cp);
            db.SaveChanges();
            query = "SELECT isnull(sum(p.price),0) FROM products p JOIN cart_products cp on cp.product_id=p.product_id";
            float sum = 0;
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                sum = (float)Convert.ToDouble(cmd.ExecuteScalar());
                con.Close();
            }
            query = "SELECT distinct(cart_id) FROM carts c left JOIN cart_products cp on cp.cart_id=c.cart_id";
            var c = db.Set<Cart>().SingleOrDefault(o => o.Cart_id == exists);
            c.Total_price = sum;
            db.SaveChanges();

            return RedirectToAction("ShowProducts");
        }

        public ActionResult FilterBrand(string brand)
        {
            List<Product> products = GetProductsByBrand(brand);
            TempData["ProductsCategory"] = products;
            return RedirectToAction("ShowProducts");
        }

        [NonAction]
        private List<Product> GetProductsByBrand(string brand)
        {
            string query = "SELECT * FROM Products WHERE Brand='" + brand + "'";
            List<Product> products = new List<Product>();
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
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
                        }
                    }
                    con.Close();
                }

                return products;
            }
        }

        public ActionResult FilterPrice(int pricemin, int pricemax)
        {
            List<Product> products = GetProductsByprice(pricemin, pricemax);
            TempData["ProductsCategory"] = products;
            return RedirectToAction("ShowProducts");
        }

        [NonAction]
        private List<Product> GetProductsByprice(int pricemin, int pricemax)
        {
            string query = "SELECT * FROM Products WHERE price between " + pricemin + " and " + pricemax;
            List<Product> products = new List<Product>();
            string constr = ConfigurationManager.ConnectionStrings["Database1Entities"].ConnectionString;
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
                        }
                    }
                    con.Close();
                }

                return products;
            }
        }
    }
}