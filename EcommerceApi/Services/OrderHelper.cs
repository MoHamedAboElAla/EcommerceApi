namespace EcommerceApi.Services
{
    public class OrderHelper
    {

        public static decimal ShippingFee { get; } = 5;

        public static Dictionary<string, string> PaymentMethods { get; } = new() {

            {"Cash" , "Cash on Delivery" },
            {"Paypal" , "Paypal"},
            {"Credit Card" , "Credit Card"}
        };

        public static List<string> PaymentStatus { get; } = new() {

            "Pending",
            "Paid",
            "Failed",
            "Refunded",
            "Cancelled",
            "Accepted",
        };

        public static List<string> OrderStatus { get; } = new() {

            "Pending",
            "Processing",
            "Shipped",
            "Delivered",
            "Cancelled",
            "Returned",
            "Created"
        };




        /*
         * Receives a string of product identifiers separated by a dash (-) and 
         * returns a dictionary with the product id as the key and the quantity as the value.
         * Example: "1-2-3-4-5-1-2" returns {1: 2, 2: 2, 3: 1, 4: 1, 5: 1}
         * returns a list of pairs (dictionary)
         *   - the pair name is productId
         *   - the pair value is the product quantity
        */

        public static Dictionary<int,int> GetProductDictionary(string productIdentifiers)
        {
            var productDictionary = new Dictionary<int, int>();

            if(productIdentifiers.Length > 0)
            {
                string [] productIdArray  = productIdentifiers.Split('-');
                foreach (var productId in productIdArray)
                { 
                
                    try
                    {
                        int id = int.Parse(productId);
                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id]++;
                        }
                        else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return productDictionary;

        }


    }
}
