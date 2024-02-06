using System;
using System.Collections.Generic;
using System.Linq;
using Task1.DoNotChange;

namespace Task1
{
    public static class LinqTask
    {
        /// <summary>
        /// Select the customers whose total turnover (the sum of all orders) exceeds a certain value. 
        /// </summary>
        /// <param name="customers"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static IEnumerable<Customer> Linq1(IEnumerable<Customer> customers, decimal limit)
        {
            var result = from customer in customers
                         where customer.Orders.Sum(order => order.Total) > limit
                         select customer;

            var result2 = customers
                .Where(customer => customer.Orders.Sum(order => order.Total) > limit);

            return result;
        }

        /// <summary>
        /// For each customer make a list of suppliers located in the same country and the same city.
        /// </summary>
        /// <param name="customers"></param>
        /// <param name="suppliers"></param>
        /// <returns></returns>
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers
        )
        {
            var result = from customer in customers
                        join supplier in suppliers
                        on new { customer.Country, customer.City } equals new { supplier.Country, supplier.City }
                        into customSuppliers
                        select (customer, customSuppliers);

            return result;
        }

        /// <summary>
        /// Grouping
        /// </summary>
        /// <param name="customers"></param>
        /// <param name="suppliers"></param>
        /// <returns></returns>
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2UsingGroup(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers
        )
        {

            var result = from customer in customers
                         join supplier in suppliers
                         on new { customer.Country, customer.City } equals new { supplier.Country, supplier.City }
                         into customSuppliers
                         from supplier in customSuppliers.DefaultIfEmpty()
                         group supplier by customer into g
                         select (g.Key, g.Select(s => s).Where(s => s != null));
            return result;
        }

        /// <summary>
        ///  Find all customers with the sum of all orders that exceed a certain value. 
        /// </summary>
        /// <param name="customers"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<Customer> Linq3(IEnumerable<Customer> customers, decimal limit)
        {

            var result = from customer in customers
                         where customer.Orders != null && customer.Orders.Any(order => order.Total > limit)
                         select customer;

            return result;
        }

        /// <summary>
        /// Select the clients, including the date of their first order. 
        /// </summary>
        /// <param name="customers"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq4(
            IEnumerable<Customer> customers
        )
        {
            var result = from customer in customers
                         let firstOrderDate = customer.Orders.Any() ? customer.Orders.Min(order => order.OrderDate) : (DateTime?)null
                         where firstOrderDate.HasValue
                         select (
                             customer,
                             dateOfEntry: firstOrderDate ?? DateTime.MinValue
                         );

            return result;
        }

        /// <summary>
        /// Repeat the previous query but order the result by year, month, turnover (descending) and customer name. 
        /// </summary>
        /// <param name="customers"></param>
        /// <returns></returns>
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq5(
            IEnumerable<Customer> customers
        )
        {
            var result = customers
                .Where(customer => customer.Orders.Any(order => order.Total != null))
                .Select(customer =>
            {
                var firstOrderDate = customer.Orders.Any() ? customer.Orders.Min(order => order.OrderDate) : (DateTime?)null;
                return (customer, dateOfEntry: firstOrderDate ?? DateTime.MinValue);
            })
                 .OrderBy(tuple => tuple.dateOfEntry.Year)
                 .ThenBy(tuple => tuple.dateOfEntry.Month)
                 .ThenByDescending(tuple => tuple.customer.Orders.Any() ? tuple.customer.Orders.Sum(order => order.Total) : 0)
                 .ThenBy(tuple => tuple.customer.CompanyName);

            return result;
        }

        /// <summary>
        /// Select the clients which either have:
        ///a.non-digit postal codE
        ///b.undefined region
        ///c.operator code in the phone is not specified (does not contain parentheses)
        /// </summary>
        /// <param name="customers"></param>
        /// <returns></returns>
        public static IEnumerable<Customer> Linq6(IEnumerable<Customer> customers)
        {
            return customers.Where(customer =>
               (!string.IsNullOrEmpty(customer.PostalCode) && customer.PostalCode.Any(c => !char.IsDigit(c))) ||
               string.IsNullOrEmpty(customer.Region) ||
               (!string.IsNullOrEmpty(customer.Phone) && (!customer.Phone.Contains("(") || !customer.Phone.Contains(")"))));
        }

        /// <summary>
        /// Group the products by category, then by availability in stock with ordering by cost. 
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<Linq7CategoryGroup> Linq7(IEnumerable<Product> products)
        {
            var result = products
                 .GroupBy(product => product.Category) // Group products by category
                 .Select(categoryGroup => new Linq7CategoryGroup
                 {
                     Category = categoryGroup.Key,
                     UnitsInStockGroup = categoryGroup
                         .GroupBy(product => product.UnitsInStock) // Group products within each category by units in stock
                         .OrderBy(unitsGroup => unitsGroup.Key) // Order by units in stock
                         .Select(unitsGroup => new Linq7UnitsInStockGroup
                         {
                             UnitsInStock = unitsGroup.Key,
                             Prices = unitsGroup
                                 .OrderBy(product => product.UnitPrice) // Order by unit price
                                 .Select(product => product.UnitPrice)
                         })
                 });

            return result;
        }

        /// <summary>
        /// Group the products by “cheap”, “average” and “expensive” following the rules:
        ///  From 0 to cheap inclusive
        ///  From cheap exclusive to average inclusive
        ///  From average exclusive to expensive inclusive 
        /// </summary>
        /// <param name="products"></param>
        /// <param name="cheap"></param>
        /// <param name="middle"></param>
        /// <param name="expensive"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<(decimal category, IEnumerable<Product> products)> Linq8(
            IEnumerable<Product> products,
            decimal cheap,
            decimal middle,
            decimal expensive
        )
        {
            var result = products.GroupBy(p =>
            {
                if (p.UnitPrice <= cheap)
                    return cheap;
                else if (p.UnitPrice <= middle)
                    return middle;
                else
                    return expensive;
            }).OrderBy(g => g.Key);

            return result.Select(g => ((decimal)g.Key, (IEnumerable<Product>)g));
        }

        /// <summary>
        /// Calculate the average profitability of each city (average amount of orders per customer) and average rate (average number of orders per customer from each city). 
        /// </summary>
        /// <param name="customers"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<(string city, int averageIncome, int averageIntensity)> Linq9(
            IEnumerable<Customer> customers
        )
        {
            var result = from customer in customers
                         group customer by customer.City into cityGroup
                         select (
                             city: cityGroup.Key,
                             averageIncome: (int)Math.Round(cityGroup.Average(c => c.Orders.Sum(o => o.Total))),
                             averageIntensity: (int)Math.Round((double)cityGroup.Sum(c => c.Orders.Length) / cityGroup.Count())
                         );

            return result;
        }

        /// <summary>
        /// Build a string of unique supplier country names, sorted first by length and then by country.
        /// </summary>
        /// <param name="suppliers"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string Linq10(IEnumerable<Supplier> suppliers)
        {
            var uniqueCountries = suppliers.Select(s => s.Country).Distinct().OrderBy(c => c.Length).ThenBy(c => c);
            // Such approach will be better
            // return string.Join(", ", uniqueCountries);
            return string.Join("",uniqueCountries);
        }
    }
}
