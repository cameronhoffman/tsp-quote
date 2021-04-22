using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TSPQuote.Data.Models;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TSPQuote.Controllers
{
    class ProgramController
    {
        priceobject _pricing;
        List<product> _products = new List<product>();
        priceobject GetPricing()
        {
            //string pricing_path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "pricingtable.json");
            string pricing_path = System.IO.Directory.GetCurrentDirectory() + "/pricingtable.json";
            StreamReader sr = new StreamReader(pricing_path);
            string pricing_data = sr.ReadToEnd();
            var pricing = JsonConvert.DeserializeObject<priceobject>(pricing_data);
            sr.Close();
            return pricing;
        }
        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public void Begin()
        {
            _pricing = GetPricing();
            bool CancelToken = false;
            while (!CancelToken)
            {
                int nextwindow = Home();
                if (nextwindow == 0) continue;
                else if(nextwindow == 1)
                {
                    AddProduct();
                }
                else if (nextwindow == 2)
                {
                    RemoveProduct();
                }
                else if (nextwindow == 3)
                {
                    GenerateQuote();
                }
                else if (nextwindow == 4)
                {
                    EditProducts();
                }
            }
        }

        public void PrintHeading(List<string> lines, ConsoleColor Color)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("TSPQuote");
            Console.ResetColor();
            Console.WriteLine("-------------------------");
            Console.ForegroundColor = Color;
            foreach(string line in lines)
            {
                Console.WriteLine(line);
            }
            Console.ResetColor();
            Console.WriteLine("-------------------------");
        }

        public void PrintProducts(bool shownumbers)
        {
            int number = 1;
            foreach(product p in _products)
            {
                if (shownumbers)
                {
                    Console.WriteLine("Product #" + number);
                    number++;
                }
                string printsinfo = "";
                foreach(print k in p.Prints)
                {
                    string baseInfo = "";
                    if (k.baseprint) baseInfo = " ($0.25 base print fee)";
                    printsinfo += " " + k.colors + " color(s) " + k.location + baseInfo + " |";
                }
                Console.WriteLine(p.ID + " - " + p.Name + " - " + "| " + printsinfo);
                foreach(quantity q in p.Quantities)
                {
                    Console.WriteLine("Qty " + q.range + ": $" + q.totalprice.ToString("0.00"));
                }
                Console.WriteLine("");
                Console.WriteLine("Setup Fee - $" + p.SetupFee);
                Console.WriteLine("-------------------------");
            }
            Console.WriteLine(" ");
        }

        public float QuantityTotalPrice(string range, product item)
        {
            float totalprice = item.Price + item.Margin; // Calculate total price for quantity
            foreach (print p in item.Prints)
            {
                int index = p.colors - 1;
                print_price pprice = _pricing.prices[index];
                float price = 0;
                switch (range)
                {
                    case "10-20":
                        price = pprice.count_10_20;
                        break;
                    case "21-40":
                        price = pprice.count_21_40;
                        break;
                    case "41-70":
                        price = pprice.count_41_70;
                        break;
                    case "71-150":
                        price = pprice.count_71_150;
                        break;
                    case "151-300":
                        price = pprice.count_151_300;
                        break;
                    case "301-500":
                        price = pprice.count_301_500;
                        break;
                    case "501-1000":
                        price = pprice.count_501_1000;
                        break;
                    case "1001-2000":
                        price = pprice.count_1001_2000;
                        break;
                }
                totalprice += price;
                if (p.baseprint) price += (float)0.25;
            }

            double dtotalprice = Math.Round(totalprice, 2) * 100;
            int checkprice = (int)(dtotalprice);
            while (checkprice % 10 != 0)
            {
                checkprice++;
            }
            totalprice = ((float)checkprice / 100);
            return totalprice;
        }
        public int Home()
        {
            //
            string edit = "";
            if (_products.Count() > 0) edit = "To edit an existing product in the quote, type 'edit'";
            List<string> Lines = new List<string>()
            {
                "To add product to the quote, type 'add'",
                "To remove product from the quote, type 'remove'",
                "To generate the final quote, type 'quote'",
                edit
            };
            PrintHeading(Lines, ConsoleColor.Yellow);
            Console.WriteLine(" ");
            PrintProducts(false);

            string selection = Console.ReadLine();
            if(selection == "add" || selection == "Add" || selection == "ADD")
            {
                return 1;
            }
            else if(selection == "remove" || selection == "Remove" || selection == "REMOVE")
            {
                return 2;
            }
            else if(selection == "quote" || selection == "Quote" || selection == "QUOTE")
            {
                return 3;
            }
            else if(selection == "edit" || selection == "Edit" || selection == "EDIT")
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }

        public void AddProduct()
        {
            // ID
            var item = new product();
            List<string> Lines = new List<string>()
            {
                "Enter the garment ID",
            };
            PrintHeading(Lines, ConsoleColor.Cyan);
            item.ID = "#" + Console.ReadLine();

            // Name
            Lines = new List<string>()
            {
                "Enter the product name",
            };
            PrintHeading(Lines, ConsoleColor.Cyan);
            item.Name = Console.ReadLine();

            // Price
            bool pricedone = false;
            string priceerror = "";
            while (!pricedone)
            {
                float productPrice;
                Lines = new List<string>()
                {
                    "Enter the product price per unit " + priceerror,
                };
                PrintHeading(Lines, ConsoleColor.Green);
                string productPriceInput = Console.ReadLine();
                if (!float.TryParse(productPriceInput, out productPrice))
                {
                    priceerror = "(Invalid entry, please try again)";
                }
                else
                {
                    pricedone = true;
                    item.Price = productPrice;
                }
            }

            // Margin
            bool margindone = false;
            string marginerror = "";
            while (!margindone)
            {
                float marginPrice;
                bool percentMargin = false;
                Lines = new List<string>()
                {
                    "Enter the margin increase:" + marginerror,
                    "Enter as a dollar amount, or a percentage of garment cost by entering a trailing '%'"
                };
                PrintHeading(Lines, ConsoleColor.Green);
                string marginInput = Console.ReadLine();
                if (marginInput.EndsWith("%"))
                {
                    marginInput = marginInput.Remove(marginInput.Length - 1);
                    percentMargin = true;
                }
                if (!float.TryParse(marginInput, out marginPrice))
                {
                    marginerror = "(Invalid entry, please try again)";
                }
                else
                {
                    margindone = true;
                    if (percentMargin)
                    {
                        item.Margin = item.Price * (marginPrice / 100);
                    }
                    else
                    {
                        item.Margin = marginPrice;
                    }
                }
            }


            // Printing Locations / Colors
            string locationInput = "";
            string colorInput = "";
            item.Prints = new List<print>();
            while(colorInput != "done" && locationInput != "done")
            {
                var addprint = new print();
                Lines = new List<string>()
                {
                    "Enter 'done' when you are done entering printing locations",
                    "Add print location",
                    " ",
                    "Enter name of the location of a print "
                };
                PrintHeading(Lines, ConsoleColor.Cyan);
                locationInput = Console.ReadLine();
                if (locationInput == "done") break;
                addprint.location = locationInput;
                string colorError = "";
                bool colorOk = false;
                while (!colorOk)
                {
                    Lines = new List<string>()
                    {
                        "Enter 'done' when you are done entering printing locations",
                        "Add printing location",
                        colorError,
                        "Enter the number of colors to be printed on (" + locationInput + ")"
                    };
                    PrintHeading(Lines, ConsoleColor.Blue);
                    colorInput = Console.ReadLine();
                    int colors = 0;
                    if (int.TryParse(colorInput, out colors))
                    {
                        colorOk = true;
                        addprint.colors = colors;
                        addprint.baseprint = false;
                        if(colors == 1)
                        {
                            bool basePrintValid = false;
                            while (!basePrintValid)
                            {
                                Lines = new List<string>()
                                {
                                    "Does this print require a base?",
                                    "Please type 'yes' or 'no'",
                                    "To cancel and return to menu, type 'done'"
                                };
                                PrintHeading(Lines, ConsoleColor.Yellow);
                                string baseInput = Console.ReadLine();
                                if (baseInput == "yes")
                                {
                                    addprint.baseprint = true;
                                    basePrintValid = true;
                                }
                                else if (baseInput == "no")
                                {
                                    addprint.baseprint = false;
                                    basePrintValid = true;
                                }
                                else if (baseInput == "done") return;
                            }
                            
                        }
                        item.Prints.Add(addprint);
                    }
                    else
                    {
                        colorError = "(Invalid entry for number of colors, please try again)";
                    }
                }
            }

            // Calculate Setup Fee
            float setupFee = 0;
            foreach(print p in item.Prints)
            {
                setupFee += (p.colors * 15);
            }
            item.SetupFee = setupFee;

            // Quantities/Values
            string rangeInput = "";
            string rangeError = "";
            item.Quantities = new List<quantity>();
            while (rangeInput != "done")
            {
                Lines = new List<string>()
                {
                    "Select quantity ranges to be displayed in quote (Enter number matching your selection)",
                    "When you are finished selecting, enter 'done'"
                };
                PrintHeading(Lines, ConsoleColor.Cyan);
                if(item.Quantities.Where(x => x.range == "10-20").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("1) 10-20");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "21-40").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("2) 21-40");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "41-70").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("3) 41-70");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "71-150").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("4) 71-150");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "151-300").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("5) 151-300");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "301-500").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("6) 301-500");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "501-1000").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("7) 501-1000");
                Console.ResetColor();
                if (item.Quantities.Where(x => x.range == "1001-2000").Any()) Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("8) 1001-2000");
                Console.ResetColor();
                Console.WriteLine("-------------------------");

                rangeInput = Console.ReadLine();
                if (rangeInput == "done") break;
                int range;
                if (!int.TryParse(rangeInput, out range))
                {
                    rangeError = "(Invalid range entered, please try again)";
                    continue;
                }
                string rangeString = "";
                switch (range)
                {
                    case 1:
                        rangeString = "10-20";
                        break;
                    case 2:
                        rangeString = "21-40";
                        break;
                    case 3:
                        rangeString = "41-70";
                        break;
                    case 4:
                        rangeString = "71-150";
                        break;
                    case 5:
                        rangeString = "151-300";
                        break;
                    case 6:
                        rangeString = "301-500";
                        break;
                    case 7:
                        rangeString = "501-1000";
                        break;
                    case 8:
                        rangeString = "1001-2000";
                        break;
                }

                var quantity = new quantity()
                {
                    range = rangeString,
                    totalprice = QuantityTotalPrice(rangeString, item)
                };
                if(quantity.range != "")
                {
                    item.Quantities.Add(quantity);
                }
            }
            _products.Add(item);
        }

        public void RemoveProduct()
        {
            if (_products.Count() == 0) return;
            List<string> Lines = new List<string>()
            {
                "Enter the number of the product you wish to remove from the quote"
            };
            PrintHeading(Lines, ConsoleColor.Red);
            Console.WriteLine(" ");
            PrintProducts(true);
            string input = Console.ReadLine();
            int index;
            if (int.TryParse(input, out index))
            {
                index -= 1;
                _products.Remove(_products[index]);
            }
        }

        public void GenerateQuote()
        {
            List<string> Lines = new List<string>()
            {
                "Below is the generated quote from the information entered",
                "To go back to the home menu, press enter",
            };
            PrintHeading(Lines, ConsoleColor.White);
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            PrintProducts(false);
            Console.WriteLine("Art Fee: $25 **if art is provided in full vector format (EPS, PDF or AI) you can avoid this fee");
            Console.WriteLine("");
            Console.WriteLine("Quote is valid for 30 days.");
            Console.WriteLine("");
            Console.WriteLine("Production time is generally 1-1.5 week(s) from date order is placed & paid for. ");
            Console.WriteLine("All payments are due in advance of at least half down and balance at pick up.");
            Console.WriteLine("If you are tax exempt, please provide all paperwork prior to payment being made. Once payment is made, we can not refund tax. ");
            Console.WriteLine("");
            Console.WriteLine("If you have any questions, feel free to reach out. We look forward to working with you.");
            Console.ReadLine();
            Console.ResetColor();
        }

        public void EditProducts()
        {
            if (_products.Count() == 0) return;
            List<string> Lines = new List<string>()
            {
                "Enter the number of the product you wish to edit"
            };
            PrintHeading(Lines, ConsoleColor.Yellow);
            Console.WriteLine(" ");
            PrintProducts(true);
            string input = Console.ReadLine();
            int index;
            if (int.TryParse(input, out index))
            {
                index -= 1;
                EditProduct(_products[index]);
            }
        }

        public void EditProduct(product product)
        {
            List<string> Lines = new List<string>()
            {
                "Edit Product (" + product.Name + ")",
                "1) ID",
                "2) Name",
                "3) Price",
                "4) Margin"
            };
            PrintHeading(Lines, ConsoleColor.Yellow);
            Console.WriteLine(" ");
            string input = Console.ReadLine();
            int index;
            if (!int.TryParse(input, out index))
            {
                // Error Handle
            }
            if(index == 1)
            {
                Lines = new List<string>()
                {
                    "Enter the new garment ID",
                };
                PrintHeading(Lines, ConsoleColor.Cyan);
                product.ID = "#" + Console.ReadLine();
            }
            else if(index == 2)
            {
                Lines = new List<string>()
                {
                    "Enter the new product name",
                };
                PrintHeading(Lines, ConsoleColor.Cyan);
                product.Name = Console.ReadLine();
            }
            else if(index == 3)
            {
                bool pricedone = false;
                string priceerror = "";
                while (!pricedone)
                {
                    float productPrice;
                    Lines = new List<string>()
                    {
                        "Enter the new product price per unit " + priceerror,
                    };
                    PrintHeading(Lines, ConsoleColor.Green);
                    string productPriceInput = Console.ReadLine();
                    if (!float.TryParse(productPriceInput, out productPrice))
                    {
                        priceerror = "(Invalid entry, please try again)";
                    }
                    else
                    {
                        pricedone = true;
                        product.Price = productPrice;
                    }
                }
            }
            else if(index == 4) // Edit Margin
            {
                bool margindone = false;
                string marginerror = "";
                while (!margindone)
                {
                    float marginPrice;
                    bool percentMargin = false;
                    Lines = new List<string>()
                    {
                        "Enter the new margin increase:" + marginerror,
                        "Enter as a dollar amount, or a percentage of garment cost by entering a trailing '%'"
                    };
                    PrintHeading(Lines, ConsoleColor.Green);
                    string marginInput = Console.ReadLine();
                    if (marginInput.EndsWith("%"))
                    {
                        marginInput = marginInput.Remove(marginInput.Length - 1);
                        percentMargin = true;
                    }
                    if (!float.TryParse(marginInput, out marginPrice))
                    {
                        marginerror = "(Invalid entry, please try again)";
                    }
                    else
                    {
                        margindone = true;
                        if (percentMargin)
                        {
                            product.Margin = product.Price * (marginPrice / 100);
                        }
                        else
                        {
                            product.Margin = marginPrice;
                        }
                    }
                }
                foreach(quantity q in product.Quantities)
                {
                    q.totalprice = QuantityTotalPrice(q.range, product);
                }
            }
        }
    }
}
