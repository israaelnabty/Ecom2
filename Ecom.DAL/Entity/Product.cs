
namespace Ecom.DAL.Entity
{
    public class Product
    {
        [Key]
        public int Id { get; private set; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public decimal Rating { get; private set; }
        public int Stock { get; private set; }
        public int QuantitySold { get; private set; }
        public string? ThumbnailUrl { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public DateTime? DeletedOn { get; private set; }
        public string? DeletedBy { get; private set; }
        public DateTime? UpdatedOn { get; private set; }
        public string? UpdatedBy { get; private set; }
        public bool IsDeleted { get; private set; }

        // Foriegn Keys
        [ForeignKey("Brand")]
        public int BrandId { get; private set; }

        [ForeignKey("Category")]
        public int CategoryId { get; private set; }

        // Navigation Properties
        public virtual Brand? Brand { get; private set; }
        public virtual Category? Category { get; private set; }
        public virtual ICollection<ProductImageUrl>? ProductImageUrls { get; private set; }
        public virtual ICollection<ProductReview>? ProductReviews { get; private set; }
        public virtual ICollection<WishlistItem>? WishlistItems { get; private set; }

        // Logic
        public Product() { }
        public Product(string title, string description, decimal price, decimal discountPercentage, int stock,
            string thumbnailUrl, string createdBy, int brandId, int categoryId, int quantitySold)
        {
            Title = title;
            Description = description;
            Price = price;
            DiscountPercentage = discountPercentage;
            Stock = stock;
            ThumbnailUrl = thumbnailUrl;
            CreatedBy = createdBy;
            CreatedOn = DateTime.UtcNow;
            IsDeleted = false;
            BrandId = brandId;
            CategoryId = categoryId;
            QuantitySold = quantitySold;
        }

        public bool Update(string? title, string description, decimal price, decimal discountPercentage, int stock,
            string thumbnailUrl, string userModified, int brandId, int categoryId, int quantitySold)
        {
            if (!string.IsNullOrEmpty(userModified))
            {
                Title = title;
                Description = description;
                Price = price;
                DiscountPercentage = discountPercentage;
                Stock = stock;
                ThumbnailUrl = thumbnailUrl;
                BrandId = brandId;
                CategoryId = categoryId;
                UpdatedOn = DateTime.UtcNow;
                UpdatedBy = userModified;
                QuantitySold = quantitySold;
                return true;
            }
            return false;
        }
        public bool ToggleDelete(string userModified)
        {
            if (!string.IsNullOrEmpty(userModified))
            {
                IsDeleted = !IsDeleted;
                DeletedOn = DateTime.UtcNow;
                DeletedBy = userModified;
                return true;
            }
            return false;
        }

    }
}
