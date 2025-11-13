
using Microsoft.Data.SqlClient;

namespace Ecom.BLL.Mapper
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {


            CreateMap<ProductImageUrl, GetProductImageUrlVM>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Title : null));

            CreateMap<CreateProductImageUrlVM, ProductImageUrl>()
                .ConstructUsing(vm => new ProductImageUrl(vm.ImageUrl!, vm.ProductId, vm.CreatedBy!));

            CreateMap<UpdateProductImageUrlVM, ProductImageUrl>()
                .ConstructUsing(vm => new ProductImageUrl(vm.ImageUrl!, vm.ProductId, vm.UpdatedBy!))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<ProductImageUrl, DeleteProductImageUrlVM>().ReverseMap();

            //Brand Mappings
           
            CreateMap<Brand, GetBrandVM>().ReverseMap();

            CreateMap<CreateBrandVM, Brand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpdateBrandVM, Brand>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeleteBrandVM, Brand>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ReverseMap();


            // User Mappings
            // Maps from the RegisterUserVM to the AppUser entity
            CreateMap<RegisterUserVM, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

            // Maps from the AppUser entity to the GetUserVM
            CreateMap<AppUser, GetUserVM>();

            // 3. Map for Updating (Special Case)
            // This tells AutoMapper how to apply an UpdateDto *onto* an
            // existing AppUser object, ignoring any null values from the DTO.
            CreateMap<UpdateUserVM, AppUser>()
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
                

        }

    }
}
