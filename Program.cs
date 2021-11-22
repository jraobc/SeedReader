using System;
using System.Data.SqlClient;
using System.IO;

namespace SeedReader
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Requires SQL database to be set up already, including a login and user 'KB_Admin' with password 'abc123'. 
             * The included createKB.sql script does this, but it is specific to my PC for now. The filepaths for the databse and logfile may require changes for you to get it to work.
             * CSV files must be stripped of headings (i.e. delete first line) before running. 
             * Also must do a find and replace of single quotes (') with 2 single quotes ('') before parsing. Note that this is not a double quote (").
             * Once seed file is prepared, change filePath variable to match it.
             */

            string filePath = "C:\\Users\\joshr\\source\\repos\\SeedReader\\TextFile1.txt";


            Console.WriteLine("Getting Connection ...");

            string datasource = "DESKTOP-O2OU20K\\SQLEXPRESS";//your server
            string database = "KnowBetterDB"; //your database name
            string username = "KB_Admin"; //username of server to connect
            string password = "abc123"; //password

            //your connection string 
            string connString = "Data Source=" + datasource + ";Initial Catalog="
                        + database + ";Persist Security Info=True;User ID=" + username + ";Password=" + password;

            //create instanace of database connection
            SqlConnection conn = new SqlConnection(connString);


            try
            {
                Console.WriteLine("Openning Connection ...");

                //open connection
                conn.Open();

                Console.WriteLine("Connection successful!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            var lines = File.ReadLines(filePath);
            string[] lineArray;
            string productName;
            string brand;
            int productId;
            int ingredientId;
            foreach (string line in lines)
            {
                lineArray = line.Split(",");
                brand = lineArray[0];
                productName = lineArray[1];

                

                //Insert into Product table and get ProductID
                productId = InsertProduct(productName, brand);

                if (productId > 0)
                {
                    for (int i = 2; i < lineArray.Length; i++)
                    {
                        //Insert into Ingredient table and get IngredientID
                        ingredientId = InsertIngredient(lineArray[i]);

                        //Insert into ProductIngredient
                        InsertProdIng(productId, ingredientId);
                    }

                }
                
            }

            Console.WriteLine("Finished");

            Console.Read();

            int InsertProduct(string productName, string brand)
            {
                int id = 0;

                string query = $"select count(ProductId) from [dbo].[Product] where ProductName = '{productName}' and Brand = '{brand}'";
                SqlCommand existsCmd = new SqlCommand(query, conn);
                int exists = Convert.ToInt32(existsCmd.ExecuteScalar());

                if (exists == 1)
                {
                    Console.WriteLine($"Product {productName} already exists in database. Aborting Insert.");

                }
                else if (exists == 0)
                {
                    query = $"INSERT INTO [dbo].[Product] (ProductName, Brand) VALUES ('{productName}', '{brand}') select SCOPE_IDENTITY() as ProductId";
                }
                else
                {
                    throw new Exception("Duplicate products in databse. Invalid state. Aborting Insert.");
                }

                SqlCommand getIdCmd = new SqlCommand(query, conn);
                id = Convert.ToInt32(getIdCmd.ExecuteScalar());

                return id;

            }

            int InsertIngredient(string ingredientName)
            {
                int id = 0;

                string query = $"select count(IngredientId) from [dbo].[Ingredient] where IngredientName = '{ingredientName}'";
                SqlCommand existsCmd = new SqlCommand(query, conn);
                int exists = Convert.ToInt32(existsCmd.ExecuteScalar());

                if (exists == 1)
                {
                    query = $"select IngredientId from [dbo].[Ingredient] where IngredientName = '{ingredientName}'";
                    
                } 
                else if (exists == 0) {
                    query = $"INSERT INTO [dbo].[Ingredient] (IngredientName) VALUES ('{ingredientName}') select SCOPE_IDENTITY() as IngredientId";
                } 
                else
                {
                    throw new Exception("Duplicate ingredients in databse. Invalid state. Aborting Insert.");
                }

                SqlCommand getIdCmd = new SqlCommand(query, conn);
                id = Convert.ToInt32(getIdCmd.ExecuteScalar());

                return id;
            }

            void InsertProdIng(int prodID, int ingID)
            {
                string query = $"select count(ProductId) from [dbo].[ProductIngredient] where IngredientId = '{ingID}' and ProductId = '{prodID}'";
                SqlCommand existsCmd = new SqlCommand(query, conn);
                int exists = Convert.ToInt32(existsCmd.ExecuteScalar());

                if (exists == 1)
                {
                    return;
                }
                else if (exists == 0)
                {
                    query = $"INSERT INTO [dbo].[ProductIngredient] (ProductId,IngredientId) VALUES ('{prodID}','{ingID}')";
                }
                else
                {
                    throw new Exception("Duplicates in database. Invalid state. Aborting Insert.");
                }

                SqlCommand insertCmd = new SqlCommand(query, conn);
                insertCmd.ExecuteNonQuery();
            }




        }
    }
}
