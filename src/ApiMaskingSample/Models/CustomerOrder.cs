namespace ApiMaskingSample.Models
{
    public class CustomerOrder
    {
        public string Id { get; set; }

        public DateTime CreateDate { get; set; }

        public string? Product { get; set; }

        public int Quantity { get; set; }

        public decimal Balance { get; set; }

        public Customer? Customer { get; set; }
    }

    public class Customer
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SSN { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// primary account number
        /// </summary>
        public string? PAN { get; set; }
    }
}